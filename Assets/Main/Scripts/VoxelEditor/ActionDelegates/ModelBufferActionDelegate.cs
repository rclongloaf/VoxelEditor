using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ModelBufferActionDelegate : ActionDelegate<EditorAction.ModelBuffer>
{
    public ModelBufferActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.ModelBuffer action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.ModelBuffer.OnCopyClicked onCopyClicked:
                reducer.ApplyPatch(new EditorPatch.ModelBuffer.Copy(activeLayer.currentSpriteData));
                break;
            case EditorAction.ModelBuffer.OnPasteClicked onPasteClicked:
                if (state.bufferedSpriteData != null)
                {
                    reducer.ApplyPatch(new EditorPatch.ModelBuffer.Paste(state.bufferedSpriteData));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}