using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public abstract class EditorState
    {
        public readonly bool isFileBrowserOpened;
        
        public EditorState(
            bool isFileBrowserOpened
        )
        {
            this.isFileBrowserOpened = isFileBrowserOpened;
        }
        
        public class WaitingForProject : EditorState
        {
            public WaitingForProject(bool isFileBrowserOpened) : base(isFileBrowserOpened) { }
        }

        public class SpriteSelecting : EditorState
        {
            public readonly Texture2D texture;

            public SpriteSelecting(Texture2D texture, bool isFileBrowserOpened) : base(isFileBrowserOpened)
            {
                this.texture = texture;
            }
        }

        public class Loaded : EditorState
        {
            public readonly HashSet<Vector3Int> voxels;
            public readonly SpriteRectData spriteRectData;
            public readonly Texture2D? texture;
            public readonly BrushType brushType;
            public readonly FreeCameraData freeCameraData;
            public readonly IsometricCameraData isometricCameraData;
            public readonly CameraType cameraType;
            public readonly ControlState controlState;

            public Loaded(
                HashSet<Vector3Int> voxels,
                SpriteRectData spriteRectData,
                Texture2D? texture,
                BrushType brushType,
                FreeCameraData freeCameraData,
                IsometricCameraData isometricCameraData,
                CameraType cameraType,
                ControlState controlState,
                bool isFileBrowserOpened
            ) : base(isFileBrowserOpened)
            {
                this.voxels = voxels;
                this.spriteRectData = spriteRectData;
                this.texture = texture;
                this.brushType = brushType;
                this.freeCameraData = freeCameraData;
                this.isometricCameraData = isometricCameraData;
                this.cameraType = cameraType;
                this.controlState = controlState;
            }

            public Loaded Copy(
                HashSet<Vector3Int>? voxels = null,
                SpriteRectData? spriteRectData = null,
                Texture2D? texture = null,
                BrushType? brushType = null,
                FreeCameraData? freeCameraData = null,
                IsometricCameraData? isometricCameraData = null,
                CameraType? cameraType = null,
                ControlState? controlState = null,
                bool? isFileBrowserOpened = null
            )
            {
                return new Loaded(
                    voxels: voxels ?? this.voxels,
                    spriteRectData: spriteRectData ?? this.spriteRectData,
                    texture: texture ? texture : this.texture,
                    brushType: brushType ?? this.brushType,
                    freeCameraData: freeCameraData ?? this.freeCameraData,
                    isometricCameraData: isometricCameraData ?? this.isometricCameraData,
                    cameraType: cameraType ?? this.cameraType,
                    controlState: controlState ?? this.controlState,
                    isFileBrowserOpened: isFileBrowserOpened ?? this.isFileBrowserOpened
                );
            }
        }
    }
}