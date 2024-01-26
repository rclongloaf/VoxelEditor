using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Brush;
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
                OnButtonDown(loadedState, onButtonDown);
                break;
            case EditorAction.Input.OnButtonUp onButtonUp:
                OnButtonUp(loadedState, onButtonUp);
                break;
            case EditorAction.Input.OnMenu onMenu:
                if (loadedState.uiState is UIState.None)
                {
                    reducer.ApplyPatch(new EditorPatch.MenuVisibility(true));
                }
                else if (loadedState.uiState is UIState.Menu)
                {
                    reducer.ApplyPatch(new EditorPatch.MenuVisibility(false));
                }
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

    private void OnButtonDown(EditorState.Loaded state, EditorAction.Input.OnButtonDown action)
    {
        switch (action)
        {
            case EditorAction.Input.OnButtonDown.Draw drawAction:
                OnDrawButtonDown(state, drawAction);
                break;
            case EditorAction.Input.OnButtonDown.Rotate:
                OnRotatingDown(state);
                break;
            case EditorAction.Input.OnButtonDown.MoveCamera:
                OnMoveButtonDown(state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    private void OnButtonUp(EditorState.Loaded state, EditorAction.Input.OnButtonUp action)
    {
        switch (action)
        {
            case EditorAction.Input.OnButtonUp.Draw:
                OnDrawButtonUp(state);
                break;
            case EditorAction.Input.OnButtonUp.Rotate:
                OnRotatingUp(state);
                break;
            case EditorAction.Input.OnButtonUp.MoveCamera:
                OnMoveButtonUp(state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
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

    private void OnDrawButtonDown(EditorState.Loaded state, EditorAction.Input.OnButtonDown.Draw action)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Start());
        ApplyDrawing(state, action);
    }

    private void OnDrawButtonUp(EditorState.Loaded state)
    {
        if (state.controlState is not ControlState.Drawing) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Finish());
    }

    private void ApplyDrawing(EditorState.Loaded state, EditorAction.Input.OnButtonDown.Draw action)
    {
        if (state.cameraType is not CameraType.Free) return;

        if (!GetVoxelUnderCursor(out var position, out var normal)) return;

        switch (action.withShift)
        {
            case false:
                switch (action.withCtrl)
                {
                    case false:
                    {
                        var addPosition = Vector3Int.RoundToInt(position + normal);
                        var voxels = new List<Vector3Int>();
                        voxels.Add(addPosition);
                        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
                        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Add(voxels)));
                        break;
                    }
                    case true:
                    {
                        var voxels = new List<Vector3Int>();
                        voxels.Add(position);
                        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(voxels));
                        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Delete(voxels)));
                        break;
                    }
                }
                break;
            case true:
                switch (action.withCtrl)
                {
                    case false:
                    {
                        var voxels = GetVoxelsSection(
                            model: state.currentSpriteData.voxels,
                            position: position,
                            normal: Vector3Int.RoundToInt(normal),
                            applyNormal: true
                        );
                        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
                        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Add(voxels)));
                        break;
                    }
                    case true:
                    {
                        var voxels = GetVoxelsSection(
                            model: state.currentSpriteData.voxels,
                            position: position,
                            normal: Vector3Int.RoundToInt(normal),
                            applyNormal: false
                        );
                        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(voxels));
                        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Delete(voxels)));
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private List<Vector3Int> GetVoxelsSection(
        HashSet<Vector3Int> model,
        Vector3Int position,
        Vector3Int normal,
        bool applyNormal
    )
    {
        var voxels = new HashSet<Vector3Int>();

        var queue = new Queue<Vector3Int>();
        queue.Enqueue(position);
        while (queue.Count > 0)
        {
            var voxel = queue.Dequeue();
            if (voxels.Contains(voxel))
            {
                continue;
            }

            if (!model.Contains(voxel)
                || model.Contains(voxel + normal))
            {
                continue;
            }

            voxels.Add(voxel);
            
            if (normal.z != 0)
            {
                queue.Enqueue(voxel + Vector3Int.left);
                queue.Enqueue(voxel + Vector3Int.right);
                queue.Enqueue(voxel + Vector3Int.up);
                queue.Enqueue(voxel + Vector3Int.down);
            }
            else if (normal.y != 0)
            {
                queue.Enqueue(voxel + Vector3Int.left);
                queue.Enqueue(voxel + Vector3Int.right);
                queue.Enqueue(voxel + Vector3Int.forward);
                queue.Enqueue(voxel + Vector3Int.back);
            } else if (normal.x != 0)
            {
                queue.Enqueue(voxel + Vector3Int.forward);
                queue.Enqueue(voxel + Vector3Int.back);
                queue.Enqueue(voxel + Vector3Int.up);
                queue.Enqueue(voxel + Vector3Int.down);
            }
        }

        if (applyNormal)
        {
            return voxels.ToList().ConvertAll(pos => pos + normal);
        }

        return voxels.ToList();
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