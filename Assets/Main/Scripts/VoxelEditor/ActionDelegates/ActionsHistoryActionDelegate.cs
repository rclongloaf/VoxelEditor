using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ActionsHistoryActionDelegate : ActionDelegate<EditorAction.ActionsHistory>
{
    public ActionsHistoryActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.ActionsHistory action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        switch (action)
        {
            case EditorAction.ActionsHistory.OnCancelClicked onCancelClicked:
                OnCancelClicked(loadedState, onCancelClicked);
                break;
            case EditorAction.ActionsHistory.OnRestoreClicked onRestoreClicked:
                OnRestoreClicked(loadedState, onRestoreClicked);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnCancelClicked(EditorState.Loaded state, EditorAction.ActionsHistory.OnCancelClicked action)
    {
        if (state.actionsHistory.TryPeek(out var lastAction))
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

    private void OnRestoreClicked(EditorState.Loaded state, EditorAction.ActionsHistory.OnRestoreClicked action)
    {
        if (state.canceledActionsHistory.TryPeek(out var lastCanceledAction))
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