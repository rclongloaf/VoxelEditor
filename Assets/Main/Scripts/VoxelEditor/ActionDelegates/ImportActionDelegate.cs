using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ImportActionDelegate : ActionDelegate<EditorAction.Import>
{
    private EditorEventsConsumer eventsConsumer;

    public ImportActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.Import action)
    {
        
    }
}
}