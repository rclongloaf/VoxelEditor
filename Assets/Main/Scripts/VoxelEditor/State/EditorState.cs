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
            BrushType brushType,
            FreeCameraData freeCameraData,
            IsometricCameraData isometricCameraData,
            CameraType cameraType,
            ControlState controlState,
            EditModeState editModeState,
            bool isFileBrowserOpened
        ) : EditorState(isFileBrowserOpened);
    }
}