using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public record VoxData(
    HashSet<Vector3Int> voxels,
    SpriteRectData spriteRectData
);
}