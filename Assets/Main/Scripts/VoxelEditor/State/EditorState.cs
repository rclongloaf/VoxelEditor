using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public abstract record EditorState(bool isFileBrowserOpened)
    {
        public record WaitingForProject(bool isFileBrowserOpened) : EditorState(isFileBrowserOpened);

        public record SpriteSelecting(
            bool isFileBrowserOpened,
            Texture2D texture
        ) : EditorState(isFileBrowserOpened);

        public record Loaded(
            VoxData voxData,
            Texture2D? texture,
            SpriteIndex currentSpriteIndex,
            SpriteData currentSpriteData,
            SpriteData? bufferedSpriteData,
            BrushType brushType,
            FreeCameraData freeCameraData,
            IsometricCameraData isometricCameraData,
            CameraType cameraType,
            ControlState controlState,
            EditModeState editModeState,
            bool isWaitingForApplyChanges,
            bool isFileBrowserOpened
        ) : EditorState(isFileBrowserOpened);
    }
}