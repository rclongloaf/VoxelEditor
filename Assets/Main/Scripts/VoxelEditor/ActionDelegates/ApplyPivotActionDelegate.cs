using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ApplyPivotActionDelegate : ActionDelegate<EditorAction.OnApplyPivotClicked>
{
    public ApplyPivotActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.OnApplyPivotClicked action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded) return;
        
        reducer.ApplyPatch(new EditorPatch.NewPivotPoint(action.pivotPoint));
    }
}
}