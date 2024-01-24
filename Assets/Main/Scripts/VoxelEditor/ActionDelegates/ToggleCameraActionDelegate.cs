using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ToggleCameraActionDelegate : ActionDelegate<EditorAction.OnToggleCameraClicked>
{
    public ToggleCameraActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.OnToggleCameraClicked action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        var cameraType = loadedState.cameraType switch
        {
            CameraType.Free => CameraType.Isometric,
            CameraType.Isometric => CameraType.Free,
            _ => throw new ArgumentOutOfRangeException()
        };
        reducer.ApplyPatch(new EditorPatch.Camera.ChangeType(cameraType));
    }
}
}