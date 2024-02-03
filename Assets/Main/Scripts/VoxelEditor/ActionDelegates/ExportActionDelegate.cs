using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

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
        if (state.activeLayer is not VoxLayerState.Loaded) return;
        
    }
}
}