using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.Helpers.MeshGeneration
{
public static class VoxelsSmoothHelper
{
    public static Dictionary<Vector3Int, Normal> GetSmoothNormalMap(Dictionary<Vector3Int, VoxelData> voxels)
    {
        var smoothNormalsMap = new Dictionary<Vector3Int, Normal>();
        foreach (var (voxel, voxelData) in voxels)
        {
            if (!voxelData.isSmooth)
            {
                smoothNormalsMap[voxel] = 0;
                continue;
            }
            
            var hasLeft = voxels.ContainsKey(voxel + Vector3Int.left);
            var hasRight = voxels.ContainsKey(voxel + Vector3Int.right);
            var hasTop = voxels.ContainsKey(voxel + Vector3Int.up);
            var hasBottom = voxels.ContainsKey(voxel + Vector3Int.down);
            var hasForward = voxels.ContainsKey(voxel + Vector3Int.forward);
            var hasBack = voxels.ContainsKey(voxel + Vector3Int.back);

            Normal normal = 0;

            Normal normalX = 0;
            Normal normalY = 0;
            Normal normalZ = 0;

            if (hasLeft != hasRight)
            {
                normalX |= hasLeft
                    ? Normal.Right
                    : Normal.Left;
            }

            if (hasTop != hasBottom)
            {
                normalY |= hasTop
                    ? Normal.Down
                    : Normal.Up;
            }

            if (hasForward != hasBack)
            {
                normalZ |= hasForward
                    ? Normal.Back
                    : Normal.Forward;
            }

            if ((normalX != 0 && normalY != 0) || (normalX != 0 && normalZ != 0) || (normalY != 0 && normalZ != 0))
            {
                normal |= normalX | normalY | normalZ;
            }

            if (voxels.ContainsKey(voxel + new Vector3Int(
                    normal.HasFlag(Normal.Right) ? 1 : normal.HasFlag(Normal.Left) ? -1 : 0,
                    normal.HasFlag(Normal.Up) ? 1 : normal.HasFlag(Normal.Down) ? -1 : 0,
                    normal.HasFlag(Normal.Forward) ? 1 : normal.HasFlag(Normal.Back) ? -1 : 0
                )))
            {
                normal = 0;
            }

            smoothNormalsMap[voxel] = normal;
        }

        foreach (var (voxel, voxelData) in voxels)
        {
            if (!voxelData.isSmooth)
            {
                smoothNormalsMap[voxel] = 0;
                continue;
            }
            
            var x = voxel.x;
            var y = voxel.y;
            var z = voxel.z;

            var hasLeft = voxels.ContainsKey(new Vector3Int(x - 1, y, z));
            var hasRight = voxels.ContainsKey(new Vector3Int(x + 1, y, z));
            var hasTop = voxels.ContainsKey(new Vector3Int(x, y + 1, z));
            var hasBottom = voxels.ContainsKey(new Vector3Int(x, y - 1, z));
            var hasForward = voxels.ContainsKey(new Vector3Int(x, y, z + 1));
            var hasBack = voxels.ContainsKey(new Vector3Int(x, y, z - 1));

            var normal = smoothNormalsMap[voxel];

            Normal normalX = 0;
            Normal normalY = 0;
            Normal normalZ = 0;

            if (hasLeft != hasRight)
            {
                normalX |= hasLeft ? Normal.Right : Normal.Left;
            }

            if (hasTop != hasBottom)
            {
                normalY |= hasTop ? Normal.Down : Normal.Up;
            }

            if (hasForward != hasBack)
            {
                normalZ |= hasForward ? Normal.Back : Normal.Forward;
            }

            if (normalX != 0 && normalY != 0 && normalZ != 0)
            {
                var hasSideX = smoothNormalsMap.TryGetValue(new Vector3Int(voxel.x - (normalX == Normal.Right ? 1 : -1), voxel.y, voxel.z), out var sideXNormal) && sideXNormal != 0;
                var hasSideY = smoothNormalsMap.TryGetValue(new Vector3Int(voxel.x, voxel.y - (normalY == Normal.Up ? 1 : -1), voxel.z), out var sideYNormal) && sideYNormal != 0;
                var hasSideZ = smoothNormalsMap.TryGetValue(new Vector3Int(voxel.x, voxel.y, voxel.z - (normalZ == Normal.Forward ? 1 : -1)), out var sideZNormal) && sideZNormal != 0;

                if (hasSideX && hasSideY && hasSideZ)
                {
                    normal = normalX | normalY | normalZ;
                }
                else
                {
                    var sideXCommonNormals = sideXNormal & normal;
                    var sideYCommonNormals = sideYNormal & normal;
                    var sideZCommonNormals = sideZNormal & normal;

                    if ((sideXCommonNormals.HasFlag(Normal.Up) || sideXCommonNormals.HasFlag(Normal.Down))
                        && (sideXCommonNormals.HasFlag(Normal.Forward) || sideXCommonNormals.HasFlag(Normal.Back)))
                    {
                        normal = sideXCommonNormals;
                    }
                    else if ((sideZCommonNormals.HasFlag(Normal.Up) || sideZCommonNormals.HasFlag(Normal.Down))
                             && (sideZCommonNormals.HasFlag(Normal.Right) || sideZCommonNormals.HasFlag(Normal.Left)))
                    {
                        normal = sideZCommonNormals;
                    }
                    else if ((sideYCommonNormals.HasFlag(Normal.Right) || sideYCommonNormals.HasFlag(Normal.Left))
                             && (sideYCommonNormals.HasFlag(Normal.Forward) || sideYCommonNormals.HasFlag(Normal.Back)))
                    {
                        normal = sideYCommonNormals;
                    }
                    else
                    {
                        normal = 0;
                    }
                }

                smoothNormalsMap[voxel] = normal;
            }
        }

        return smoothNormalsMap;
    }

    public static Dictionary<Normal, Mesh> GenerateMeshesPerNormals()
    {
        var meshesMap = new Dictionary<Normal, Mesh>();
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    var normal = GetNormal(new Vector3Int(x, y, z));
                    if (meshesMap.ContainsKey(normal)) continue;

                    meshesMap[normal] = VoxelMeshGenerationHelper.GenerateMeshByNormal(normal, 1f);
                }
            }
        }

        return meshesMap;
    }
    
    public static bool IsCorner(this Normal normal)
    {
        var normalX = normal.HasFlag(Normal.Right) || normal.HasFlag(Normal.Left);
        var normalY = normal.HasFlag(Normal.Up) || normal.HasFlag(Normal.Down);
        var normalZ = normal.HasFlag(Normal.Forward) || normal.HasFlag(Normal.Back);

        return normalX && normalY && normalZ;
    }

    private static Normal GetNormal(Vector3Int direction)
    {
        var normal = (Normal)0;
        normal |= (direction.x switch
        {
            -1 => Normal.Left,
            1 => Normal.Right,
            _ => 0
        });
        
        normal |= (direction.y switch
        {
            -1 => Normal.Down,
            1 => Normal.Up,
            _ => 0
        });
        
        normal |= (direction.z switch
        {
            -1 => Normal.Back,
            1 => Normal.Forward,
            _ => 0
        });
        return normal;
    }
}
}