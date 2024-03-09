using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface SelectionState
{
    public record None : SelectionState;

    public record Selected(
        Dictionary<Vector3Int, VoxelData> voxels,
        Vector3Int offset
    ) : SelectionState;
}
}