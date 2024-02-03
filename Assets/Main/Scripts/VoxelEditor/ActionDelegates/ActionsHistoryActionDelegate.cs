using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

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
                case EditAction.Delete delete:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(delete.voxels));
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
                case EditAction.Delete delete:
                    reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(delete.voxels));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastCanceledAction));
            }
        }
    }
}
}