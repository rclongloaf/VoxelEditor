using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ShaderActionDelegate : ActionDelegate<EditorAction.Shader>
{
    public ShaderActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.Shader action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        switch (action)
        {
            case EditorAction.Shader.OnToggleGridClicked onToggleGridClicked:
                reducer.ApplyPatch(new EditorPatch.Shader.ChangeGridEnabled(!loadedState.shaderData.isGridEnabled));
                break;
            case EditorAction.Shader.OnToggleTransparentClicked onToggleTransparentClicked:
                reducer.ApplyPatch(new EditorPatch.Shader.ChangeTransparentEnabled(!loadedState.shaderData.isTransparentEnabled));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}