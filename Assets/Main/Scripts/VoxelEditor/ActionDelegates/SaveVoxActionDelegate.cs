using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SaveVoxActionDelegate : ActionDelegate<EditorAction.SaveVox>
{
    private EditorEventsConsumer eventsConsumer;

    public SaveVoxActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.SaveVox action)
    {
        if (state is not EditorState.Loaded) return;
        
        
    }
}
}