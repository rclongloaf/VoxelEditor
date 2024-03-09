using System;
using System.Collections.Generic;
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

    public void Select(Dictionary<Vector3Int, VoxelData> selectedVoxels)
    {
        if (selectedVoxels.Count == 0) return;
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Select(selectedVoxels)));
        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(selectedVoxels));
        reducer.ApplyPatch(new EditorPatch.Selection.Select(selectedVoxels, Vector3Int.zero));
    }

    public void PasteBufferWithSelection(EditorState state, SpriteData spriteData)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;
        
        var voxels = new Dictionary<Vector3Int, VoxelData>();

        var deltaPivot = activeLayer.currentSpriteData.pivot - spriteData.pivot;

        var offset = Vector3Int.RoundToInt(new Vector3(deltaPivot.x, deltaPivot.y, 0));

        foreach (var (pos, voxelData) in spriteData.voxels)
        {
            voxels[pos + offset] = voxelData;
        }
            
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Paste(voxels)));
        reducer.ApplyPatch(new EditorPatch.Selection.Select(voxels, Vector3Int.zero));
    }

    public void CancelSelection(EditorState state)
    {
        if (state.activeLayer is not VoxLayerState.Loaded
            {
                selectionState: SelectionState.Selected selectionState
            } activeLayer) return;
        
            
        var voxels = new Dictionary<Vector3Int, VoxelData>();
        var textureData = activeLayer.voxData.textureData;
        foreach (var (pos, selectedVoxelData) in selectionState.voxels)
        {
            var voxel = pos + selectionState.offset;
            if (voxel.x < textureData.spriteWidth
                && voxel.x >= 0
                && voxel.y + voxel.z < textureData.spriteHeight
                && voxel.y + voxel.z >= 0
                && voxel.z < textureData.spriteHeight * 0.5
                && voxel.z >= -textureData.spriteHeight * 0.5)
            {
                voxels[voxel] = selectedVoxelData;
            }
        }

        var overrideVoxels = new Dictionary<Vector3Int, VoxelData>();

        foreach (var (pos, voxelData) in voxels)
        {
            if (activeLayer.currentSpriteData.voxels.ContainsKey(pos))
            {
                overrideVoxels[pos] = voxelData;
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

    public void DeleteSelected(EditorState state)
    {
        if (state.activeLayer is not VoxLayerState.Loaded
            {
                selectionState: SelectionState.Selected selectionState
            }) return;
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.DeleteSelected(selectionState)));
        reducer.ApplyPatch(new EditorPatch.Selection.CancelSelection());
    }
}
}