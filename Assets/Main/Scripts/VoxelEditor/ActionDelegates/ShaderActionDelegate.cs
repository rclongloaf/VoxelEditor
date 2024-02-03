using System;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ShaderActionDelegate : ActionDelegate<EditorAction.Shader>
{
    public ShaderActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.Shader action)
    {
        switch (action)
        {
            case EditorAction.Shader.OnToggleGridClicked onToggleGridClicked:
                reducer.ApplyPatch(new EditorPatch.Shader.ChangeGridEnabled(!state.shaderData.isGridEnabled));
                break;
            case EditorAction.Shader.OnToggleTransparentClicked onToggleTransparentClicked:
                reducer.ApplyPatch(new EditorPatch.Shader.ChangeTransparentEnabled(!state.shaderData.isTransparentEnabled));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}