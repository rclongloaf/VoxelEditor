using System.Collections.Generic;
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
            HashSet<Vector3Int> voxels,
            SpriteRectData spriteRectData,
            Texture2D? texture,
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