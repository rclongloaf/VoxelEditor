using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor.ActionDelegates.Input
{
public class InputActionDelegate : ActionDelegate<EditorAction.Input>
{
    public InputActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.Input action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.Input.OnButtonDown onButtonDown:
                OnButtonDown(state, onButtonDown);
                break;
            case EditorAction.Input.OnButtonUp onButtonUp:
                OnButtonUp(state, onButtonUp);
                break;
            case EditorAction.Input.OnButtonDraw onButtonDrawAction:
                OnButtonDraw(state, onButtonDrawAction);
                break;
            case EditorAction.Input.OnMenu onMenu:
                if (state.uiState is UIState.None)
                {
                    reducer.ApplyPatch(new EditorPatch.MenuVisibility(true));
                }
                else if (state.uiState is UIState.Menu)
                {
                    reducer.ApplyPatch(new EditorPatch.MenuVisibility(false));
                }
                break;
            case EditorAction.Input.OnMouseDelta onMouseDelta:
                OnMouseDelta(state, onMouseDelta);
                break;
            case EditorAction.Input.OnWheelScroll onWheelScroll:
                OnWheelScroll(state, onWheelScroll);
                break;
            case EditorAction.Input.OnToggleSpriteRef onToggleSpriteRefAction:
                OnToggleSpriteRef(state, onToggleSpriteRefAction);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnButtonDown(EditorState state, EditorAction.Input.OnButtonDown action)
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

    private void OnButtonUp(EditorState state, EditorAction.Input.OnButtonUp action)
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

    private void OnMouseDelta(EditorState state, EditorAction.Input.OnMouseDelta action)
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

    private void OnWheelScroll(EditorState state, EditorAction.Input.OnWheelScroll action)
    {
        if (state.controlState is not ControlState.None
            || state.cameraType is not CameraType.Free) return;

        reducer.ApplyPatch(new EditorPatch.Camera.NewDistance(
            state.freeCameraData.distance * (action.delta > 0 ? 0.9f : 1.1f)
        ));
    }

    private void OnToggleSpriteRef(EditorState state, EditorAction.Input.OnToggleSpriteRef action)
    {
        reducer.ApplyPatch(new EditorPatch.ChangeSpriteRefVisibility(!state.isSpriteRefVisible));
    }

    private void OnDrawButtonDown(EditorState state, EditorAction.Input.OnButtonDown.Draw action)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;
        
        if (!GetVoxelUnderCursor(out var hitPosition, out var normalFloat)) return;

        var normal = Vector3Int.RoundToInt(normalFloat);
        
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Start(
            position: hitPosition,
            normal: normal,
            deleting: action.withDelete,
            bySection: action.withSection,
            withProjection: action.withProjection
        ));
        if (action.withSection)
        {
            ApplyDrawingBySection(
                activeLayer,
                action,
                hitPosition + Vector3Int.RoundToInt(activeLayer.currentSpriteData.pivot),
                normal
            );
        }
    }

    private void OnDrawButtonUp(EditorState state)
    {
        if (state.controlState is not ControlState.Drawing drawingState) return;

        reducer.ApplyPatch(drawingState.deleting
            ? new EditorPatch.ActionsHistory.NewAction(new EditAction.Delete(drawingState.drawnVoxels))
            : new EditorPatch.ActionsHistory.NewAction(new EditAction.Add(drawingState.drawnVoxels))
        );
        reducer.ApplyPatch(new EditorPatch.Control.Drawing.Finish());
    }
    
    private void OnButtonDraw(EditorState state, EditorAction.Input.OnButtonDraw action)
    {
        if (state.controlState is not ControlState.Drawing drawingState
            || drawingState.bySection
            || state.cameraType is not CameraType.Free
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        var normal = drawingState.normal;
        var deltaNormal = normal.x > 0 ||
                          normal.y > 0 ||
                          normal.z > 0
            ? normal
            : Vector3Int.zero;

        var planePosition = drawingState.position + deltaNormal;
        
        var plane = new Plane(normal, planePosition);
        if (!GetVoxelUnderCursorOnPlane(plane, out var position)) return;
        
        ApplyDrawingByOne(activeLayer, drawingState, position - deltaNormal + Vector3Int.RoundToInt(activeLayer.currentSpriteData.pivot));
    }

    private void ApplyDrawingByOne(
        VoxLayerState.Loaded activeLayer,
        ControlState.Drawing drawingState,
        Vector3Int position
    )
    {
        if (drawingState.deleting)
        {
            if (!activeLayer.currentSpriteData.voxels.Contains(position)) return;
            
            var voxels = new List<Vector3Int>();
            voxels.Add(position);
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(voxels));
        }
        else
        {
            var addPosition = position + drawingState.normal + (drawingState.withProjection ? new Vector3Int(0, -drawingState.normal.z, 0) : Vector3Int.zero);
            if (activeLayer.currentSpriteData.voxels.Contains(addPosition)) return;
            
            var voxels = new List<Vector3Int>();
            voxels.Add(addPosition);
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
        }
    }

    private void ApplyDrawingBySection(
        VoxLayerState.Loaded activeLayer,
        EditorAction.Input.OnButtonDown.Draw action,
        Vector3Int position,
        Vector3Int normal
    )
    {
        if (action.withDelete)
        {
            var voxels = GetVoxelsSection(
                model: activeLayer.currentSpriteData.voxels,
                position: position,
                normal: Vector3Int.RoundToInt(normal),
                withProjection: false,
                applyNormal: false
            );
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(voxels));
        }
        else
        {
            var voxels = GetVoxelsSection(
                model: activeLayer.currentSpriteData.voxels,
                position: position,
                normal: Vector3Int.RoundToInt(normal),
                withProjection: action.withProjection,
                applyNormal: true
            );
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
        }
    }

    private List<Vector3Int> GetVoxelsSection(
        HashSet<Vector3Int> model,
        Vector3Int position,
        Vector3Int normal,
        bool withProjection,
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
            return voxels.ToList().ConvertAll(pos => pos + normal + (withProjection ? new Vector3Int(0, -normal.z, 0) : Vector3Int.zero));
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
    
    private bool GetVoxelUnderCursorOnPlane(Plane plane, out Vector3Int position)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            position = Vector3Int.zero;
            return false;
        }

        var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
        if (plane.Raycast(ray, out var cursorDistance))
        {
            position = Vector3Int.FloorToInt(ray.GetPoint(cursorDistance));
            return true;
        }

        position = Vector3Int.zero;
        return false;
    }

    private void OnMoveButtonDown(EditorState state)
    {
        if (state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Moving.Start());
    }

    private void OnMoveButtonUp(EditorState state)
    {
        if (state.controlState is not ControlState.Moving) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Moving.Finish());
    }

    private void MoveByMouseDelta(EditorState state, float deltaX, float deltaY)
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
    
    private void OnRotatingDown(EditorState state)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Rotating.Start());
    }

    private void OnRotatingUp(EditorState state)
    {
        if (state.controlState is not ControlState.Rotating) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Rotating.Finish());
    }

    private void RotateByMouseDelta(EditorState state, float deltaX, float deltaY)
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