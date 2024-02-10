using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ExportActionDelegate : ActionDelegate<EditorAction.Export>
{
    private EditorRepository repository;
    private EditorEventsConsumer eventsConsumer;
    private SelectionDelegate selectionDelegate;

    public ExportActionDelegate(
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
    
    public override void ApplyAction(EditorState state, EditorAction.Export action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.Export.OnCanceled onCanceled:
                OnCanceled();
                break;
            case EditorAction.Export.OnExportClicked onExportClicked:
                OnExportClicked(state, activeLayer);
                break;
            case EditorAction.Export.OnPathSelected onPathSelected:
                OnPathSelected(activeLayer, onPathSelected);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnExportClicked(EditorState state, VoxLayerState.Loaded activeLayer)
    {
        selectionDelegate.CancelSelection(state);
        
        eventsConsumer.Consume(new EditorEvent.OpenBrowserForExport());
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
    }

    private void OnPathSelected(VoxLayerState.Loaded loadedLayer, EditorAction.Export.OnPathSelected action)
    {
        repository.ExportMesh(
            action.path,
            loadedLayer.currentSpriteData.voxels,
            loadedLayer.texture?.width ?? 1,
            loadedLayer.texture?.height ?? 1,
            loadedLayer.voxData.textureData,
            loadedLayer.currentSpriteIndex,
            loadedLayer.currentSpriteData.pivot,
            20
        );
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
    
    private void OnCanceled()
    {
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
}
}