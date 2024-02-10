using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface EditAction
{
    public record Add(IEnumerable<Vector3Int> voxels) : EditAction;
    
    public record Delete(IEnumerable<Vector3Int> voxels) : EditAction;

    public record Select(IEnumerable<Vector3Int> voxels) : EditAction;

    public record Paste(IEnumerable<Vector3Int> voxels) : EditAction;

    public record CancelSelection(
        IEnumerable<Vector3Int> voxels,
        Vector3Int offset,
        IEnumerable<Vector3Int> overrideVoxels
    ) : EditAction;

    public record DeleteSelected(SelectionState.Selected selectedState) : EditAction;

    public record MoveSelection(
        Vector3Int deltaOffset
    ) : EditAction;
}
}