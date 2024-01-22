using System;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SaveVoxActionDelegate : ActionDelegate<EditorAction.SaveVox>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;

    public SaveVoxActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.repository = repository;
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.SaveVox action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        switch (action)
        {
            case EditorAction.SaveVox.OnSaveClicked onSaveClicked:
                eventsConsumer.Consume(new EditorEvent.OpenBrowserForSaveVox());
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
                break;
            case EditorAction.SaveVox.OnPathSelected onPathSelected:
                repository.SaveVoxFile(onPathSelected.path, loadedState.voxData);
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            case EditorAction.SaveVox.OnCanceled onCanceled:
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}