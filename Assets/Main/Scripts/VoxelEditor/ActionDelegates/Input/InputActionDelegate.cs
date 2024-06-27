using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Helpers;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor.ActionDelegates.Input
{
public class InputActionDelegate : ActionDelegate<EditorAction.Input>
{
    private SelectionDelegate selectionDelegate;
    
    public InputActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        SelectionDelegate selectionDelegate
    ) : base(feature, reducer)
    {
        this.selectionDelegate = selectionDelegate;
    }
    
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
            case EditorAction.Input.OnButtonSmooth onButtonSmooth:
                OnButtonSmooth(state, onButtonSmooth);
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
            case EditorAction.Input.UpdateMoveSelection updateMoveSelection:
                MoveSelection(state);
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
            case EditorAction.Input.OnButtonDown.Delete delete:
                OnDeleteButtonDown(state);
                break;
            case EditorAction.Input.OnButtonDown.Draw drawAction:
                OnDrawButtonDown(state, drawAction);
                break;
            case EditorAction.Input.OnButtonDown.RotateCamera:
                OnRotatingCameraDown(state);
                break;
            case EditorAction.Input.OnButtonDown.Smooth smoothAction:
                OnSmoothDown(state, smoothAction);
                break;
            case EditorAction.Input.OnButtonDown.Select select:
                OnSelectDown(state);
                break;
            case EditorAction.Input.OnButtonDown.MoveCamera:
                OnMoveButtonDown(state);
                break;
            case EditorAction.Input.OnButtonDown.MoveSelection moveSelection:
                OnMoveSelectionButtonDown(state);
                break;
            case EditorAction.Input.OnButtonDown.RotateVoxels rotateVoxels:
                OnRotatingVoxelsDown(state, rotateVoxels);
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
            case EditorAction.Input.OnButtonUp.RotateCamera:
                OnRotatingCameraUp(state);
                break;
            case EditorAction.Input.OnButtonUp.RotateVoxels rotateVoxels:
                OnRotatingVoxelsUp(state);
                break;
            case EditorAction.Input.OnButtonUp.Smooth smooth:
                OnSmoothUp(state);
                break;
            case EditorAction.Input.OnButtonUp.Select select:
                OnSelectUp(state);
                break;
            case EditorAction.Input.OnButtonUp.MoveCamera:
                OnMoveButtonUp(state);
                break;
            case EditorAction.Input.OnButtonUp.MoveSelection moveSelection:
                OnMoveSelectionButtonUp(state);
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
            case ControlState.CameraMoving:
                MoveCameraByMouseDelta(state, action.deltaX, action.deltaY);
                break;
            case ControlState.RotatingVoxels:
                RotateVoxelsByMouseDelta(state, action.deltaX, action.deltaY);
                break;
            case ControlState.RotatingCamera:
                RotateCameraByMouseDelta(state, action.deltaX, action.deltaY);
                break;
            case ControlState.Selection selection:
                break;
            case ControlState.SelectionMoving selectionMoving:
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

    private void OnDeleteButtonDown(EditorState state)
    {
        selectionDelegate.DeleteSelected(state);
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
            if (!activeLayer.currentSpriteData.voxels.ContainsKey(position)) return;
            
            var voxels = new Dictionary<Vector3Int, VoxelData>();
            voxels[position] = activeLayer.currentSpriteData.voxels[position];
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(voxels));
        }
        else
        {
            var voxel = position + drawingState.normal + (drawingState.withProjection ? new Vector3Int(0, -drawingState.normal.z, 0) : Vector3Int.zero);
            if (activeLayer.currentSpriteData.voxels.ContainsKey(voxel)) return;
            
            var voxels = new Dictionary<Vector3Int, VoxelData>();
            var textureData = activeLayer.voxData.textureData;
            
            if (voxel.x < textureData.spriteWidth
                && voxel.x >= 0
                && voxel.y + voxel.z < textureData.spriteHeight
                && voxel.y + voxel.z >= 0
                && voxel.z < textureData.spriteHeight * 0.5
                && voxel.z >= -textureData.spriteHeight * 0.5)
            {
                //todo draw with smooth flag
                voxels[voxel] = new VoxelData(false);
                reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
            }
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

            var voxelsInBounds = new Dictionary<Vector3Int, VoxelData>();
            var textureData = activeLayer.voxData.textureData;
            foreach (var (voxel, data) in voxels)
            {
                if (voxel.x < textureData.spriteWidth
                    && voxel.x >= 0
                    && voxel.y + voxel.z < textureData.spriteHeight
                    && voxel.y + voxel.z >= 0
                    && voxel.z < textureData.spriteHeight * 0.5
                    && voxel.z >= -textureData.spriteHeight * 0.5)
                {
                    voxelsInBounds[voxel] = data;
                }
            }
            reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxelsInBounds));
        }
    }

    private Dictionary<Vector3Int, VoxelData> GetVoxelsSection(
        Dictionary<Vector3Int, VoxelData> model,
        Vector3Int position,
        Vector3Int normal,
        bool withProjection,
        bool applyNormal
    )
    {
        var voxels = new Dictionary<Vector3Int, VoxelData>();

        var queue = new Queue<Vector3Int>();
        queue.Enqueue(position);
        while (queue.Count > 0)
        {
            var voxel = queue.Dequeue();
            if (voxels.ContainsKey(voxel))
            {
                continue;
            }

            if (!model.ContainsKey(voxel)
                || model.ContainsKey(voxel + normal))
            {
                continue;
            }

            voxels[voxel] = model[voxel];
            
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
            var voxelsWithAppliedNormal = new Dictionary<Vector3Int, VoxelData>();
            foreach (var (pos, voxelData) in voxels)
            {
                voxelsWithAppliedNormal[pos + normal + (withProjection ? new Vector3Int(0, -normal.z, 0) : Vector3Int.zero)] = voxelData;
            }
            return voxelsWithAppliedNormal;
        }

        return voxels;
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
        
        position = Vector3Int.RoundToInt(hitInfo.transform.parent.position);
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
        if (state.controlState is not ControlState.CameraMoving) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Moving.Finish());
    }

    private void MoveCameraByMouseDelta(EditorState state, float deltaX, float deltaY)
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

    private void OnMoveSelectionButtonDown(EditorState state)
    {
        var camera = Camera.main;
        if (camera == null
            || state.activeLayer is not VoxLayerState.Loaded
            {
                selectionState: SelectionState.Selected selectionState
            } activeLayer) return;

        var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            reducer.ApplyPatch(new EditorPatch.Control.SelectionMoving.Start(Vector3Int.RoundToInt(hit.normal), hit.point));
        }
        else
        {
            selectionDelegate.CancelSelection(state);
        }
    }

    private void OnMoveSelectionButtonUp(EditorState state)
    {
        if (state.controlState is not ControlState.SelectionMoving selectionMoving) return;
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.MoveSelection(selectionMoving.deltaOffset)));
        reducer.ApplyPatch(new EditorPatch.Control.SelectionMoving.Finish());
    }

    private void MoveSelection(EditorState state)
    {
        var camera = Camera.main;
        if (camera == null
            || state.activeLayer is not VoxLayerState.Loaded activeLayer
            || activeLayer.selectionState is not SelectionState.Selected selectionState
            || state.controlState is not ControlState.SelectionMoving controlState) return;
        
        var plane = new Plane(controlState.normal, controlState.fromPosition);
        var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

        var point = plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : controlState.fromPosition;

        var offset = Vector3Int.RoundToInt(point - controlState.fromPosition);

        var deltaOffset = offset - controlState.deltaOffset;
        
        reducer.ApplyPatch(new EditorPatch.Control.SelectionMoving.ChangeSelectionOffset(deltaOffset));
    }

    private void OnRotatingVoxelsDown(EditorState state, EditorAction.Input.OnButtonDown.RotateVoxels action)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        var axis = action.x ? Axis.X : action.y ? Axis.Y : Axis.Z;
        
        reducer.ApplyPatch(new EditorPatch.Control.RotatingVoxels.Start(axis));
    }

    private void OnRotatingVoxelsUp(EditorState state)
    {
        if (state.controlState is not ControlState.RotatingVoxels rotatingVoxels
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;
        
        Dictionary<Vector3Int, VoxelData> newVoxels;

        if (activeLayer.selectionState is SelectionState.Selected selectedState)
        {
            newVoxels = selectedState.voxels;
        }
        else
        {
            newVoxels = activeLayer.currentSpriteData.voxels;
        }
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(
            new EditAction.RotateVoxels(rotatingVoxels.voxels, newVoxels)
        ));
        reducer.ApplyPatch(new EditorPatch.Control.RotatingVoxels.Finish());
    }
    
    private void OnRotatingCameraDown(EditorState state)
    {
        if (state.cameraType is not CameraType.Free
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.RotatingCamera.Start());
    }

    private void OnRotatingCameraUp(EditorState state)
    {
        if (state.controlState is not ControlState.RotatingCamera) return;
        reducer.ApplyPatch(new EditorPatch.Control.RotatingCamera.Finish());
    }

    private void RotateCameraByMouseDelta(EditorState state, float deltaX, float deltaY)
    {
        if (state.controlState is not ControlState.RotatingCamera
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

    private void RotateVoxelsByMouseDelta(EditorState state, float deltaX, float deltaY)
    {
        if (state.controlState is not ControlState.RotatingVoxels rotatingVoxelsState
            || state.cameraType is not CameraType.Free
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        var axis = rotatingVoxelsState.axis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => throw new ArgumentOutOfRangeException()
        };

        var angle = rotatingVoxelsState.angle - deltaX * 4f;
        var snappedAngle = Mathf.Round(angle / 15) * 15;
        var quaternion = Quaternion.AngleAxis(snappedAngle, axis);

        var voxels = rotatingVoxelsState.voxels;

        
        var rotatedVoxels = new Dictionary<Vector3Int, VoxelData>();

        var offset = new Vector3Int(
            (int)activeLayer.currentSpriteData.pivot.x,
            (int)activeLayer.currentSpriteData.pivot.y,
            0
        );

        foreach (var (position, data) in voxels)
        {
            var deltaPosition = position - offset;
            rotatedVoxels.TryAdd(Vector3Int.RoundToInt(quaternion * deltaPosition) + offset, data);
        }
        rotatedVoxels.FillEmptySpaces();
       
        reducer.ApplyPatch(new EditorPatch.Control.RotatingVoxels.ApplyNewVoxels(rotatedVoxels));
        reducer.ApplyPatch(new EditorPatch.Control.RotatingVoxels.ChangeAngle(angle));
    }
    
    private void OnSmoothDown(EditorState state, EditorAction.Input.OnButtonDown.Smooth action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded
            || state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Smoothing.Start(action.enableSmooth));
    }
        
    private void OnButtonSmooth(EditorState state, EditorAction.Input.OnButtonSmooth action)
    {
        if (state.controlState is not ControlState.Smoothing smoothingState
            || state.cameraType is not CameraType.Free
            || state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        if (!Physics.Raycast(camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out var hitInfo)) return;
        
        var position = Vector3Int.RoundToInt(hitInfo.transform.parent.position) + Vector3Int.RoundToInt(activeLayer.currentSpriteData.pivot);

        if (smoothingState.smoothVoxels.Contains(position)
            || !activeLayer.currentSpriteData.voxels.TryGetValue(position, out var voxelData)
            || voxelData.isSmooth == smoothingState.enableSmooth)
        {
            return;
        }
        
        reducer.ApplyPatch(new EditorPatch.Control.Smoothing.AddSmoothVoxel(position));

        var smoothVoxels = new Dictionary<Vector3Int, bool>();
        smoothVoxels[position] = smoothingState.enableSmooth;
        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.ChangeSmoothState(smoothVoxels));
    }
    
    private void OnSmoothUp(EditorState state)
    {
        if (state.activeLayer is not VoxLayerState.Loaded loadedLayer
            || state.controlState is not ControlState.Smoothing smoothingState) return;

        var smoothVoxelsMap = new Dictionary<Vector3Int, bool>();
        foreach (var pos in smoothingState.smoothVoxels)
        {
            smoothVoxelsMap[pos] = smoothingState.enableSmooth;
        }

        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(
            new EditAction.ChangeSmooth(smoothVoxelsMap)
        ));
        reducer.ApplyPatch(new EditorPatch.Control.Smoothing.Finish());
    }

    private void OnSelectDown(EditorState state)
    {
        if (state.controlState is not ControlState.None) return;
        
        reducer.ApplyPatch(new EditorPatch.Control.Selection.Start(UnityEngine.Input.mousePosition));
    }

    private void OnSelectUp(EditorState state)
    {
        if (state.activeLayer is not VoxLayerState.Loaded loadedLayer
            || state.controlState is not ControlState.Selection selectionState) return;
        
        var mousePos = UnityEngine.Input.mousePosition;

        var selectedVoxels = new Dictionary<Vector3Int, VoxelData>();
        foreach (var (voxel, voxelData) in loadedLayer.currentSpriteData.voxels)
        {
            var pivot = loadedLayer.currentSpriteData.pivot;
            var voxPos = new Vector3(voxel.x - pivot.x + 0.5f, voxel.y - pivot.y + 0.5f, voxel.z + 0.5f);
            if (IsWithinPolygon(voxPos, selectionState.startMousePos, mousePos))
            {
                selectedVoxels[voxel] = loadedLayer.currentSpriteData.voxels[voxel];
            }
        }
        
        selectionDelegate.Select(selectedVoxels);
        reducer.ApplyPatch(new EditorPatch.Control.Selection.Finish());
    }
    
    private bool IsWithinPolygon(Vector3 unitPos, Vector3 fromPos, Vector3 toPos)
    {
        var camera = Camera.main;
        if (camera == null) return false;

        var plane = new Plane(camera.transform.forward, unitPos);
        var minX = Math.Min(fromPos.x, toPos.x);
        var minY = Math.Min(fromPos.y, toPos.y);
        var maxX = Math.Max(fromPos.x, toPos.x);
        var maxY = Math.Max(fromPos.y, toPos.y);

        var TLRay = camera.ScreenPointToRay(new Vector3(minX, maxY, 0));
        var TRRay = camera.ScreenPointToRay(new Vector3(maxX, maxY, 0));
        var BLRay = camera.ScreenPointToRay(new Vector3(minX, minY, 0));
        var BRRay = camera.ScreenPointToRay(new Vector3(maxX, minY, 0));

        Vector3 TL;
        Vector3 TR;
        Vector3 BL;
        Vector3 BR;
        
        if (plane.Raycast(TLRay, out var TLDistance))
        {
            TL = TLRay.GetPoint(TLDistance);
        }
        else
        {
            return false;
        }
        
        if (plane.Raycast(TRRay, out var TRDistance))
        {
            TR = TRRay.GetPoint(TRDistance);
        }
        else
        {
            return false;
        }
        
        if (plane.Raycast(BLRay, out var BLDistance))
        {
            BL = BLRay.GetPoint(BLDistance);
        }
        else
        {
            return false;
        }
        
        if (plane.Raycast(BRRay, out var BRDistance))
        {
            BR = BRRay.GetPoint(BRDistance);
        }
        else
        {
            return false;
        }
        
        if (IsWithinTriangle(unitPos, TL, BL, TR))
        {
            return true;
        }

        //Triangle 2: TR - BL - BR
        if (IsWithinTriangle(unitPos, TR, BL, BR))
        {
            return true;
        }

        return false;
    }

    bool IsWithinTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(p, p1, p2);
        d2 = Sign(p, p2, p3);
        d3 = Sign(p, p3, p1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    private float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
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