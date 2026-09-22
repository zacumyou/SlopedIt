# Sloped It

Automatically align new terrain decals to slopes in Cities: Skylines II, including the hover preview.

**Release:** 1.0.1 · **Paradox Mods compatibility:** 1.6.*

I wanted to stamp decals onto hills. Tilting each one was shaving years off my life. Too much work. So I made a mod.

## Features

- Samples terrain across the decal footprint.
- Aligns the preview before placement.
- Preserves heading and placement height.
- Includes an English/Korean settings toggle under Options → Sloped It.
- Supports imported decal packs such as RealVision Decals with force alignment enabled by default.
- Leaves existing objects unchanged.

Only standalone ground-facing decals are supported. Wall/attached decals and functional composite prefabs are excluded. This adjusts the decal's projection rotation; it does not deform its mesh. Sharp terrain changes may exceed the projection volume.

## Bug reports

Use this repository's Issues or the linked Paradox Forum thread. Include game/mod versions, reproduction steps, a screenshot and the SlopedIt log from the game's user-data `Logs` folder.

## Build

Install the official Cities: Skylines II modding toolchain. Its `CSII_*` environment variables must be available. Run `build.ps1` from PowerShell. It builds via the official Mod.props, Mod.targets and ModPostProcessor, then runs the numerical tests. Output is staged in `local/SlopedIt`; the build does not install or publish.

Run `install.ps1` with the game closed to back up an existing local installation and install the staged output. Keep only one active local/subscribed copy of the mod.

The reviewed game build is 1.6.2f1, Game.dll SHA-256 `AAEE15C4FA41C130ABAA1183840667E4FAAE531D67E8A9FA7536618EFFA2F86A`. The runtime guard intentionally stops on an unreviewed build. The listing's 1.6.* range is not a promise that future patches have already been tested.

The user confirmed the existing feature works in game. Version 1.0.1 adds receiver-mask-independent alignment. Build and numerical tests passed; this change has not yet been verified in game.

Official guidance: [Modding Toolchain](https://cs2.paradoxwikis.com/Modding_Toolchain), [Options UI](https://cs2.paradoxwikis.com/Options_UI), [Decals](https://cs2.paradoxwikis.com/Asset_Pipeline:_Decals).

---

# Sloped It — 한국어

시티즈: 스카이라인 II에서 새 지형 데칼을 경사에 맞춥니다. 배치 전 미리보기에도 적용됩니다.

**버전:** 1.0.1 · **Paradox Mods 호환 표기:** 1.6.*

데칼을 경사면에 새기고 싶었습니다. 하나씩 기울이다 제 인생이 먼저 깎일 것 같았습니다. 귀찮아서 모드로 만들었습니다.

## 주요 기능

- 데칼 크기 범위의 지형을 샘플링합니다.
- 배치 전 미리보기부터 경사를 적용합니다.
- 회전 방향과 높이를 유지합니다.
- 옵션 → Sloped It에서 자동 정렬을 켜고 끕니다.
- RealVision Decals 같은 외부 데칼도 기본 활성화된 강제 정렬로 지원합니다.
- 기존 오브젝트는 변경하지 않습니다.

독립 배치하는 지형용 데칼 전용입니다. 벽면·부착형·기능성 복합 에셋은 제외합니다. 메시를 휘게 만드는 기능은 아닙니다. 급격히 꺾인 지형에서는 일부가 잘릴 수 있습니다.

## 오류 제보

GitHub Issues 또는 연결된 Paradox Forum 글을 이용해 주세요. 게임·모드 버전, 재현 방법, 스크린샷과 게임 사용자 데이터의 `Logs` 폴더에 있는 SlopedIt 로그를 첨부해 주세요.

## 빌드

공식 모딩 도구를 설치한 뒤 PowerShell에서 `build.ps1`을 실행하세요. `CSII_*` 환경 변수가 필요합니다. 결과는 `local/SlopedIt`에 생성됩니다. 게임을 종료하고 `install.ps1`을 실행하면 기존 로컬 설치를 백업한 뒤 설치합니다. 로컬판과 구독판을 동시에 활성화하지 마세요.

실제 검토한 게임은 1.6.2f1입니다. 미검토 게임 빌드는 호환성 보호 검사로 중지합니다. 1.6.* 표기가 앞으로 나올 모든 패치의 시험 완료를 뜻하지는 않습니다.
