using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts
{
public static class MeshGenerationHelper
{
    public static Mesh GenerateMesh(
        SpriteData spriteData,
        float pixelsPerUnit,
        TextureData textureData,
        SpriteIndex spriteIndex,
        int textureWidth,
        int textureHeight
    )
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        var normals = new List<Vector3>();

        var voxSize = 1 / pixelsPerUnit;
        var pivot = spriteData.pivot;
        var centerOffset = new Vector3(pivot.x, pivot.y, 0);
        var spriteWidth = textureWidth / textureData.columnsCount;
        var spriteHeight = textureHeight / textureData.rowsCount;
        var rect = new Vector2(
            x: spriteWidth * spriteIndex.columnIndex,
            y: spriteHeight * (textureData.rowsCount - spriteIndex.rowIndex - 1)
        );

        foreach (var voxelPosition in spriteData.voxels)
        {
            AddCubeVert(voxelPosition, 1, vertices, triangles);
        }

        for (var i = 0; i < vertices.Count; i++)
        {
            var vert = vertices[i];
            var pixelPos = new Vector2(
                vert.x + rect.x,
                vert.y + vert.z + rect.y
            );
            uv.Add(new Vector2(pixelPos.x / textureWidth, pixelPos.y / textureHeight));
            normals.Add(Vector3.back);
        }

        var mesh = new Mesh();
        mesh.vertices = vertices.ConvertAll(vert => (vert - centerOffset) * voxSize).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }

    private static void AddCubeVert(Vector3 position, float size, List<Vector3> vertices, List<int> triangles)
    {
        var vertStartIndex = vertices.Count;

        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y + size, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z + size));
        vertices.Add(new Vector3(position.x, position.y + size, position.z + size));
        vertices.Add(new Vector3(position.x + size, position.y, position.z));
        vertices.Add(new Vector3(position.x + size, position.y + size, position.z));
        vertices.Add(new Vector3(position.x + size, position.y, position.z + size));
        vertices.Add(new Vector3(position.x + size, position.y + size, position.z + size));

        var LBB = vertStartIndex + 0;
        var LTB = vertStartIndex + 1;
        var LBF = vertStartIndex + 2;
        var LTF = vertStartIndex + 3;
        var RBB = vertStartIndex + 4;
        var RTB = vertStartIndex + 5;
        var RBF = vertStartIndex + 6;
        var RTF = vertStartIndex + 7;

        triangles.Add(LTB);
        triangles.Add(RTB);
        triangles.Add(RBB);
        triangles.Add(LTB);
        triangles.Add(RBB);
        triangles.Add(LBB);

        triangles.Add(RTF);
        triangles.Add(LTF);
        triangles.Add(LBF);
        triangles.Add(RTF);
        triangles.Add(LBF);
        triangles.Add(RBF);

        triangles.Add(LTF);
        triangles.Add(LTB);
        triangles.Add(LBB);
        triangles.Add(LTF);
        triangles.Add(LBB);
        triangles.Add(LBF);

        triangles.Add(RTB);
        triangles.Add(RTF);
        triangles.Add(RBF);
        triangles.Add(RTB);
        triangles.Add(RBF);
        triangles.Add(RBB);

        triangles.Add(LTF);
        triangles.Add(RTF);
        triangles.Add(RTB);
        triangles.Add(LTF);
        triangles.Add(RTB);
        triangles.Add(LTB);

        triangles.Add(LBB);
        triangles.Add(RBB);
        triangles.Add(RBF);
        triangles.Add(LBB);
        triangles.Add(RBF);
        triangles.Add(LBF);
    }
}
}