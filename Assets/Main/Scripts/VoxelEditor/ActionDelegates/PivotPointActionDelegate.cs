using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class PivotPointActionDelegate : ActionDelegate<EditorAction.PivotPoint>
{
    public PivotPointActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.PivotPoint action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded) return;

        switch (action)
        {
            case EditorAction.PivotPoint.OnApplyPivotClicked onApplyPivotClicked:
                reducer.ApplyPatch(new EditorPatch.PivotPoint.NewPivotPoint(onApplyPivotClicked.pivotPoint));
                break;
            case EditorAction.PivotPoint.OnApplyPivotPointForAllSpritesClicked onApplyPivotPointForAllSpritesClicked:
                reducer.ApplyPatch(new EditorPatch.PivotPoint.ApplyPivotPointForAll());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
        
    }
}
}