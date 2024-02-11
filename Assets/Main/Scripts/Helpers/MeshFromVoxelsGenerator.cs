using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.VoxelEditor.State.Vox;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Main.Scripts.Helpers
{
public class MeshFromVoxelsGenerator
{
    private HashSet<Vector3Int> voxels;
    private int textureWidth;
    private int textureHeight;
    private TextureData textureData;
    private SpriteIndex spriteIndex;
    private Vector2 pivotPoint;
    private float pixelsPerUnit;

    public MeshFromVoxelsGenerator(
        HashSet<Vector3Int> voxels,
        int textureWidth,
        int textureHeight,
        TextureData textureData,
        SpriteIndex spriteIndex,
        Vector2 pivotPoint,
        float pixelsPerUnit
    )
    {
        this.voxels = voxels;
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
            var voxelsMap = FilterVoxelsByNormals();
            var polygonsMap = GetPolygonsDataByNormals(voxelsMap);

            var verticesDictionary = new Dictionary<Vector3Int, int>();
            var vertIndexesMap = new Dictionary<int, int>();
            var sortedVerticesList = new List<Vector3Int>();
            var boundsMin = Vector3.one * float.MaxValue;
            var boundsMax = Vector3.one * float.MinValue;

            for (var i = 0; i < polygonsMap.vertices.Count; i++)
            {
                var fromVert = polygonsMap.vertices[i];
                var vert = new Vector3Int(
                    (int)Math.Round(fromVert.x),
                    (int)Math.Round(fromVert.y),
                    (int)Math.Round(fromVert.z)
                );
                if (verticesDictionary.TryGetValue(vert, out var index))
                {
                    vertIndexesMap.Add(i, index);
                }
                else
                {
                    verticesDictionary.Add(vert, sortedVerticesList.Count);
                    vertIndexesMap.Add(i, sortedVerticesList.Count);
                    sortedVerticesList.Add(vert);
                }
            }

            foreach (var vert in sortedVerticesList)
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

            boundsMax += Vector3.one;

            var newTriangles = polygonsMap.triangles.ConvertAll(index => vertIndexesMap[index]);

            var mesh = new Mesh();
            
            var spriteWidth = textureWidth / textureData.columnsCount;
            var spriteHeight = textureHeight / textureData.rowsCount;

            mesh.vertices = sortedVerticesList.ConvertAll(ApplyScaleTransform).ToArray();
            mesh.triangles = newTriangles.ToArray();
            mesh.uv = sortedVerticesList.ConvertAll(vert => new Vector2(
                (vert.x + spriteWidth * spriteIndex.columnIndex) / (float)textureWidth,
                (vert.y + spriteHeight * (textureData.rowsCount - spriteIndex.rowIndex - 1)) / (float)textureHeight
            )).ToArray();
            mesh.normals = sortedVerticesList.ConvertAll(_ => Vector3.back).ToArray();
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

    private Vector3 ApplyScaleTransform(Vector3Int vertex)
    {
        return new Vector3(
            vertex.x - pivotPoint.x,
            vertex.y - pivotPoint.y,
            vertex.z
        ) / pixelsPerUnit;
    }

    private List<List<List<Vector2>>> GetTopStepsPointsPathsList(
        VertexData[][][] vertexMap,
        int sizeX,
        int sizeY,
        int sizeZ
    )
    {
        var stepsPointsPathsList = new List<List<List<Vector2>>>();

        for (var z = 0; z < sizeZ; z++)
        {
            var stepPointsPathsList = new List<List<Vector2>>();

            var checkedVertexes = new bool[sizeX][];
            for (var x = 0; x < sizeX; x++)
            {
                checkedVertexes[x] = new bool[sizeY];
            }


            for (var y = sizeY - 1; y >= 0; y--)
            {
                for (var x = 0; x < sizeX; x++)
                {
                    if (vertexMap[x][y][z] == null)
                    {
                        checkedVertexes[x][y] = true;
                        continue;
                    }

                    if (!checkedVertexes[x][y])
                    {
                        var pointsPathes = GetTopPointsPath(
                            vertexMap,
                            checkedVertexes,
                            x,
                            y,
                            z
                        );
                        if (pointsPathes != null)
                        {
                            foreach (var pointsPath in Split(pointsPathes))
                            {
                                stepPointsPathsList.Add(pointsPath);
                            }
                        }
                    }
                }
            }

            stepsPointsPathsList.Add(stepPointsPathsList);
        }

        return stepsPointsPathsList;
    }

    private List<List<Vector2>> Split(List<Vector2> pointsPath)
    {
        var lists = new List<List<Vector2>>();
        var pointsSet = new HashSet<Vector2>();
        var pointsStack = new Stack<Vector2>();
        
        for (var i = 0; i < pointsPath.Count; i++)
        {
            var point = pointsPath[i];
    
            if (pointsSet.Contains(point))
            {
                var newList = new List<Vector2>();
                newList.Add(point);
                while (pointsStack.TryPeek(out var peekedPoint) && peekedPoint != point)
                {
                    newList.Add(pointsStack.Pop());
                    pointsSet.Remove(peekedPoint);
                }

                newList.Reverse();
                lists.Add(newList);
            }
            else
            {
                pointsSet.Add(point);
                pointsStack.Push(point);
            }
        }
        
        var lastList = new List<Vector2>();
        foreach (var point in pointsSet)
        {
            lastList.Add(point);
        }

        lists.Add(lastList);

        return lists;
    }

    private List<Vector2>? GetTopPointsPath(
        VertexData[][][] vertexMap,
        bool[][] checkedVertexes,
        int startX,
        int startY,
        int z
    )
    {
        var pointsList = new List<Vector2>();
        var x = startX;
        var y = startY;
        var direction = Direction.Up;
        while (true)
        {
            if (vertexMap[x][y][z] != null)
            {
                var fromDirection = vertexMap[x][y][z].fromDirection;
                var toDirection = vertexMap[x][y][z].toDirection;

                var isFlat = fromDirection switch
                {
                    Direction.Up => toDirection == Direction.Down,
                    Direction.Right => toDirection == Direction.Left,
                    Direction.Down => toDirection == Direction.Up,
                    Direction.Left => toDirection == Direction.Right
                };

                var isCorrectDir = direction switch
                {
                    Direction.Up => fromDirection == Direction.Down,
                    Direction.Right => fromDirection == Direction.Left,
                    Direction.Down => fromDirection == Direction.Up,
                    Direction.Left => fromDirection == Direction.Right,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (!isFlat)
                {
                    pointsList.Add(new Vector2(x, y));
                    if (isCorrectDir)
                    {
                        direction = vertexMap[x][y][z].toDirection;
                    }
                    else
                    {
                        if (fromDirection != direction)
                        {
                            return null;
                        }

                        direction = InverseDirection(toDirection);
                    }
                }

                if (isFlat || isCorrectDir)
                {
                    checkedVertexes[x][y] = true;
                }

                if (isFlat && pointsList.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                checkedVertexes[x][y] = true;
            }

            switch (direction)
            {
                case Direction.Up:
                    y++;
                    break;
                case Direction.Right:
                    x++;
                    break;
                case Direction.Down:
                    y--;
                    break;
                case Direction.Left:
                    x--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (x == startX && y == startY)
            {
                break;
            }
        }

        return pointsList;
    }

    private Direction InverseDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Down => Direction.Up,
            Direction.Right => Direction.Left,
            Direction.Left => Direction.Right,
            Direction.Up => Direction.Down,
        };
    }

    private Dictionary<Normal, HashSet<Vector3Int>> FilterVoxelsByNormals()
    {
        var voxelsMap = new Dictionary<Normal, HashSet<Vector3Int>>();
        foreach (Normal normal in Enum.GetValues(typeof(Normal)))
        {
            voxelsMap[normal] = new HashSet<Vector3Int>();
        }

        foreach (var voxel in voxels)
        {
            var x = voxel.x;
            var y = voxel.y;
            var z = voxel.z;

            if (!voxels.Contains(voxel + Vector3Int.up))
            {
                voxelsMap[Normal.Up].Add(new Vector3Int(x, z, -(y + 1)));
            }

            if (!voxels.Contains(voxel + Vector3Int.down))
            {
                voxelsMap[Normal.Down].Add(new Vector3Int(z, x, y));
            }

            if (!voxels.Contains(voxel + Vector3Int.forward))
            {
                voxelsMap[Normal.Forward].Add(new Vector3Int(y, x, -(z + 1)));
            }

            if (!voxels.Contains(voxel + Vector3Int.back))
            {
                voxelsMap[Normal.Back].Add(voxel);
            }

            if (!voxels.Contains(voxel + Vector3Int.left))
            {
                voxelsMap[Normal.Left].Add(new Vector3Int(y, z, x));
            }

            if (!voxels.Contains(voxel + Vector3Int.right))
            {
                voxelsMap[Normal.Right].Add(new Vector3Int(z, y, -(x + 1)));
            }
        }

        return voxelsMap;
    }

    private PolygonsData GetPolygonsDataByNormals(Dictionary<Normal, HashSet<Vector3Int>> voxelsMap)
    {
        var polygonsMap = new Dictionary<Normal, PolygonsData>();
        foreach (Normal normal in Enum.GetValues(typeof(Normal)))
        {
            var polygonsData = GetPolygonsData(voxelsMap[normal]);
            var vertices = polygonsData.vertices.ConvertAll(vertex =>
            {
                var x = vertex.x;
                var y = vertex.y;
                var z = vertex.z;
                return normal switch
                {
                    Normal.Up => new Vector3(x, -z, y),
                    Normal.Down => new Vector3(y, z, x),
                    Normal.Forward => new Vector3(y, x, -z),
                    Normal.Back => vertex,
                    Normal.Left => new Vector3(z, x, y),
                    Normal.Right => new Vector3(-z, y, x),
                    _ => throw new ArgumentOutOfRangeException()
                };
            });

            polygonsMap[normal] = polygonsData with
            {
                vertices = vertices,
            };
        }

        var verticesList = new List<Vector3>();
        var trianglesList = new List<int>();

        foreach (var (_, polygonsData) in polygonsMap)
        {
            var verticesStartIndex = verticesList.Count;
            for (var j = 0; j < polygonsData.vertices.Count; j++)
            {
                verticesList.Add(polygonsData.vertices[j]);
            }

            for (var j = 0; j < polygonsData.triangles.Count; j++)
            {
                trianglesList.Add(polygonsData.triangles[j] + verticesStartIndex);
            }
        }

        return new PolygonsData(
            verticesList,
            trianglesList
        );
    }

    private PolygonsData GetPolygonsData(HashSet<Vector3Int> voxels)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var minZ = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        var maxZ = int.MinValue;

        foreach (var voxel in voxels)
        {
            minX = Math.Min(minX, voxel.x);
            minY = Math.Min(minY, voxel.y);
            minZ = Math.Min(minZ, voxel.z);
            maxX = Math.Max(maxX, voxel.x);
            maxY = Math.Max(maxY, voxel.y);
            maxZ = Math.Max(maxZ, voxel.z);
        }

        var sizeX = maxX - minX + 1;
        var sizeY = maxY - minY + 1;
        var sizeZ = maxZ - minZ + 1;

        var offset = new Vector3Int(minX, minY, minZ);

        var shiftedVoxels = new HashSet<Vector3Int>();
        foreach (var voxel in voxels)
        {
            shiftedVoxels.Add(voxel - offset);
        }

        var vertexMap = GetVertexes(
            shiftedVoxels,
            sizeX,
            sizeY,
            sizeZ
        );

        var pathsList = GetTopStepsPointsPathsList(
            vertexMap,
            sizeX + 1,
            sizeY + 1,
            sizeZ
        );

        var topPolygons = new List<Polygon>();

        for (var z = 0; z < sizeZ; z++)
        {
            var polygon = new Polygon();

            var square = new List<Vector2>();
            square.Add(new Vector2(
                -1,
                -1
            ));
            square.Add(new Vector2(
                -1,
                sizeY + 1
            ));
            square.Add(new Vector2(
                sizeX + 1,
                sizeY + 1
            ));
            square.Add(new Vector2(
                sizeX + 1,
                -1
            ));
            
            polygon.Add(square);

            foreach (var pointsPath in pathsList[z])
            {
                if (pointsPath.Count == 0) continue;
                polygon.Add(pointsPath);

                // var stA = pointsPath[0];
                // var b = pointsPath[1];
                // var stC = pointsPath[2];
                //
                // var deltaA = (stA - b).normalized;
                // var a = b + deltaA;
                //
                // var deltaC = (stC - b).normalized;
                // var c = b + deltaC;
                //
                // var pos = b + (deltaA + deltaC) / 2;
                //
                // var voxPos = Vector3Int.RoundToInt(new Vector3(
                //     Math.Min(Math.Min(a.x, b.x), c.x),
                //     Math.Min(Math.Min(a.y, b.y), c.y),
                //     z
                // ));
                //
                // if (!shiftedVoxels.Contains(voxPos))
                // {
                //     polygon.Holes.Add(new Point(pos.x, pos.y));
                // }
                // if (!shiftedVoxels.Contains(voxPos + Vector3Int.up))
                // {
                //     polygon.Holes.Add(new Point(pos.x, pos.y + 1));
                // }
                // if (!shiftedVoxels.Contains(voxPos + Vector3Int.down))
                // {
                //     polygon.Holes.Add(new Point(pos.x, pos.y - 1));
                // }
                // if (!shiftedVoxels.Contains(voxPos + Vector3Int.left))
                // {
                //     polygon.Holes.Add(new Point(pos.x - 1, pos.y));
                // }
                // if (!shiftedVoxels.Contains(voxPos + Vector3Int.right))
                // {
                //     polygon.Holes.Add(new Point(pos.x + 1, pos.y));
                // }
            }

            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    if (!shiftedVoxels.Contains(new Vector3Int(x, y, z)))
                    {
                        polygon.Holes.Add(new Point(x + 0.5f, y + 0.5f));
                    }
                }
            }

            topPolygons.Add(polygon);
        }

        var verticesList = new List<Vector3>();
        var trianglesList = new List<int>();

        for (var z = 0; z < topPolygons.Count; z++)
        {
            if (topPolygons[z].Count == 0)
            {
                continue;
            }
            
            var triangleNetMesh = (TriangleNetMesh)topPolygons[z].Triangulate();
            TriangleToMesh(
                triangleNetMesh,
                null,
                out var vertices,
                out var triangles,
                out var bounds
            );
            var verticesStartIndex = verticesList.Count;
            for (var j = 0; j < vertices.Length; j++)
            {
                var vert = vertices[j];
                verticesList.Add(new Vector3(vert.x, vert.z, z) + offset);
            }

            for (var j = 0; j < triangles.Length; j++)
            {
                trianglesList.Add(triangles[j] + verticesStartIndex);
            }
        }

        return new PolygonsData(
            verticesList,
            trianglesList
        );
    }

    private bool IsClockwise(List<Vector2> points)
    {
        var angle = 0f;
        var j = 1;
        for (var i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = points[j];
            angle += a.x * b.y - b.x * a.y;
            j = (j + 1) % points.Count;
        }

        return angle > 0;
    }

    private VertexData[][][] GetVertexes(
        HashSet<Vector3Int> voxels,
        int sizeX,
        int sizeY,
        int sizeZ
    )
    {
        var vertexMap = new VertexData[sizeX + 1][][];
        for (var x = 0; x <= sizeX; x++)
        {
            vertexMap[x] = new VertexData[sizeY + 1][];
            for (var y = 0; y <= sizeY; y++)
            {
                vertexMap[x][y] = new VertexData[sizeZ];
            }
        }

        for (var z = 0; z < sizeZ; z++)
        {
            for (var x = 0; x <= sizeX; x++)
            {
                for (var y = 0; y <= sizeY; y++)
                {
                    var leftBottom = x > 0 && y > 0 && voxels.Contains(new Vector3Int(x - 1, y - 1, z));
                    var rightBottom = x < sizeX && y > 0 && voxels.Contains(new Vector3Int(x, y - 1, z));
                    var rightTop = x < sizeX && y < sizeY && voxels.Contains(new Vector3Int(x, y, z));
                    var leftTop = x > 0 && y < sizeY && voxels.Contains(new Vector3Int(x - 1, y, z));

                    if (!leftTop
                        && !rightTop
                        && !leftBottom
                        && !rightBottom)
                    {
                        continue;
                    }

                    //outer corners
                    if (leftTop && !rightTop && !leftBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Up,
                            toDirection: Direction.Left
                        );
                    }

                    if (rightTop && !leftTop && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Right,
                            toDirection: Direction.Up
                        );
                    }

                    if (leftBottom && !leftTop && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Left,
                            toDirection: Direction.Down
                        );
                    }

                    if (rightBottom && !rightTop && !leftBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Down,
                            toDirection: Direction.Right
                        );
                    }

                    //inner corners
                    if (leftTop && rightTop && leftBottom && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Right,
                            toDirection: Direction.Down
                        );
                    }

                    if (rightTop && leftTop && rightBottom && !leftBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Down,
                            toDirection: Direction.Left
                        );
                    }

                    if (leftBottom && leftTop && rightBottom && !rightTop)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Up,
                            toDirection: Direction.Right
                        );
                    }

                    if (rightBottom && rightTop && leftBottom && !leftTop)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Left,
                            toDirection: Direction.Up
                        );
                    }
                }
            }
        }

        return vertexMap;
    }

    private void TriangleToMesh(
        TriangleNetMesh triangleNetMesh,
        QualityOptions? options,
        out Vector3[] vertices,
        out int[] triangles,
        out Bounds bounds
    )
    {
        if (options != null)
        {
            triangleNetMesh.Refine(options);
        }

        var triangleNetVerts = triangleNetMesh.Vertices.ToList();

        var trianglesList = triangleNetMesh.Triangles;

        vertices = new Vector3[triangleNetVerts.Count];
        triangles = new int[trianglesList.Count * 3];

        for (int i = 0; i < vertices.Length; i++)
        {
            var point = (Vector3)triangleNetVerts[i];
            vertices[i] = new Vector3(point.x, 0, point.y);
        }

        int k = 0;

        foreach (var triangle in trianglesList)
        {
            for (int i = 2; i >= 0; i--)
            {
                triangles[k] = triangleNetVerts.IndexOf(triangle.GetVertex(i));
                k++;
            }
        }

        var rectangle = triangleNetMesh.Bounds;

        bounds = new Bounds(
            new Vector3((rectangle.Left + rectangle.Right) / 2f, 0, (rectangle.Bottom + rectangle.Top) / 2f),
            new Vector3(rectangle.Right - rectangle.Left, 0, rectangle.Top - rectangle.Bottom)
        );
    }

    private class VertexData
    {
        public readonly Direction fromDirection;
        public readonly Direction toDirection;

        public VertexData(
            Direction fromDirection,
            Direction toDirection
        )
        {
            this.fromDirection = fromDirection;
            this.toDirection = toDirection;
        }
    }

    private record PolygonsData(
        List<Vector3> vertices,
        List<int> triangles
    );

    private enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    private enum Normal
    {
        Up,
        Down,
        Forward,
        Back,
        Left,
        Right
    }
}
}