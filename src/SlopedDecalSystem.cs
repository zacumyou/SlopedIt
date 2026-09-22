using System;
using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SlopedIt
{
    /// <summary>Edits creation definitions before vanilla generates the visible Temp objects.</summary>
    public partial class SlopedDecalSystem : GameSystemBase
    {
        private EntityQuery m_Definitions;
        private TerrainSystem m_Terrain;
        private ToolSystem m_Tools;
        private PrefabSystem m_Prefabs;
        private long m_Aligned;
        private bool m_LoggedFirst;

        private struct HeightSampler : IHeightSampler
        {
            public TerrainHeightData Data;
            public float Height(float3 position) => TerrainUtils.SampleHeight(ref Data, position);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Terrain = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_Tools = World.GetOrCreateSystemManaged<ToolSystem>();
            m_Prefabs = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_Definitions = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadWrite<ObjectDefinition>(), ComponentType.ReadOnly<CreationDefinition>(), ComponentType.ReadOnly<Updated>() },
                None = new[] { ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Overridden>(), ComponentType.ReadOnly<OwnerDefinition>() }
            });
            RequireForUpdate(m_Definitions);
        }

        protected override void OnUpdate()
        {
            if (!Mod.Active || !(Mod.Options?.Enabled ?? true)) return;
            var tool = m_Tools.activeTool;
            if (tool == null) return;
            // Find It / Anarchy use ObjectToolSystem. The external Line Tool emits the same definitions.
            if (!(tool is ObjectToolSystem) && tool.toolID != "Line Tool") return;
            try
            {
                Dependency.Complete();
                // CPU reads finish synchronously here, so no outstanding terrain-reader job remains.
                var sampler = new HeightSampler { Data = m_Terrain.GetHeightData() };
                if (!sampler.Data.isCreated || sampler.Data.heights.Length < 4) return;
                using var entities = m_Definitions.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var creation = EntityManager.GetComponentData<CreationDefinition>(entity);
                    if (creation.m_Original != Entity.Null || creation.m_Owner != Entity.Null ||
                        creation.m_Attached != Entity.Null ||
                        (creation.m_Flags & (CreationFlags.Delete | CreationFlags.Upgrade | CreationFlags.Relocate | CreationFlags.Attach)) != 0) continue;
                    var definition = EntityManager.GetComponentData<ObjectDefinition>(entity);
                    if (definition.m_ParentMesh >= 0) continue;
                    if (!TryFootprint(creation, definition, out float2 min, out float2 max)) continue;
                    if (!SlopeMath.TryAlign(ref sampler, definition.m_Position, definition.m_Rotation, min, max, out var rotation)) continue;
                    definition.m_Rotation = rotation;
                    definition.m_LocalRotation = rotation; // only standalone definitions reach here
                    EntityManager.SetComponentData(entity, definition);
                    m_Aligned++;
                    if (!m_LoggedFirst)
                    {
                        m_LoggedFirst = true;
                        Mod.Log.Info($"First decal preview aligned. Prefab={creation.m_Prefab}; footprint={max - min}; position={definition.m_Position}");
                    }
                }
            }
            catch (Exception error)
            {
                Mod.Active = false;
                Mod.Status = "Stopped after error / 오류로 중지";
                Mod.Log.Error(error, "Sloped It stopped; see log. Existing placed objects were not scanned or rewritten.");
            }
        }

        private bool TryFootprint(CreationDefinition creation, ObjectDefinition definition, out float2 min, out float2 max)
        {
            min = max = default;
            var prefab = creation.m_Prefab;
            if (prefab == Entity.Null || EntityManager.HasComponent<BuildingData>(prefab) ||
                EntityManager.HasComponent<TreeData>(prefab)) return false;
            if (EntityManager.HasComponent<PlaceableObjectData>(prefab))
            {
                var flags = EntityManager.GetComponentData<PlaceableObjectData>(prefab).m_Flags;
                if ((flags & (PlacementFlags.Wall | PlacementFlags.Hanging | PlacementFlags.Floating | PlacementFlags.RoadSide |
                    PlacementFlags.RoadEdge | PlacementFlags.RoadNode | PlacementFlags.NetObject | PlacementFlags.Attached)) != 0) return false;
            }
            // Functional prefabs (parking lanes, networks, child objects) need their own alignment policy.
            if (EntityManager.HasBuffer<Game.Prefabs.SubLane>(prefab) && EntityManager.GetBuffer<Game.Prefabs.SubLane>(prefab, true).Length != 0) return false;
            if (EntityManager.HasBuffer<SubNet>(prefab) && EntityManager.GetBuffer<SubNet>(prefab, true).Length != 0) return false;
            if (EntityManager.HasBuffer<Game.Prefabs.SubObject>(prefab) && EntityManager.GetBuffer<Game.Prefabs.SubObject>(prefab, true).Length != 0) return false;

            // Direct render-prefab placement in the editor uses m_SubPrefab + m_Scale.
            if (creation.m_SubPrefab != Entity.Null)
            {
                if (!IsTerrainDecal(creation.m_SubPrefab)) return false;
                var bounds = EntityManager.GetComponentData<MeshData>(creation.m_SubPrefab).m_Bounds;
                var a = bounds.min.xz * definition.m_Scale.xz;
                var b = bounds.max.xz * definition.m_Scale.xz;
                min = math.min(a, b); max = math.max(a, b);
                return true;
            }
            if (!EntityManager.HasBuffer<SubMesh>(prefab) || !EntityManager.HasComponent<ObjectGeometryData>(prefab)) return false;
            var meshes = EntityManager.GetBuffer<SubMesh>(prefab, true);
            if (meshes.Length == 0) return false;
            foreach (var mesh in meshes)
            {
                if (!IsTerrainDecal(mesh.m_SubMesh)) return false;
                // Sideways projection meshes are wall decals even without a Wall placement flag.
                if (math.dot(math.rotate(mesh.m_Rotation, math.up()), math.up()) < 0.999f) return false;
            }
            var geometry = EntityManager.GetComponentData<ObjectGeometryData>(prefab);
            min = geometry.m_Bounds.min.xz;
            max = geometry.m_Bounds.max.xz;
            return true;
        }

        private bool IsTerrainDecal(Entity mesh)
        {
            if (!EntityManager.HasComponent<MeshData>(mesh)) return false;
            var data = EntityManager.GetComponentData<MeshData>(mesh);
            if ((data.m_State & MeshFlags.Decal) == 0 || !m_Prefabs.TryGetPrefab<RenderPrefab>(mesh, out var render)) return false;
            // MeshData.m_DecalLayer describes the mesh's receiving layer, not this decal's projection mask.
            var decal = render.GetComponent<DecalProperties>();
            return decal != null && (decal.m_LayerMask & DecalLayers.Terrain) != 0;
        }

        public void Stop()
        {
            Enabled = false;
            Mod.Log.Info($"Sloped It stopped. Aligned creation definitions: {m_Aligned}");
        }
    }
}
