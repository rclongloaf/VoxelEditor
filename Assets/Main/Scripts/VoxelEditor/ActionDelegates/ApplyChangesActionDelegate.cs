using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ApplyChangesActionDelegate : ActionDelegate<EditorAction.ApplyChanges>
{
    public ApplyChangesActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.ApplyChanges action)
    {
        if (state is not EditorState.Loaded { uiState: UIState.ApplyChanges }) return;

        switch (action)
        {
            case EditorAction.ApplyChanges.Apply apply:
                reducer.ApplyPatch(new EditorPatch.SpriteChanges.Apply());
                break;
            case EditorAction.ApplyChanges.Discard discard:
                reducer.ApplyPatch(new EditorPatch.SpriteChanges.Discard());
                break;
            case EditorAction.ApplyChanges.Cancel cancel:
                reducer.ApplyPatch(new EditorPatch.SpriteChanges.Cancel());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}