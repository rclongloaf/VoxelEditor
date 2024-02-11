using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State.Vox
{
public interface VoxLayerState
{
    public record Init : VoxLayerState;

    public record SpriteSelecting(
        Texture2D texture
    ) : VoxLayerState;

    public record Loaded(
        bool isVisible,
        VoxData voxData,
        Texture2D? texture,
        SpriteIndex currentSpriteIndex,
        SpriteData currentSpriteData,
        SelectionState selectionState,
        Stack<EditAction> actionsHistory,
        Stack<EditAction> canceledActionsHistory
    ) : VoxLayerState;
}
}