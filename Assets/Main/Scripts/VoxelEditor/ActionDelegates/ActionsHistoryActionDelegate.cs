using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ActionsHistoryActionDelegate : ActionDelegate<EditorAction.ActionsHistory>
{
    public ActionsHistoryActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.ActionsHistory action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.ActionsHistory.OnCancelClicked onCancelClicked:
                OnCancelClicked(activeLayer, onCancelClicked);
                break;
            case EditorAction.ActionsHistory.OnRestoreClicked onRestoreClicked:
                OnRestoreClicked(activeLayer, onRestoreClicked);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnCancelClicked(VoxLayerState.Loaded activeLayer, EditorAction.ActionsHistory.OnCancelClicked action)
    {
        if (activeLayer.actionsHistory.TryPeek(out var lastAction))
        {
            reducer.ApplyPatch(new EditorPatch.ActionsHistory.CancelAction());
            switch (lastAction)
            {
                case EditAction.Add add:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(add.voxels));
                    break;
                case EditAction.CancelSelection cancelSelection:
                    var deleteVoxels = new Dictionary<Vector3Int, VoxelData>();
                    foreach (var (pos, voxelData) in cancelSelection.voxels)
                    {
                        deleteVoxels[pos + cancelSelection.offset] = voxelData;
                    }
                    
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(deleteVoxels));
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(cancelSelection.overrideVoxels));
                    reducer.ApplyPatch(new EditorPatch.Selection.Select(cancelSelection.voxels, cancelSelection.offset));
                    break;
                case EditAction.ChangeSmooth changeSmooth:
                    var smoothMap = new Dictionary<Vector3Int, bool>();
                    foreach (var (pos, isSmooth) in changeSmooth.voxelsSmoothMap)
                    {
                        smoothMap[pos] = !isSmooth;
                    }
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.ChangeSmoothState(smoothMap));
                    break;
                case EditAction.Delete delete:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(delete.voxels));
                    break;
                case EditAction.DeleteSelected deleteSelected:
                    reducer.ApplyPatch(new EditorPatch.Selection.Select(
                        deleteSelected.selectedState.voxels,
                        deleteSelected.selectedState.offset
                    ));
                    break;
                case EditAction.MoveSelection moveSelection:
                    reducer.ApplyPatch(new EditorPatch.Control.SelectionMoving.ChangeSelectionOffset(-moveSelection.deltaOffset));
                    break;
                case EditAction.Paste paste:
                    reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
                    break;
                case EditAction.Select select:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(select.voxels));
                    reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastAction));
            }
        }
    }

    private void OnRestoreClicked(VoxLayerState.Loaded activeLayer, EditorAction.ActionsHistory.OnRestoreClicked action)
    {
        if (activeLayer.canceledActionsHistory.TryPeek(out var lastCanceledAction))
        {
            reducer.ApplyPatch(new EditorPatch.ActionsHistory.RestoreAction());
            switch (lastCanceledAction)
            {
                case EditAction.Add add:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(add.voxels));
                    break;
                case EditAction.CancelSelection cancelSelection:
                    var voxels = new Dictionary<Vector3Int, VoxelData>();
                    foreach (var (pos, voxelData) in cancelSelection.voxels)
                    {
                        voxels[pos + cancelSelection.offset] = voxelData;
                    }
                    
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
                    reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
                    break;
                case EditAction.ChangeSmooth changeSmooth:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.ChangeSmoothState(changeSmooth.voxelsSmoothMap));
                    break;
                case EditAction.Delete delete:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(delete.voxels));
                    break;
                case EditAction.DeleteSelected deleteSelected:
                    reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
                    break;
                case EditAction.MoveSelection moveSelection:
                    reducer.ApplyPatch(new EditorPatch.Control.SelectionMoving.ChangeSelectionOffset(moveSelection.deltaOffset));
                    break;
                case EditAction.Paste paste:
                    reducer.ApplyPatch(new EditorPatch.Selection.Select(paste.voxels, Vector3Int.zero));
                    break;
                case EditAction.Select select:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(select.voxels));
                    reducer.ApplyPatch(new EditorPatch.Selection.Select(select.voxels, Vector3Int.zero));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastCanceledAction));
            }
        }
    }
}
}