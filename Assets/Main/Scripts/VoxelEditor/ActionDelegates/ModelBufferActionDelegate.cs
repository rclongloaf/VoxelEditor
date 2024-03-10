using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class ModelBufferActionDelegate : ActionDelegate<EditorAction.ModelBuffer>
{
    private SelectionDelegate selectionDelegate;
    
    public ModelBufferActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        SelectionDelegate selectionDelegate
    ) : base(feature, reducer)
    {
        this.selectionDelegate = selectionDelegate;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.ModelBuffer action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;

        switch (action)
        {
            case EditorAction.ModelBuffer.OnCopyClicked onCopyClicked:
                OnCopyClicked(state, activeLayer);
                break;
            case EditorAction.ModelBuffer.OnPasteClicked onPasteClicked:
                OnPasteClicked(state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnCopyClicked(EditorState state, VoxLayerState.Loaded activeLayer)
    {
        if (activeLayer.selectionState is SelectionState.Selected selectionState)
        {
            var selectedVoxels = new Dictionary<Vector3Int, VoxelData>();

            foreach (var (pos, voxelData) in selectionState.voxels)
            {
                selectedVoxels[pos + selectionState.offset] = voxelData;
            }
            
            var selectedSpriteData = activeLayer.currentSpriteData with
            {
                voxels = selectedVoxels
            };
            reducer.ApplyPatch(new EditorPatch.ModelBuffer.Copy(selectedSpriteData));
        }
        else
        {
            reducer.ApplyPatch(new EditorPatch.ModelBuffer.Copy(activeLayer.currentSpriteData));
        }
    }

    private void OnPasteClicked(EditorState state)
    {
        if (state.bufferedSpriteData != null)
        {
            selectionDelegate.CancelSelection(state);

            selectionDelegate.PasteBufferWithSelection(state, state.bufferedSpriteData);
        }
    }
}
}