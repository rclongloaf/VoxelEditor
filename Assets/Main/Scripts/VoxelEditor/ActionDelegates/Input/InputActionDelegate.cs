using System;
using Main.Scripts.VoxelEditor.State;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor.ActionDelegates.Input
{
public class InputActionDelegate : ActionDelegate<EditorAction.Input>
{
    public InputActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.Input action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        switch (action)
        {
            case EditorAction.Input.OnButtonDown onButtonDown:
                OnButtonDown(loadedState, onButtonDown.keyCode);
                break;
            case EditorAction.Input.OnButtonUp onButtonUp:
                OnButtonUp(loadedState, onButtonUp.keyCode);
                break;
            case EditorAction.Input.OnMouseDelta onMouseDelta:
                OnMouseDelta(loadedState, onMouseDelta);
                break;
            case EditorAction.Input.OnWheelScroll onWheelScroll:
                OnWheelScroll(loadedState, onWheelScroll);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnButtonDown(EditorState.Loaded state, KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Mouse0:
                OnDrawButtonDown(state);
                break;
            case KeyCode.Mouse1:
                OnRotatingDown(state);
                break;
            case KeyCode.Mouse2:
                OnMoveButtonDown(state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(keyCode), keyCode, null);
        }
    }

    private void OnButtonUp(EditorState.Loaded state, KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Mouse0:
                OnDrawButtonUp(state);
                break;
            case KeyCode.Mouse1:
                OnRotatingUp(state);
                break;
            case KeyCode.Mouse2:
                OnMoveButtonUp(state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(keyCode), keyCode, null);
        }
    }

    private void OnMouseDelta(EditorState.Loaded state, EditorAction.Input.OnMouseDelta action)
    {
        switch (state.controlState)
        {
            case ControlState.None:
                break;
            case ControlState.Drawing:
                break;
            case ControlState.Moving:
                MoveByMouseDelta(state, action.deltaX, action.deltaY);
                break;
            case ControlState.Rotating:
                RotateByMouseDelta(state, action.deltaX, action.deltaY);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnWheelScroll(EditorState.Loaded state, EditorAction.Input.OnWheelScroll action)
    {
        if (state.controlState is not ControlState.None
            || state.cameraType is not CameraType.Free) return;

        reducer.ApplyPatch(new EditorPatch.Camera.NewDistance(
            state.freeCameraData.distance * (action.delta > 0 ? 0.9f : 1.1f)
        ));
    }

    private void OnDrawButtonDown(EditorState.Loaded state)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Start());
        ApplyDrawing(state);
    }

    private void OnDrawButtonUp(EditorState.Loaded state)
    {
        if (state.controlState is not ControlState.Drawing) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Finish());
    }

    private void ApplyDrawing(EditorState.Loaded state)
    {
        if (state.cameraType is not CameraType.Free) return;

        if (!GetVoxelUnderCursor(out var position, out var normal)) return;

        switch (state.brushType)
        {
            case BrushType.Add:
                var addPosition = Vector3Int.RoundToInt(position + normal);
                reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(addPosition));
                break;
            case BrushType.Delete:
                reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(position));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private bool GetVoxelUnderCursor(out Vector3Int position, out Vector3 normal)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            position = Vector3Int.zero;
            normal = Vector3.zero;
            return false;
        }

        var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

        if (!Physics.Raycast(ray, out var hitInfo))
        {
            position = Vector3Int.zero;
            normal = Vector3.zero;
            return false;
        }
        
        position = Vector3Int.RoundToInt(hitInfo.transform.position);
        normal = hitInfo.normal;
        return true;
    }

    private void OnMoveButtonDown(EditorState.Loaded state)
    {
        if (state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Moving.Start());
    }

    private void OnMoveButtonUp(EditorState.Loaded state)
    {
        if (state.controlState is not ControlState.Moving) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Moving.Finish());
    }

    private void MoveByMouseDelta(EditorState.Loaded state, float deltaX, float deltaY)
    {
        var deltaPos = new Vector3(-deltaX, -deltaY, 0);
        var position = state.cameraType switch
        {
            CameraType.Free => state.freeCameraData.pivotPoint
                               + state.freeCameraData.rotation
                               * (state.freeCameraData.distance
                                  * 0.03f
                                  * deltaPos),
            CameraType.Isometric => state.isometricCameraData.position
                                    + Quaternion.Euler(45, 0, 0) * deltaPos,
            _ => throw new ArgumentOutOfRangeException()
        };
        reducer.ApplyPatch(new EditorPatch.Camera.NewPivotPoint(position));
    }
    
    private void OnRotatingDown(EditorState.Loaded state)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Rotating.Start());
    }

    private void OnRotatingUp(EditorState.Loaded state)
    {
        if (state.controlState is not ControlState.Rotating) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Rotating.Finish());
    }

    private void RotateByMouseDelta(EditorState.Loaded state, float deltaX, float deltaY)
    {
        if (state.controlState is not ControlState.Rotating
            || state.cameraType is not CameraType.Free) return;

        var eulerAngles = state.freeCameraData.rotation.eulerAngles;

        var rotationX = eulerAngles.y;
        var rotationY = eulerAngles.x;
                        
        rotationX += deltaX * 4f;
        rotationY -= deltaY * 4f;
                        
        rotationX = ClampAngle(rotationX, -180, 180);
        rotationY = ClampAngle(rotationY, -90, 90);

        var xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        var yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
        
        var newRotation = xQuaternion * yQuaternion;

        reducer.ApplyPatch(new EditorPatch.Camera.NewRotation(newRotation));
    }
    
    private static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -180F)
            angle += 360F;
        if (angle > 180F)
            angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }
}
}