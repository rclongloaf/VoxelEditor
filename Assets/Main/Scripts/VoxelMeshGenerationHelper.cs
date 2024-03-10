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
        var uv = new List<Vector2>();
        var normals = new List<Vector3>();

        var halfSize = size / 2;

        var LBB = new Vector3(-halfSize, -halfSize, -halfSize);
        var LTB = new Vector3(-halfSize, halfSize, -halfSize);
        var LBF = new Vector3(-halfSize, -halfSize, halfSize);
        var LTF = new Vector3(-halfSize, halfSize, halfSize);
        var RBB = new Vector3(halfSize, -halfSize, -halfSize);
        var RTB = new Vector3(halfSize, halfSize, -halfSize);
        var RBF = new Vector3(halfSize, -halfSize, halfSize);
        var RTF = new Vector3(halfSize, halfSize, halfSize);
        
        var normalX = normal.HasFlag(Normal.Right) ? 1 : (normal.HasFlag(Normal.Left) ? -1 : 0);
        var normalY = normal.HasFlag(Normal.Up) ? 1 : (normal.HasFlag(Normal.Down) ? -1 : 0);
        var normalZ = normal.HasFlag(Normal.Forward) ? 1 : (normal.HasFlag(Normal.Back) ? -1 : 0);

        var normalVec = new Vector3(normalX, normalY, normalZ);

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
                Vector3.back,
                vertices,
                triangles,
                uv,
                normals
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
                Vector3.forward,
                vertices,
                triangles,
                uv,
                normals
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
                Vector3.left,
                vertices,
                triangles,
                uv,
                normals
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
                Vector3.right,
                vertices,
                triangles,
                uv,
                normals
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
                Vector3.up,
                vertices,
                triangles,
                uv,
                normals
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
                Vector3.down,
                vertices,
                triangles,
                uv,
                normals
            );
        }

        if (normal.IsCorner())
        {
            
            switch (normalX, normalY, normalZ)
            {
                case (1, 1, 1):
                    FillTriangle(
                        LTB,
                        LBF,
                        RBB,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (-1, 1, 1):
                    FillTriangle(
                        RTB,
                        LBB,
                        RBF,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (1, -1, 1):
                    FillTriangle(
                        LBB,
                        RTB,
                        LTF,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (-1, -1, 1):
                    FillTriangle(
                        LTB,
                        RBB,
                        RTF,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (1, 1, -1):
                    FillTriangle(
                        LBB,
                        LTF,
                        RBF,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (-1, 1, -1):
                    FillTriangle(
                        LBF,
                        RTF,
                        RBB,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (1, -1, -1):
                    FillTriangle(
                        LBF,
                        LTB,
                        RTF,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
                case (-1, -1, -1):
                    FillTriangle(
                        RBF,
                        LTF,
                        RTB,
                        normalVec,
                        vertices,
                        triangles,
                        uv,
                        normals
                    );
                    break;
            }
        }
        else
        {
            if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Up))
            {
                FillSquare(normalVec,RTF, RTB, LBF, LBB, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Up))
            {
                FillSquare(normalVec,LTB, LTF, RBB, RBF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Down))
            {
                FillSquare(normalVec,LTF, LTB, RBF, RBB, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Down))
            {
                FillSquare(normalVec,RTB, RTF, LBB, LBF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Up))
            {
                FillSquare(normalVec,LTF, RTF, LBB, RBB, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Up))
            {
                FillSquare(normalVec,RTB, LTB, RBF, LBF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Down))
            {
                FillSquare(normalVec,LTB, RTB, LBF, RBF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Down))
            {
                FillSquare(normalVec,RTF, LTF, RBB, LBB, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Right))
            {
                FillSquare(normalVec,LBB, LTB, RBF, RTF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Right))
            {
                FillSquare(normalVec,RBB, RTB, LBF, LTF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Back) && normal.HasFlag(Normal.Left))
            {
                FillSquare(normalVec,RTB, RBB, LTF, LBF, vertices, triangles, uv, normals);
            }
            if (normal.HasFlag(Normal.Forward) && normal.HasFlag(Normal.Left))
            {
                FillSquare(normalVec,RBF, RTF, LBB, LTB, vertices, triangles, uv, normals);
            }
        }
        

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();
        mesh.bounds = new Bounds(
            Vector3.zero,
            Vector3.zero * size
        );
        return mesh;
    }

    private static void FillTriangle(
        Vector3 v1,
        Vector3 v2,
        Vector3 v3,
        Vector3 normal,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uv,
        List<Vector3> normals
    )
    {
        triangles.Add(vertices.Count + 0);
        triangles.Add(vertices.Count + 1);
        triangles.Add(vertices.Count + 2);
        
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        
        uv.Add(Vector2.zero);
        uv.Add(Vector2.zero);
        uv.Add(Vector2.zero);
        
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
    }

    private static void FillSide(
        Normal normal,
        Normal left,
        Normal right,
        Normal up,
        Normal down,
        Vector3 LT,
        Vector3 RT,
        Vector3 LB,
        Vector3 RB,
        Vector3 normalVec,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uv,
        List<Vector3> normals
    )
    {
        if (!IsXYCorner(normal, left, right, up, down))
        {
            FillSquare(normalVec, LT, RT, LB, RB, vertices, triangles, uv, normals);
            return;
        }
        if (normal.HasFlag(left) && normal.HasFlag(up))
        {
            FillTriangle(
                RB,
                LB,
                RT,
                normalVec,
                vertices,
                triangles,
                uv,
                normals
            );
            return;
        }
        if (normal.HasFlag(right) && normal.HasFlag(up))
        {
            FillTriangle(
                LT,
                RB,
                LB,
                normalVec,
                vertices,
                triangles,
                uv,
                normals
            );
            return;
        }
        if (normal.HasFlag(left) && normal.HasFlag(down))
        {
            FillTriangle(
                LT,
                RT,
                RB,
                normalVec,
                vertices,
                triangles,
                uv,
                normals
            );
            return;
        }
        if (normal.HasFlag(right) && normal.HasFlag(down))
        {
            FillTriangle(
                LT,
                RT,
                LB,
                normalVec,
                vertices,
                triangles,
                uv,
                normals
            );
            return;
        }
    }

    private static void FillSquare(
        Vector3 normal,
        Vector3 LT,
        Vector3 RT,
        Vector3 LB,
        Vector3 RB,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uv,
        List<Vector3> normals
    )
    {
        triangles.Add(vertices.Count + 2);
        triangles.Add(vertices.Count + 3);
        triangles.Add(vertices.Count + 1);
        triangles.Add(vertices.Count + 2);
        triangles.Add(vertices.Count + 1);
        triangles.Add(vertices.Count + 0);
        
        vertices.Add(LB);
        vertices.Add(RB);
        vertices.Add(LT);
        vertices.Add(RT);
        
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(1, 1));
        
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
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