using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface EditAction
{
    public record Add(Dictionary<Vector3Int, VoxelData> voxels) : EditAction;
    
    public record Delete(Dictionary<Vector3Int, VoxelData> voxels) : EditAction;

    public record ChangeSmooth(Dictionary<Vector3Int, bool> voxelsSmoothMap) : EditAction;

    public record Select(Dictionary<Vector3Int, VoxelData> voxels) : EditAction;

    public record Paste(Dictionary<Vector3Int, VoxelData> voxels) : EditAction;

    public record CancelSelection(
        Dictionary<Vector3Int, VoxelData> voxels,
        Vector3Int offset,
        Dictionary<Vector3Int, VoxelData> overrideVoxels
    ) : EditAction;

    public record DeleteSelected(SelectionState.Selected selectedState) : EditAction;

    public record MoveSelection(
        Vector3Int deltaOffset
    ) : EditAction;

    public record RotateVoxels(
        Dictionary<Vector3Int, VoxelData> fromVoxels,
        Dictionary<Vector3Int, VoxelData> toVoxels
    ) : EditAction;
}
}