using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.Helpers
{
public static class VoxelsHelper
{
    public static void FillEmptySpaces(this Dictionary<Vector3Int, VoxelData> voxels)
    {
        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        var minZ = int.MaxValue;
        var maxZ = int.MinValue;
        
        foreach (var (pos, _) in voxels)
        {
            minX = Math.Min(minX, pos.x);
            maxX = Math.Max(maxX, pos.x);
            minY = Math.Min(minY, pos.y);
            maxY = Math.Max(maxY, pos.y);
            minZ = Math.Min(minZ, pos.z);
            maxZ = Math.Max(maxZ, pos.z);
        }

        var checkedSet = new HashSet<Vector3Int>();
        var queue = new Queue<Vector3Int>();
        var shouldFill = new HashSet<Vector3Int>();

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                for (var z = minZ; z <= maxZ; z++)
                {
                    var vox = new Vector3Int(x, y, z);
                    if (checkedSet.Contains(vox)) continue;
                    if (voxels.ContainsKey(vox))
                    {
                        checkedSet.Add(vox);
                        continue;
                    }

                    queue.Clear();
                    queue.Enqueue(vox);
                    var isFill = true;
                    
                    while (queue.TryDequeue(out var pos))
                    {
                        if (!checkedSet.Add(pos)) continue;
                        shouldFill.Add(pos);
                        
                        if (voxels.ContainsKey(pos))
                        {
                            continue;
                        }

                        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY || pos.z < minZ || pos.z > maxZ)
                        {
                            isFill = false;
                            continue;
                        }

                        var voxUp = pos + Vector3Int.up;
                        var voxDown = pos + Vector3Int.down;
                        var voxRight = pos + Vector3Int.right;
                        var voxLeft = pos + Vector3Int.left;
                        var voxForward = pos + Vector3Int.forward;
                        var voxBack = pos + Vector3Int.back;

                        queue.Enqueue(voxUp);
                        queue.Enqueue(voxDown);
                        queue.Enqueue(voxRight);
                        queue.Enqueue(voxLeft);
                        queue.Enqueue(voxForward);
                        queue.Enqueue(voxBack);
                    }

                    if (isFill)
                    {
                        foreach (var pos in shouldFill)
                        {
                            voxels.TryAdd(pos, new VoxelData(false));
                        }
                    }

                    shouldFill.Clear();
                }
            }
        }
        
    }
}
}