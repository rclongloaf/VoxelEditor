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
            case EditorAction.Export.Single single:
                OnSingleAction(state, activeLayer, single);
                break;
            case EditorAction.Export.All all:
                OnAllAction(state, activeLayer, all);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnSingleAction(EditorState state, VoxLayerState.Loaded activeLayer, EditorAction.Export.Single action)
    {
        switch (action)
        {
            case EditorAction.Export.Single.OnCanceled onCanceled:
                OnCanceled();
                break;
            case EditorAction.Export.Single.OnClicked onExportClicked:
                OnExportClicked(state, activeLayer, onExportClicked);
                break;
            case EditorAction.Export.Single.OnPathSelected onPathSelected:
                OnSinglePathSelected(activeLayer, onPathSelected);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnAllAction(EditorState state, VoxLayerState.Loaded activeLayer, EditorAction.Export.All action)
    {
        switch (action)
        {
            case EditorAction.Export.All.OnCanceled onCanceled:
                OnCanceled();
                break;
            case EditorAction.Export.All.OnClicked onExportClicked:
                OnExportClicked(state, activeLayer, onExportClicked);
                break;
            case EditorAction.Export.All.OnPathSelected onPathSelected:
                OnAllPathSelected(activeLayer, onPathSelected);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnExportClicked(EditorState state, VoxLayerState.Loaded activeLayer, EditorAction.Export action)
    {
        selectionDelegate.CancelSelection(state);

        if (activeLayer.currentSpriteData != activeLayer.voxData.sprites[activeLayer.currentSpriteIndex])
        {
            reducer.ApplyPatch(new EditorPatch.SpriteChanges.ApplyRequest(action));
            return;
        }

        switch (action)
        {
            case EditorAction.Export.All all:
                eventsConsumer.Consume(new EditorEvent.OpenBrowserForExportAll());
                break;
            case EditorAction.Export.Single single:
                eventsConsumer.Consume(new EditorEvent.OpenBrowserForExportSingle());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Opened());
    }

    private void OnSinglePathSelected(VoxLayerState.Loaded loadedLayer, EditorAction.Export.Single.OnPathSelected action)
    {
        repository.ExportMesh(
            action.path.Split('.')[0],
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

    private void OnAllPathSelected(VoxLayerState.Loaded loadedLayer, EditorAction.Export.All.OnPathSelected action)
    {
        var fileName = action.path.Split('.')[0];
        foreach (var (spriteIndex, spriteData) in loadedLayer.voxData.sprites)
        {
            repository.ExportMesh(
                $"{fileName}_{spriteIndex.rowIndex + 1}_{spriteIndex.columnIndex + 1}",
                spriteData.voxels,
                loadedLayer.texture?.width ?? 1,
                loadedLayer.texture?.height ?? 1,
                loadedLayer.voxData.textureData,
                spriteIndex,
                spriteData.pivot,
                20
            );
        }
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
    
    private void OnCanceled()
    {
        reducer.ApplyPatch(new EditorPatch.FileBrowser.Closed());
    }
}
}