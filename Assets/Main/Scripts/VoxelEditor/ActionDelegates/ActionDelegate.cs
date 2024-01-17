using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public abstract class ActionDelegate<T> where T : EditorAction
{
    protected EditorFeature feature;
    protected EditorReducer reducer;

    protected ActionDelegate(EditorFeature feature, EditorReducer reducer)
    {
        this.feature = feature;
        this.reducer = reducer;
    }

    public abstract void ApplyAction(EditorState state, T action);
}
}