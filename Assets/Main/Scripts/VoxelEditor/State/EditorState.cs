using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.State
{
    public record EditorState(
        Dictionary<int, VoxLayerState> layers,
        int activeLayerKey,
        SpriteData? bufferedSpriteData,
        ShaderData shaderData,
        bool isSpriteRefVisible,
        FreeCameraData freeCameraData,
        IsometricCameraData isometricCameraData,
        CameraType cameraType,
        ControlState controlState,
        EditModeState editModeState,
        UIState uiState
    )
    {
        public VoxLayerState activeLayer => layers[activeLayerKey];
    }
}