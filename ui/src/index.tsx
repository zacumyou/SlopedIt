import React from "react";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ModRegistrar } from "cs2/modding";
import groundIcon from "./stick-to-ground.svg";
const id = "SlopedIt";
const visible$ = bindValue<boolean>(id, "Visible", false);
const enabled$ = bindValue<boolean>(id, "Enabled", false);
const decal$ = bindValue<boolean>(id, "Decal", true);
const panel = "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx";
const register: ModRegistrar = registry => {
    const Loc = registry.get("game-ui/common/localization/loc.generated.ts", "Loc");
    const ToolButton = registry.get("game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton");
    const theme = registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss", "classes");
    const AlignmentButton = () => {
        const visible = useValue(visible$);
        const enabled = useValue(enabled$);
        const decal = useValue(decal$);
        const { translate } = useLocalization();
        if (!visible) return null;
        return <ToolButton src={groundIcon} selected={enabled} multiSelect={true}
            disabled={false} className={theme.button}
            tooltip={translate(id + (decal ? ".DecalDescription" : ".PropDescription"), "Align with the terrain slope")}
            onSelect={() => trigger(id, "Toggle")} />;
    };
    // Match the native localization component, not rendered text or a tutorial-only tag.
    // Append inside its horizontal button container, preserving vanilla and mod controls.
    registry.extend(panel, "Section", (Original: any) => (props: any) => {
        if (!React.isValidElement(props.title) || props.title.type !== Loc.Toolbar.SNAPPING_TITLE ||
            !React.isValidElement<any>(props.children)) return <Original {...props} />;
        const row = props.children;
        return <Original {...props}>{React.cloneElement(row, {},
            ...React.Children.toArray(row.props.children), <AlignmentButton key={id} />)}</Original>;
    });
};
export default register;
