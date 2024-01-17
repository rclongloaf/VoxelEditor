using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public interface EditorState
    {
        public class WaitingForProject : EditorState { }

        public class Loaded : EditorState
        {
            public readonly HashSet<Vector3Int> voxels;
            public readonly BrushType brushType;
            public readonly FreeCameraData freeCameraData;
            public readonly IsometricCameraData isometricCameraData;
            public readonly CameraType cameraType;
            public readonly bool isFileBrowserOpened;
            public readonly ControlState controlState;

            public Loaded(
                HashSet<Vector3Int> voxels,
                BrushType brushType,
                FreeCameraData freeCameraData,
                IsometricCameraData isometricCameraData,
                CameraType cameraType,
                bool isFileBrowserOpened,
                ControlState controlState
            )
            {
                this.voxels = voxels;
                this.brushType = brushType;
                this.freeCameraData = freeCameraData;
                this.isometricCameraData = isometricCameraData;
                this.cameraType = cameraType;
                this.isFileBrowserOpened = isFileBrowserOpened;
                this.controlState = controlState;
            }

            public Loaded Copy(
                HashSet<Vector3Int>? voxels = null,
                BrushType? brushType = null,
                FreeCameraData? freeCameraData = null,
                IsometricCameraData? isometricCameraData = null,
                CameraType? cameraType = null,
                bool? isFileBrowserOpened = null,
                ControlState? controlState = null
            )
            {
                return new Loaded(
                    voxels: voxels ?? this.voxels,
                    brushType: brushType ?? this.brushType,
                    freeCameraData: freeCameraData ?? this.freeCameraData,
                    isometricCameraData: isometricCameraData ?? this.isometricCameraData,
                    cameraType: cameraType ?? this.cameraType,
                    isFileBrowserOpened: isFileBrowserOpened ?? this.isFileBrowserOpened,
                    controlState: controlState ?? this.controlState
                );
            }
        }
    }
}