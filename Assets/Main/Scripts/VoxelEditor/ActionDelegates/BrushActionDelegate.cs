using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class BrushActionDelegate : ActionDelegate<EditorAction.Brush>
{
    public BrushActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.Brush action)
    {
        if (state is not EditorState.Loaded) return;

        reducer.ApplyPatch(new EditorPatch.Brush.ChangeType(
            action is EditorAction.Brush.OnBrushAddClicked ? BrushType.Add : BrushType.Delete
        ));
    }
}
}