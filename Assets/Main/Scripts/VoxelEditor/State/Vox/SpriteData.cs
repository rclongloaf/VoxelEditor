using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State.Vox
{
public record SpriteData(
    Vector2 pivot,
    HashSet<Vector3Int> voxels
);
}