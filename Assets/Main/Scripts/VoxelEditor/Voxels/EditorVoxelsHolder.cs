using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.Voxels
{
public class EditorVoxelsHolder
{
    private GameObject voxelPrefab;

    private Dictionary<Vector3Int, GameObject> currentVoxels = new();

    public EditorVoxelsHolder(GameObject voxelPrefab)
    {
        this.voxelPrefab = voxelPrefab;
    }

    public void ApplyVoxels(HashSet<Vector3Int> voxels)
    {
        foreach (var voxel in voxels)
        {
            if (!currentVoxels.ContainsKey(voxel))
            {
                AddVoxel(voxel);
            }
        }

        var voxelsToRemove = new List<Vector3Int>();

        foreach (var (voxel, _) in currentVoxels)
        {
            if (!voxels.Contains(voxel))
            {
                voxelsToRemove.Add(voxel);
            }
        }

        foreach (var voxel in voxelsToRemove)
        {
            RemoveVoxel(voxel);
        }
    }

    private void AddVoxel(Vector3Int position)
    {
        if (currentVoxels.ContainsKey(position))
        {
            return;
        }
        
        var voxel = Object.Instantiate(voxelPrefab);

        currentVoxels.Add(position, voxel);

        voxel.transform.position = Vector3.one * 0.5f + position;
    }

    private void RemoveVoxel(Vector3Int position)
    {
        if (currentVoxels.TryGetValue(position, out var voxel))
        {
            currentVoxels.Remove(position);
            Object.Destroy(voxel);
        }
    }
}
}