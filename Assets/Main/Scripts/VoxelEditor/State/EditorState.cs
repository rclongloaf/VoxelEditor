using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Brush;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public abstract record EditorState
    {
        public record WaitingForProject : EditorState;

        public record SpriteSelecting(
            Texture2D texture
        ) : EditorState;

        public record Loaded(
            VoxData voxData,
            Texture2D? texture,
            SpriteIndex currentSpriteIndex,
            SpriteData currentSpriteData,
            SpriteData? bufferedSpriteData,
            BrushData brushData,
            ShaderData shaderData,
            Stack<EditAction> actionsHistory,
            Stack<EditAction> canceledActionsHistory,
            FreeCameraData freeCameraData,
            IsometricCameraData isometricCameraData,
            CameraType cameraType,
            ControlState controlState,
            EditModeState editModeState,
            UIState uiState
        ) : EditorState;
    }
}