using System;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ImportActionDelegate : ActionDelegate<EditorAction.Import>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;

    public ImportActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.repository = repository;
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.Import action)
    {
        switch (action)
        {
            case EditorAction.Import.OnImportClicked onImportClicked:
                OnImportClicked();
                break;
            case EditorAction.Import.OnFileSelected onFileSelected:
                OnFileSelected(onFileSelected);
                break;
            case EditorAction.Import.OnCanceled onCanceled:
                OnCanceled();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnImportClicked()
    {
        eventsConsumer.Consume(new EditorEvent.OpenBrowserForImport());
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
    }

    private void OnFileSelected(EditorAction.Import.OnFileSelected action)
    {
        var texture = repository.LoadTexture(action.path);
        if (texture != null)
        {
            reducer.ApplyPatch(new EditorPatch.Import.TextureSelected(texture));
        }
    }

    private void OnCanceled()
    {
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
}
}