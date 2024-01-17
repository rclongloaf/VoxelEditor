using System;
using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor
{
public class EditorReducer
{
    private EditorFeature feature;
    private EditorState state => feature.state;

    public EditorReducer(EditorFeature feature)
    {
        this.feature = feature;
    }

    public void ApplyPatch(EditorPatch patch)
    {
        var newState = patch switch
        {
            EditorPatch.FileBrowser fileBrowserPatch => ApplyFileBrowserPatch(fileBrowserPatch),
            EditorPatch.VoxLoaded voxLoadedPatch => ApplyVoxLoadedPatch(voxLoadedPatch),
            EditorPatch.Control controlPatch => ApplyControlPatch(controlPatch),
            EditorPatch.VoxelsChanges voxelsChanges => ApplyVoxelsChangesPatch(voxelsChanges),
            EditorPatch.Brush.ChangeType brushPatch => ApplyBrushPatch(brushPatch),
            EditorPatch.Camera cameraPatch => ApplyCameraPatch(cameraPatch)
        };

        feature.UpdateState(newState);
    }

    private EditorState ApplyFileBrowserPatch(EditorPatch.FileBrowser patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState.Copy(
            isFileBrowserOpened: patch is EditorPatch.FileBrowser.Opened    
        );
    }

    private EditorState ApplyVoxLoadedPatch(EditorPatch.VoxLoaded patch)
    {
        return state switch
        {
            EditorState.WaitingForProject => new EditorState.Loaded(
                voxels: patch.voxels,
                brushType: BrushType.Add,
                freeCameraData: new FreeCameraData(
                    pivotPoint: new Vector3(14, 18, 0),
                    distance: 30,
                    rotation: default
                ),
                isometricCameraData: new IsometricCameraData(default),
                cameraType: CameraType.Free,
                isFileBrowserOpened: false,
                controlState: ControlState.None
            ),
            EditorState.Loaded loadedState => loadedState.Copy(
                voxels: patch.voxels
            )
        };
    }

    private EditorState ApplyControlPatch(EditorPatch.Control patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;
        
        return patch switch
        {
            EditorPatch.Control.Drawing.Start => loadedState.Copy(controlState: ControlState.Drawing),
            EditorPatch.Control.Drawing.Finish => loadedState.Copy(controlState: ControlState.None),
            EditorPatch.Control.Moving.Start => loadedState.Copy(controlState: ControlState.Moving),
            EditorPatch.Control.Moving.Finish => loadedState.Copy(controlState: ControlState.None),
            EditorPatch.Control.Rotating.Start => loadedState.Copy(controlState: ControlState.Rotating),
            EditorPatch.Control.Rotating.Finish => loadedState.Copy(controlState: ControlState.None),
            
            _ => throw new ArgumentOutOfRangeException(nameof(patch), patch, null)
        };
    }

    private EditorState ApplyVoxelsChangesPatch(EditorPatch.VoxelsChanges patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return patch switch
        {
            EditorPatch.VoxelsChanges.Add addPatch => loadedState.Copy(voxels: loadedState.voxels.Plus(addPatch.voxel)),
            EditorPatch.VoxelsChanges.Delete deletePatch => loadedState.Copy(voxels: loadedState.voxels.Minus(deletePatch.voxel)),
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
    }

    private EditorState ApplyBrushPatch(EditorPatch.Brush.ChangeType patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState.Copy(brushType: patch.brushType);
    }

    private EditorState ApplyCameraPatch(EditorPatch.Camera patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        switch (patch)
        {
            case EditorPatch.Camera.NewPivotPoint newPosition:
                if (loadedState.cameraType is CameraType.Free)
                {
                    return loadedState.Copy(freeCameraData: loadedState.freeCameraData.Copy(
                        pivotPoint: newPosition.position
                    ));
                }
                
                return loadedState.Copy(isometricCameraData: new IsometricCameraData(newPosition.position));
                break;
            case EditorPatch.Camera.NewDistance newDistance:
                if (loadedState.cameraType is CameraType.Free)
                {
                    return loadedState.Copy(freeCameraData: loadedState.freeCameraData.Copy(
                        distance: newDistance.distance
                    ));
                }

                return loadedState;
                break;
            case EditorPatch.Camera.NewRotation newRotation:
                return loadedState.Copy(freeCameraData: loadedState.freeCameraData.Copy(
                    rotation: newRotation.rotation
                ));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }
}
}