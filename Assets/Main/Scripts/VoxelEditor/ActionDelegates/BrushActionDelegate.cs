using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Brush;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class BrushActionDelegate : ActionDelegate<EditorAction.Brush>
{
    public BrushActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.Brush action)
    {
        switch (action)
        {
            case EditorAction.Brush.OnBrushAddClicked onBrushAddClicked:
                reducer.ApplyPatch(new EditorPatch.Brush.ChangeType(BrushType.Add));
                break;
            case EditorAction.Brush.OnBrushDeleteClicked onBrushDeleteClicked:
                reducer.ApplyPatch(new EditorPatch.Brush.ChangeType(BrushType.Delete));
                break;
            case EditorAction.Brush.OnBrushModeOneClicked onBrushModeOneClicked:
                reducer.ApplyPatch(new EditorPatch.Brush.ChangeMode(BrushMode.One));
                break;
            case EditorAction.Brush.OnBrushModeSectionClicked onBrushModeSectionClicked:
                reducer.ApplyPatch(new EditorPatch.Brush.ChangeMode(BrushMode.Section));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}