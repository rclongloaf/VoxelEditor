using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public class VoxData
{
    public readonly HashSet<Vector3Int> voxels;
    public readonly SpriteRectData spriteRectData;

    public VoxData(
        HashSet<Vector3Int> voxels,
        SpriteRectData spriteRectData
    )
    {
        this.voxels = voxels;
        this.spriteRectData = spriteRectData;
    }
}
}