using System;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SaveVoxActionDelegate : ActionDelegate<EditorAction.SaveVox>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;
    private SelectionDelegate selectionDelegate;

    public SaveVoxActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository,
        EditorEventsConsumer eventsConsumer,
        SelectionDelegate selectionDelegate
    ) : base(feature, reducer)
    {
        this.repository = repository;
        this.eventsConsumer = eventsConsumer;
        this.selectionDelegate = selectionDelegate;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.SaveVox action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.SaveVox.OnSaveClicked onSaveClicked:
                OnSaveClicked(state, activeLayer, onSaveClicked);
                break;
            case EditorAction.SaveVox.OnPathSelected onPathSelected:
                repository.SaveVoxFile(onPathSelected.path, activeLayer.voxData);
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            case EditorAction.SaveVox.OnCanceled onCanceled:
                reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnSaveClicked(EditorState state, VoxLayerState.Loaded activeLayer, EditorAction.SaveVox.OnSaveClicked action)
    {
        selectionDelegate.CancelSelection(state);
        
        if (activeLayer.currentSpriteData == activeLayer.voxData.sprites[activeLayer.currentSpriteIndex])
        {
            eventsConsumer.Consume(new EditorEvent.OpenBrowserForSaveVox());
            reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
        }
        else
        {
            reducer.ApplyPatch(new EditorPatch.SpriteChanges.ApplyRequest());
        }
    }
}
}