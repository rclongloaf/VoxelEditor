using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ApplyPivotActionDelegate : ActionDelegate<EditorAction.OnApplyPivotClicked>
{
    public ApplyPivotActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.OnApplyPivotClicked action)
    {
        if (state is not EditorState.Loaded loadedState) return;
        
        reducer.ApplyPatch(new EditorPatch.NewPivotPoint(action.pivotPoint));
    }
}
}