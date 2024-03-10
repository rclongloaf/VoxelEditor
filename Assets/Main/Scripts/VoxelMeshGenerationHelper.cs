using System.Collections.Generic;
using Main.Scripts.Helpers.MeshGeneration;
using Main.Scripts.VoxelEditor.State.Vox;
using Unity.VisualScripting;
using UnityEngine;

namespace Main.Scripts
{
public static class VoxelMeshGenerationHelper
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

        foreach (var (voxelPosition, _) in spriteData.voxels)
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
    
    public static Mesh GenerateMeshByNormal(
        Normal normal,
        float size
    )
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var vertStartIndex = vertices.Count;

        var halfSize = size / 2;

        vertices.Add(new Vector3(-halfSize, -halfSize, -halfSize));
        vertices.Add(new Vector3(-halfSize, halfSize, -halfSize));
        vertices.Add(new Vector3(-halfSize, -halfSize, halfSize));
        vertices.Add(new Vector3(-halfSize, halfSize, halfSize));
        vertices.Add(new Vector3(halfSize, -halfSize, -halfSize));
        vertices.Add(new Vector3(halfSize, halfSize, -halfSize));
        vertices.Add(new Vector3(halfSize, -halfSize, halfSize));
        vertices.Add(new Vector3(halfSize, halfSize, halfSize));

        var LBB = vertStartIndex + 0;
        var LTB = vertStartIndex + 1;
        var LBF = vertStartIndex + 2;
        var LTF = vertStartIndex + 3;
        var RBB = vertStartIndex + 4;
        var RTB = vertStartIndex + 5;
        var RBF = vertStartIndex + 6;
        var RTF = vertStartIndex + 7;

        //back
        if (!normal.HasFlag(Normal.Back))
        {
            FillSide(
                normal,
                Normal.Left,
                Normal.Right,
                Normal.Up,
                Normal.Down,
                LTB,
                RTB,
                LBB,
                RBB,
                triangles
            );
        }

        //forward
        if (!normal.HasFlag(Normal.Forward))
        {
            FillSide(
                normal,
                Normal.Right,
                Normal.Left,
                Normal.Up,
                Normal.Down,
                RTF,
                LTF,
                RBF,
                LBF,
                triangles
            );
        }

        //left
        if (!normal.HasFlag(Normal.Left))
        {
            FillSide(
                normal,
                Normal.Forward,
                Normal.Back,
                Normal.Up,
                Normal.Down,
                LTF,
                LTB,
                LBF,
                LBB,
                triangles
            );
        }

        //right
        if (!normal.HasFlag(Normal.Right))
        {
            FillSide(
                normal,
                Normal.Back,
                Normal.Forward,
                Normal.Up,
                Normal.Down,
                RTB,
                RTF,
                RBB,
                RBF,
                triangles
            );
        }

        //top
        if (!normal.HasFlag(Normal.Up))
        {
            FillSide(
                normal,
                Normal.Left,
                Normal.Right,
                Normal.Forward,
                Normal.Back,
                LTF,
                RTF,
                LTB,
                RTB,
                triangles
            );
        }

        //bottom
        if (!normal.HasFlag(Normal.Down))
        {
            FillSide(
                normal,
                Normal.Left,
                Normal.Right,
                Normal.Back,
                Normal.Forward,
                LBB,
                RBB,
                LBF,
                RBF,
                triangles
            );
        }

