using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.Helpers.MeshGeneration
{
public class PointsMeshFromVoxelsGenerator : MeshGenerator
{
    private Dictionary<Vector3Int, VoxelData> voxels;
    private int textureWidth;
    private int textureHeight;
    private TextureData textureData;
    private SpriteIndex spriteIndex;
    private Vector2 pivotPoint;
    private float pixelsPerUnit;

    public PointsMeshFromVoxelsGenerator(
        Dictionary<Vector3Int, VoxelData> voxels,
        int textureWidth,
        int textureHeight,
        TextureData textureData,
        SpriteIndex spriteIndex,
        Vector2 pivotPoint,
        float pixelsPerUnit
    )
    {
        this.voxels = new Dictionary<Vector3Int, VoxelData>(voxels);
        this.voxels.FillEmptySpaces();
        this.textureWidth = textureWidth;
        this.textureHeight = textureHeight;
        this.textureData = textureData;
        this.spriteIndex = spriteIndex;
        this.pivotPoint = pivotPoint;
        this.pixelsPerUnit = pixelsPerUnit;
    }

    public Mesh? GenerateMesh()
    {
        try
        {
            var mesh = new Mesh();

            var spriteWidth = textureWidth / textureData.columnsCount;
            var spriteHeight = textureHeight / textureData.rowsCount;

            var verticesList = new List<Vector3Int>();

            foreach (var (position, _) in voxels)
            {
                if (HasEmptySide(position))
                {
                    verticesList.Add(position);
                }
            }

            var boundsMin = Vector3.one * float.MaxValue;
            var boundsMax = Vector3.one * float.MinValue;

            foreach (var vert in verticesList)
            {
                boundsMin = new Vector3(
                    Math.Min(boundsMin.x, vert.x),
                    Math.Min(boundsMin.y, vert.y),
                    Math.Min(boundsMin.z, vert.z)
                );
                boundsMax = new Vector3(
                    Math.Max(boundsMax.x, vert.x),
                    Math.Max(boundsMax.y, vert.y),
                    Math.Max(boundsMax.z, vert.z)
                );
            }

            mesh.vertices = verticesList.ConvertAll(ApplyScaleTransform).ToArray();
            mesh.uv = verticesList.ConvertAll(vert => new Vector2(
                (vert.x + 0.5f + spriteWidth * spriteIndex.columnIndex) / (float)textureWidth,
                (vert.y + 0.5f + vert.z + spriteHeight * (textureData.rowsCount - spriteIndex.rowIndex - 1)) / (float)textureHeight
            )).ToArray();
            mesh.bounds = new Bounds(
                Vector3.zero,
                (boundsMax - boundsMin) * 2 / pixelsPerUnit
            );
            return mesh;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    private bool HasEmptySide(Vector3Int position)
    {
        return !voxels.ContainsKey(position + Vector3Int.up)
               || !voxels.ContainsKey(position + Vector3Int.down)
               || !voxels.ContainsKey(position + Vector3Int.right)
               || !voxels.ContainsKey(position + Vector3Int.left)
               || !voxels.ContainsKey(position + Vector3Int.forward)
               || !voxels.ContainsKey(position + Vector3Int.back);
    }

    private Vector3 ApplyScaleTransform(Vector3Int vertex)
    {
        return new Vector3(
            vertex.x - pivotPoint.x + 0.5f,
            vertex.y - pivotPoint.y + 0.5f,
            vertex.z + 0.5f
        ) / pixelsPerUnit;
    }
}
}