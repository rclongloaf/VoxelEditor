using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ExportActionDelegate : ActionDelegate<EditorAction.Export>
{
    private EditorEventsConsumer eventsConsumer;

    public ExportActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.Export action)
    {
        if (state is not EditorState.Loaded) return;
        
    }
}
}