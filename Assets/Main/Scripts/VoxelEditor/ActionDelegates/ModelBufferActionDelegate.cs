using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ModelBufferActionDelegate : ActionDelegate<EditorAction.ModelBuffer>
{
    public ModelBufferActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.ModelBuffer action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        switch (action)
        {
            case EditorAction.ModelBuffer.OnCopyClicked onCopyClicked:
                reducer.ApplyPatch(new EditorPatch.ModelBuffer.Copy(loadedState.currentSpriteData));
                break;
            case EditorAction.ModelBuffer.OnPasteClicked onPasteClicked:
                if (loadedState.bufferedSpriteData != null)
                {
                    reducer.ApplyPatch(new EditorPatch.ModelBuffer.Paste(loadedState.bufferedSpriteData));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}