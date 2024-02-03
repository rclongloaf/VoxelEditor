using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface EditAction
{
    public record Add(List<Vector3Int> voxels) : EditAction;
    public record Delete(List<Vector3Int> voxels) : EditAction;
}
}