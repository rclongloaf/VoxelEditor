using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class EditModeActionDelegate : ActionDelegate<EditorAction.EditMode>
{
    private SelectionDelegate selectionDelegate;

    public EditModeActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        SelectionDelegate selectionDelegate
    ) : base(feature, reducer)
    {
        this.selectionDelegate = selectionDelegate;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.EditMode action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;
        
        switch (action)
        {
            case EditorAction.EditMode.OnEditModeClicked onEditModeClicked:
                reducer.ApplyPatch(new EditorPatch.EditMode.EditModeSelected());
                break;
            case EditorAction.EditMode.OnRenderModeClicked onRenderModeClicked:
                selectionDelegate.CancelSelection(state);
                reducer.ApplyPatch(new EditorPatch.EditMode.RenderModeSelected());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}