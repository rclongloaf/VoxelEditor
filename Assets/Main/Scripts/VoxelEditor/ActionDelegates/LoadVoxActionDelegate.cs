using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class LoadVoxActionDelegate : ActionDelegate<EditorAction.LoadVox>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;

    public LoadVoxActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.repository = repository;
        this.eventsConsumer = eventsConsumer;
    }

    public override void ApplyAction(EditorState state, EditorAction.LoadVox action)
    {
        switch (action)
        {
            case EditorAction.LoadVox.OnLoadClicked:
                OnClicked();
                break;
            case EditorAction.LoadVox.OnFileSelected selectedAction:
                OnFileSelected(selectedAction);
                break;
            case EditorAction.LoadVox.OnCanceled:
                OnCanceled();
                break;
        }
    }

    private void OnClicked()
    {
        eventsConsumer.Consume(new EditorEvent.OpenBrowserForLoadVox());
        
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
    }

    private void OnFileSelected(EditorAction.LoadVox.OnFileSelected action)
    {
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());

        if (repository.LoadVoxFile(action.path, out var voxels))
        {
            reducer.ApplyPatch(new EditorPatch.VoxLoaded(voxels));
        }
    }

    private void OnCanceled()
    {
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
}
}