        if (normal.IsCorner())
        {
            var normalX = normal.HasFlag(Normal.Right) ? 1 : (normal.HasFlag(Normal.Left) ? -1 : 0);
            var normalY = normal.HasFlag(Normal.Up) ? 1 : (normal.HasFlag(Normal.Down) ? -1 : 0);
            var normalZ = normal.HasFlag(Normal.Forward) ? 1 : (normal.HasFlag(Normal.Back) ? -1 : 0);
            
            switch (normalX, normalY, normalZ)
            {
                case (1, 1, 1):
                    triangles.Add(LTB);
                    triangles.Add(LBF);
                    triangles.Add(RBB);
                    break;
                case (-1, 1, 1):
                    triangles.Add(RTB);
                    triangles.Add(LBB);
                    triangles.Add(RBF);
                    break;
                case (1, -1, 1):
                    triangles.Add(LBB);
                    triangles.Add(RTB);
                    triangles.Add(LTF);
                    break;
                case (-1, -1, 1):
                    triangles.Add(LTB);
                    triangles.Add(RBB);
                    triangles.Add(RTF);
                    break;
                case (1, 1, -1):
                    triangles.Add(LBB);
                    triangles.Add(LTF);
                    triangles.Add(RBF);
                    break;
                case (-1, 1, -1):
                    triangles.Add(LBF);
                    triangles.Add(RTF);
                    triangles.Add(RBB);
                    break;
                case (1, -1, -1):
                    triangles.Add(LBF);
                    triangles.Add(LTB);
                    triangles.Add(RTF);
                    break;
                case (-1, -1, -1):
                    triangles.Add(RBF);
                    triangles.Add(LTF);
                    triangles.Add(RTB);
                    break;
            }
        }
        else
        {
            if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Up))
            {
                FillSquare(RTF, RTB, LBF, LBB, triangles);
            }
            if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Up))
            {
                FillSquare(LTB, LTF, RBB, RBF, triangles);
            }
            if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Down))
            {
                FillSquare(LTF, LTB, RBF, RBB, triangles);
            }
            if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Down))
            {
                FillSquare(RTB, RTF, LBB, LBF, triangles);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Up))
            {
                FillSquare(LTF, RTF, LBB, RBB, triangles);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Up))
            {
                FillSquare(RTB, LTB, RBF, LBF, triangles);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Down))
            {
                FillSquare(LTB, RTB, LBF, RBF, triangles);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Down))
            {
                FillSquare(RTF, LTF, RBB, LBB, triangles);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Right))
            {
                FillSquare(LBB, LTB, RBF, RTF, triangles);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Right))
            {
                FillSquare(RBB, RTB, LBF, LTF, triangles);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Left))
            {
                FillSquare(RTB, RBB, LTF, LBF, triangles);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Left))
            {
                FillSquare(RBF, RTF, LBB, LTB, triangles);
            }
        }
        

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        return mesh;
    }

    private static void FillSide(
        Normal normal,
        Normal left,
        Normal right,
        Normal up,
        Normal down,
        int LT,
        int RT,
        int LB,
        int RB,
        List<int> triangles
    )
    {
        if (!IsXYCorner(normal, left, right, up, down))
        {
            FillSquare(LT, RT, LB, RB, triangles);
            return;
        }
        if (normal.HasFlag(left) && normal.HasFlag(up))
        {
            triangles.Add(RB);
            triangles.Add(LB);
            triangles.Add(RT);
            return;
        }
        if (normal.HasFlag(right) && normal.HasFlag(up))
        {
            triangles.Add(LT);
            triangles.Add(RB);
            triangles.Add(LB);
            return;
        }
        if (normal.HasFlag(left) && normal.HasFlag(down))
        {
            triangles.Add(LT);
            triangles.Add(RT);
            triangles.Add(RB);
            return;
        }
        if (normal.HasFlag(right) && normal.HasFlag(down))
        {
            triangles.Add(LT);
            triangles.Add(RT);
            triangles.Add(LB);
            return;
        }
    }

    private static void MaybeFillCorner(
        Normal normal,
        Normal normalX,
        Normal normalY,
        Normal normalZ,
        int R,
        int F,
        int T,
        bool inv,
        List<int> triangles
    )
    {
        if (normal.HasFlag(normalX) && normal.HasFlag(normalY) && normal.HasFlag(normalZ))
        {
            if (inv)
            {
                triangles.Add(R);
                triangles.Add(F);
                triangles.Add(T);
            }
            else
            {
                triangles.Add(R);
                triangles.Add(T);
                triangles.Add(F);
            }
        }
    }

    private static void FillSquare(
        int LT,
        int RT,
        int LB,
        int RB,
        List<int> triangles
    )
    {
        triangles.Add(LT);
        triangles.Add(RT);
        triangles.Add(RB);
        triangles.Add(LT);
        triangles.Add(RB);
        triangles.Add(LB);
    }
    
    private static bool IsXYCorner(Normal normal, Normal left, Normal right, Normal up, Normal down)
    {
        if (normal.HasFlag(left) && normal.HasFlag(up))
        {
            return true;
        }

        if (normal.HasFlag(right) && normal.HasFlag(up))
        {
            return true;
        }

        if (normal.HasFlag(left) && normal.HasFlag(down))
        {
            return true;
        }

        if (normal.HasFlag(right) && normal.HasFlag(down))
        {
            return true;
        }

        return false;
    }
}
}