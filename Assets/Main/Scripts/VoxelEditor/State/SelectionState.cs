using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface SelectionState
{
    public record None : SelectionState;

    public record Selected(
        IEnumerable<Vector3Int> voxels,
        Vector3Int offset
    ) : SelectionState;
}
}