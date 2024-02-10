﻿using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor
{
public class SelectionDelegate
{
    private EditorReducer reducer;
    
    public SelectionDelegate(EditorReducer reducer)
    {
        this.reducer = reducer;
    }

    public void CancelSelection(EditorState state)
    {
        if (state.activeLayer is not VoxLayerState.Loaded
            {
                selectionState: SelectionState.Selected selectionState
            } activeLayer) return;
        
            
        var voxels = new List<Vector3Int>();
        foreach (var voxel in selectionState.voxels)
        {
            voxels.Add(voxel + selectionState.offset);
        }

        var overrideVoxels = new List<Vector3Int>();

        foreach (var voxel in voxels)
        {
            if (activeLayer.currentSpriteData.voxels.Contains(voxel))
            {
                overrideVoxels.Add(voxel);
            }
        }
            
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.CancelSelection(
            selectionState.voxels,
            selectionState.offset,
            overrideVoxels
        )));
        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Add(voxels));
        reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
    }
}
}