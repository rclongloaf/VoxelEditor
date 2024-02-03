using System;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class LoadTextureActionDelegate : ActionDelegate<EditorAction.LoadTexture>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;
    
    public LoadTextureActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.repository = repository;
        this.eventsConsumer = eventsConsumer;
    }

    public override void ApplyAction(EditorState state, EditorAction.LoadTexture action)
    {
        switch (action)
        {
            case EditorAction.LoadTexture.OnLoadClicked onLoadClicked:
                eventsConsumer.Consume(new EditorEvent.OpenBrowserForLoadTexture());
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
                break;
            case EditorAction.LoadTexture.OnPathSelected onPathSelected:
                OnPathSelected(state, onPathSelected);
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            case EditorAction.LoadTexture.OnCancel onCancel:
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnPathSelected(EditorState state, EditorAction.LoadTexture.OnPathSelected action)
    {
        var texture = repository.LoadTexture(action.path);
        if (texture != null)
        {
            reducer.ApplyPatch(new EditorPatch.TextureLoaded(texture));
        }
    }
}
}