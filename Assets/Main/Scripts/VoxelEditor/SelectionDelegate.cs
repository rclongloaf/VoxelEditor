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

    public void Select(HashSet<Vector3Int> selectedVoxels)
    {
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(new EditAction.Select(selectedVoxels)));
        reducer.ApplyPatch(new EditorPatch.VoxelsChanges.Delete(selectedVoxels));
        reducer.ApplyPatch(new EditorPatch.Selection.Select(selectedVoxels, Vector3Int.zero));
    }

    public void PasteBufferWithSelection(EditorState state, SpriteData spriteData)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return;
        
        var voxels = new HashSet<Vector3Int>();

        var deltaPivot = activeLayer.currentSpriteData.pivot - spriteData.pivot;

        var offset = Vector3Int.RoundToInt(new Vector3(deltaPivot.x, deltaPivot.y, 0));

        foreach (var voxel in spriteData.voxels)
        {
            voxels.Add(voxel + offset);
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