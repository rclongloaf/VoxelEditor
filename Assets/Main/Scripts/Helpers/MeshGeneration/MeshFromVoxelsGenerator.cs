using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.VoxelEditor.State.Vox;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Main.Scripts.Helpers.MeshGeneration
{
public class MeshFromVoxelsGenerator : MeshGenerator
{
    private Dictionary<Vector3Int, VoxelData> voxels;
    private int textureWidth;
    private int textureHeight;
    private TextureData textureData;
    private SpriteIndex spriteIndex;
    private Vector2 pivotPoint;
    private float pixelsPerUnit;
    private bool optimizeNormals;

    public MeshFromVoxelsGenerator(
        Dictionary<Vector3Int, VoxelData> voxels,
        int textureWidth,
        int textureHeight,
        TextureData textureData,
        SpriteIndex spriteIndex,
        Vector2 pivotPoint,
        float pixelsPerUnit,
        bool optimizeNormals
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
        this.optimizeNormals = optimizeNormals;
    }

    public Mesh? GenerateMesh()
    {
        try
        {
            var smoothNormalMap = VoxelsSmoothHelper.GetSmoothNormalMap(voxels);

            var voxelsMap = FilterVoxelsByNormals(smoothNormalMap);
            var polygonsMap = GetPolygonsDataByNormals(voxelsMap, smoothNormalMap);

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
                if (optimizeNormals)
                {
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
                else
                {
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

            var newTriangles = optimizeNormals ? polygonsMap.triangles.ConvertAll(index => vertIndexesMap[index]) : polygonsMap.triangles;

            var mesh = new Mesh();

            var spriteWidth = textureWidth / textureData.columnsCount;
            var spriteHeight = textureHeight / textureData.rowsCount;

            mesh.vertices = sortedVerticesList.ConvertAll(ApplyScaleTransform).ToArray();
            mesh.triangles = newTriangles.ToArray();
            mesh.uv = sortedVerticesList.ConvertAll(vert => new Vector2(
                (vert.x + spriteWidth * spriteIndex.columnIndex) / (float)textureWidth,
                (vert.y + vert.z + spriteHeight * (textureData.rowsCount - spriteIndex.rowIndex - 1)) / (float)textureHeight
            )).ToArray();
            mesh.normals = optimizeNormals ? sortedVerticesList.ConvertAll(_ => Vector3.back).ToArray() : polygonsMap.normals.ToArray();
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
                var shouldSmooth = vertexMap[x][y][z].shouldSmooth;

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
                    var curToDirection = direction;

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

                    if (shouldSmooth)
                    {
                        var fromX = x;
                        var fromY = y;
                        var toX = x;
                        var toY = y;

                        switch (curToDirection)
                        {
                            case Direction.Up:
                                fromY--;
                                break;
                            case Direction.Right:
                                fromX--;
                                break;
                            case Direction.Down:
                                fromY++;
                                break;
                            case Direction.Left:
                                fromX++;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (direction)
                        {
                            case Direction.Up:
                                toY++;
                                break;
                            case Direction.Right:
                                toX++;
                                break;
                            case Direction.Down:
                                toY--;
                                break;
                            case Direction.Left:
                                toX--;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (pointsList.Count == 0 || pointsList[^1] != new Vector2(fromX, fromY))
                        {
                            AddPointWithReplacingLastCorner(pointsList, new Vector2(fromX, fromY));
                        }

                        if (pointsList.Count == 0 || pointsList[0] != new Vector2(toX, toY))
                        {
                            AddPointWithReplacingLastCorner(pointsList, new Vector2(toX, toY));
                        }
                    }
                    else
                    {
                        if (pointsList.Count == 0 || (pointsList[^1] != new Vector2(x, y) && pointsList[0] != new Vector2(x, y)))
                        {
                            AddPointWithReplacingLastCorner(pointsList, new Vector2(x, y));
                        }
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

    private void AddPointWithReplacingLastCorner(List<Vector2> pointsList, Vector2 point)
    {
        if (pointsList.Count > 1)
        {
            var lastPointsDelta = (pointsList[^1] - pointsList[^2]).normalized;
            var pointDelta = (point - pointsList[^1]).normalized;

            if (lastPointsDelta == pointDelta)
            {
                pointsList.RemoveAt(pointsList.Count - 1);
            }
        }

        pointsList.Add(point);
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

    private Dictionary<Normal, VoxelsInfo> FilterVoxelsByNormals(Dictionary<Vector3Int, Normal> smoothNormalMap)
    {
        var voxelsMap = new Dictionary<Normal, VoxelsInfo>();
        foreach (Normal normal in Enum.GetValues(typeof(Normal)))
        {
            voxelsMap[normal] = new VoxelsInfo(new HashSet<Vector3Int>(), new Dictionary<Vector3Int, Normal>());
            foreach (Normal secondNormal in Enum.GetValues(typeof(Normal)))
            {
                var sideNormal = normal | secondNormal;
                if (!isSmoothed(sideNormal) || voxelsMap.ContainsKey(sideNormal)) continue;
                voxelsMap[sideNormal] = new VoxelsInfo(new HashSet<Vector3Int>(), new Dictionary<Vector3Int, Normal>());
            }
        }

        foreach (var (voxel, _) in voxels)
        {
            var x = voxel.x;
            var y = voxel.y;
            var z = voxel.z;
            var voxelNormal = smoothNormalMap[voxel];
            
            {
                var convertedNormal = (voxelNormal.HasFlag(Normal.Right) ? Normal.Right : 0)
                                      | (voxelNormal.HasFlag(Normal.Left) ? Normal.Left : 0)
                                      | (voxelNormal.HasFlag(Normal.Forward) ? Normal.Up : 0)
                                      | (voxelNormal.HasFlag(Normal.Back) ? Normal.Down : 0)
                                      | (voxelNormal.HasFlag(Normal.Down) ? Normal.Forward : 0)
                                      | (voxelNormal.HasFlag(Normal.Up) ? Normal.Back : 0);
                var vox = new Vector3Int(x, z, -(y + 1));
                voxelsMap[Normal.Up].voxels.Add(vox);
                voxelsMap[Normal.Up].smoothNormalsMap[vox] = convertedNormal;
            }

            {
                var convertedNormal = (voxelNormal.HasFlag(Normal.Forward) ? Normal.Right : 0)
                                      | (voxelNormal.HasFlag(Normal.Back) ? Normal.Left : 0)
                                      | (voxelNormal.HasFlag(Normal.Right) ? Normal.Up : 0)
                                      | (voxelNormal.HasFlag(Normal.Left) ? Normal.Down : 0)
                                      | (voxelNormal.HasFlag(Normal.Up) ? Normal.Forward : 0)
                                      | (voxelNormal.HasFlag(Normal.Down) ? Normal.Back : 0);
                var vox = new Vector3Int(z, x, y);
                voxelsMap[Normal.Down].voxels.Add(vox);
                voxelsMap[Normal.Down].smoothNormalsMap[vox] = convertedNormal;
            }
            
            {
                var convertedNormal = (voxelNormal.HasFlag(Normal.Up) ? Normal.Right : 0)
                                      | (voxelNormal.HasFlag(Normal.Down) ? Normal.Left : 0)
                                      | (voxelNormal.HasFlag(Normal.Right) ? Normal.Up : 0)
                                      | (voxelNormal.HasFlag(Normal.Left) ? Normal.Down : 0)
                                      | (voxelNormal.HasFlag(Normal.Back) ? Normal.Forward : 0)
                                      | (voxelNormal.HasFlag(Normal.Forward) ? Normal.Back : 0);
                var vox = new Vector3Int(y, x, -(z + 1));
                voxelsMap[Normal.Forward].voxels.Add(vox);
                voxelsMap[Normal.Forward].smoothNormalsMap[vox] = convertedNormal;
            }
            
            {
                voxelsMap[Normal.Back].voxels.Add(voxel);
                voxelsMap[Normal.Back].smoothNormalsMap[voxel] = voxelNormal;
            }
            
            {
                var convertedNormal = (voxelNormal.HasFlag(Normal.Up) ? Normal.Right : 0)
                                      | (voxelNormal.HasFlag(Normal.Down) ? Normal.Left : 0)
                                      | (voxelNormal.HasFlag(Normal.Forward) ? Normal.Up : 0)
                                      | (voxelNormal.HasFlag(Normal.Back) ? Normal.Down : 0)
                                      | (voxelNormal.HasFlag(Normal.Right) ? Normal.Forward : 0)
                                      | (voxelNormal.HasFlag(Normal.Left) ? Normal.Back : 0);
                var vox = new Vector3Int(y, z, x);
                voxelsMap[Normal.Left].voxels.Add(vox);
                voxelsMap[Normal.Left].smoothNormalsMap[vox] = convertedNormal;
            }
            
            {
                var convertedNormal = (voxelNormal.HasFlag(Normal.Forward) ? Normal.Right : 0)
                                      | (voxelNormal.HasFlag(Normal.Back) ? Normal.Left : 0)
                                      | (voxelNormal.HasFlag(Normal.Up) ? Normal.Up : 0)
                                      | (voxelNormal.HasFlag(Normal.Down) ? Normal.Down : 0)
                                      | (voxelNormal.HasFlag(Normal.Left) ? Normal.Forward : 0)
                                      | (voxelNormal.HasFlag(Normal.Right) ? Normal.Back : 0);
                var vox = new Vector3Int(z, y, -(x + 1));
                voxelsMap[Normal.Right].voxels.Add(vox);
                voxelsMap[Normal.Right].smoothNormalsMap[vox] = convertedNormal;
            }
            
            if (voxelNormal.IsCorner()) continue;

            if (voxelNormal.HasFlag(Normal.Back) && voxelNormal.HasFlag(Normal.Up))
            {
                var vox = new Vector3Int(x, y, z - y);
                voxelsMap[Normal.Back | Normal.Up].voxels.Add(vox);
                voxelsMap[Normal.Back | Normal.Up].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Forward) && voxelNormal.HasFlag(Normal.Up))
            {
                var vox = new Vector3Int(-x, y, z + y);
                voxelsMap[Normal.Forward | Normal.Up].voxels.Add(vox);
                voxelsMap[Normal.Forward | Normal.Up].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Back) && voxelNormal.HasFlag(Normal.Down))
            {
                var vox = new Vector3Int(x, y, z + y);
                voxelsMap[Normal.Back | Normal.Down].voxels.Add(vox);
                voxelsMap[Normal.Back | Normal.Down].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Forward) && voxelNormal.HasFlag(Normal.Down))
            {
                var vox = new Vector3Int(-x, y, z - y);
                voxelsMap[Normal.Forward | Normal.Down].voxels.Add(vox);
                voxelsMap[Normal.Forward | Normal.Down].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Up))
            {
                var vox = new Vector3Int(z, y, x + y);
                voxelsMap[Normal.Right | Normal.Up].voxels.Add(vox);
                voxelsMap[Normal.Right | Normal.Up].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Up))
            {
                var vox = new Vector3Int(-z, y, x - y);
                voxelsMap[Normal.Left | Normal.Up].voxels.Add(vox);
                voxelsMap[Normal.Left | Normal.Up].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Down))
            {
                var vox = new Vector3Int(z, y, x - y);
                voxelsMap[Normal.Right | Normal.Down].voxels.Add(vox);
                voxelsMap[Normal.Right | Normal.Down].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Down))
            {
                var vox = new Vector3Int(-z, y, y + x);
                voxelsMap[Normal.Left | Normal.Down].voxels.Add(vox);
                voxelsMap[Normal.Left | Normal.Down].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Back))
            {
                var vox = new Vector3Int(-y, z, x - z);
                voxelsMap[Normal.Right | Normal.Back].voxels.Add(vox);
                voxelsMap[Normal.Right | Normal.Back].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Back))
            {
                var vox = new Vector3Int(y, z, x + z);
                voxelsMap[Normal.Left | Normal.Back].voxels.Add(vox);
                voxelsMap[Normal.Left | Normal.Back].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Forward))
            {
                var vox = new Vector3Int(-y, z, x + z);
                voxelsMap[Normal.Right | Normal.Forward].voxels.Add(vox);
                voxelsMap[Normal.Right | Normal.Forward].smoothNormalsMap[vox] = Normal.Forward;
            }

            if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Forward))
            {
                var vox = new Vector3Int(y, z, x - z);
                voxelsMap[Normal.Left | Normal.Forward].voxels.Add(vox);
                voxelsMap[Normal.Left | Normal.Forward].smoothNormalsMap[vox] = Normal.Forward;
            }
        }

        return voxelsMap;
    }

    private PolygonsData GetPolygonsDataByNormals(
        Dictionary<Normal, VoxelsInfo> voxelsInfoMap,
        Dictionary<Vector3Int, Normal> smoothNormalMap
    )
    {
        var polygonsMap = new Dictionary<Normal, PolygonsData>();
        var checkedNormals = new HashSet<Normal>();
        foreach (Normal normal in Enum.GetValues(typeof(Normal)))
        {
            var polygonsData = GetPolygonsData(voxelsInfoMap[normal]);
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
            var normals = new List<Vector3>();
            var normalVector = GetNormalVector(normal, true);
            for (var i = 0; i < vertices.Count; i++)
            {
                normals.Add(normalVector);
            }

            polygonsMap[normal] = polygonsData with
            {
                vertices = vertices,
                normals = normals
            };

            foreach (Normal secondNormal in Enum.GetValues(typeof(Normal)))
            {
                var voxelNormal = normal | secondNormal;
                if (!isSmoothed(voxelNormal) || checkedNormals.Contains(voxelNormal) || !voxelsInfoMap.ContainsKey(voxelNormal))
                {
                    checkedNormals.Add(voxelNormal);
                    continue;
                }

                checkedNormals.Add(voxelNormal);

                var sidePolygonsData = GetPolygonsData(voxelsInfoMap[voxelNormal]);
                var sideVertices = sidePolygonsData.vertices.ConvertAll(vertex =>
                {
                    var x = vertex.x;
                    var y = vertex.y;
                    var z = vertex.z;

                    if (voxelNormal.HasFlag(Normal.Back) && voxelNormal.HasFlag(Normal.Up))
                    {
                        return new Vector3(x, y, z + y);
                    }

                    if (voxelNormal.HasFlag(Normal.Forward) && voxelNormal.HasFlag(Normal.Up))
                    {
                        return new Vector3(-x + 1, y, z - y + 1);
                    }

                    if (voxelNormal.HasFlag(Normal.Back) && voxelNormal.HasFlag(Normal.Down))
                    {
                        return new Vector3(x, y, z - y + 1);
                    }

                    if (voxelNormal.HasFlag(Normal.Forward) && voxelNormal.HasFlag(Normal.Down))
                    {
                        return new Vector3(-x + 1, y, z + y);
                    }

                    if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Up))
                    {
                        return new Vector3(z - y + 1, y, x);
                    }

                    if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Up))
                    {
                        return new Vector3(z + y, y, -x + 1);
                    }

                    if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Down))
                    {
                        return new Vector3(z + y, y, x);
                    }

                    if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Down))
                    {
                        return new Vector3(z - y + 1, y, -x + 1);
                    }

                    if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Back))
                    {
                        return new Vector3(z + y, -x + 1, y);
                    }

                    if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Back))
                    {
                        return new Vector3(z - y + 1, x, y);
                    }

                    if (voxelNormal.HasFlag(Normal.Right) && voxelNormal.HasFlag(Normal.Forward))
                    {
                        return new Vector3(z - y + 1, -x + 1, y);
                    }

                    if (voxelNormal.HasFlag(Normal.Left) && voxelNormal.HasFlag(Normal.Forward))
                    {
                        return new Vector3(z + y, x, y);
                    }

                    throw new Exception($"Normal {voxelNormal} must be contains");
                });

                polygonsMap[voxelNormal] = sidePolygonsData with
                {
                    vertices = sideVertices,
                };
            }
        }

        var voxelsByCornerNormals = new Dictionary<Normal, HashSet<Vector3Int>>();

        foreach (var (voxel, normal) in smoothNormalMap)
        {
            if (!normal.IsCorner()) continue;

            if (!voxelsByCornerNormals.ContainsKey(normal))
            {
                voxelsByCornerNormals[normal] = new HashSet<Vector3Int>();
            }

            voxelsByCornerNormals[normal].Add(voxel);
        }

        foreach (var (normal, voxels) in voxelsByCornerNormals)
        {
            polygonsMap[normal] = GetCornerPolygonsData(voxels, normal);
        }

        var verticesList = new List<Vector3>();
        var normalsList = new List<Vector3>();
        var trianglesList = new List<int>();
        

        foreach (var (normal, polygonsData) in polygonsMap)
        {
            var normalVector = GetNormalVector(normal, true);
            
            var verticesStartIndex = verticesList.Count;
            for (var j = 0; j < polygonsData.vertices.Count; j++)
            {
                verticesList.Add(polygonsData.vertices[j]);
                normalsList.Add(normalVector);
            }

            for (var j = 0; j < polygonsData.triangles.Count; j++)
            {
                trianglesList.Add(polygonsData.triangles[j] + verticesStartIndex);
            }
        }

        return new PolygonsData(
            verticesList,
            normalsList,
            trianglesList
        );
    }

    private Vector3 GetNormalVector(Normal normal, bool replaceUpToBack)
    {
        var normalX = normal.HasFlag(Normal.Right) ? 1 : (normal.HasFlag(Normal.Left) ? -1 : 0);
        var normalY = normal.HasFlag(Normal.Up) ? 1 : (normal.HasFlag(Normal.Down) ? -1 : 0);
        var normalZ = normal.HasFlag(Normal.Forward) ? 1 : (normal.HasFlag(Normal.Back) ? -1 : 0);

        if (replaceUpToBack && normalY > 0 && normalZ <= 0)
        {
            normalY = 0;
            normalZ = -1;
        }

        return new Vector3(normalX, normalY, normalZ).normalized;
    }

    private bool isSmoothed(Normal normal)
    {
        var normalX = normal.HasFlag(Normal.Right) || normal.HasFlag(Normal.Left);
        var normalY = normal.HasFlag(Normal.Up) || normal.HasFlag(Normal.Down);
        var normalZ = normal.HasFlag(Normal.Forward) || normal.HasFlag(Normal.Back);

        return (normalX && normalY && normalZ)
               || (normalX && normalY)
               || (normalX && normalZ)
               || (normalY && normalZ);
    }

    private PolygonsData GetPolygonsData(VoxelsInfo voxelsInfo)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var minZ = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        var maxZ = int.MinValue;

        foreach (var voxel in voxelsInfo.voxels)
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
        var shiftedSmoothNormalsMap = new Dictionary<Vector3Int, Normal>();
        foreach (var voxel in voxelsInfo.voxels)
        {
            var vox = voxel - offset;
            shiftedVoxels.Add(vox);
            shiftedSmoothNormalsMap[vox] = voxelsInfo.smoothNormalsMap[voxel];
        }

        var vertexMap = GetVertexes(
            shiftedVoxels,
            shiftedSmoothNormalsMap,
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
            }

            for (var x = -1; x <= sizeX; x++)
            {
                for (var y = -1; y <= sizeY; y++)
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

        var usedVertexIndexes = new HashSet<int>();
        foreach (var index in trianglesList)
        {
            usedVertexIndexes.Add(index);
        }

        var vertexIndexMap = new Dictionary<int, int>();
        var filteredVerticesList = new List<Vector3>();
        for (var i = 0; i < verticesList.Count; i++)
        {
            if (usedVertexIndexes.Contains(i))
            {
                vertexIndexMap[i] = vertexIndexMap.Count;
                filteredVerticesList.Add(verticesList[i]);
            }
        }

        trianglesList = trianglesList.ConvertAll(index => vertexIndexMap[index]);

        return new PolygonsData(
            filteredVerticesList,
            new List<Vector3>(),
            trianglesList
        );
    }

    private PolygonsData GetCornerPolygonsData(HashSet<Vector3Int> voxels, Normal normal)
    {
        var checkedVoxels = new HashSet<Vector3Int>();

        var verticesList = new List<Vector3>();
        var normalsList = new List<Vector3>();
        var trianglesList = new List<int>();

        var step = GetStepByNormal(normal);

        if (step == Vector3Int.zero)
        {
            throw new Exception("Incorrect corner normal");
        }

        foreach (var voxel in voxels)
        {
            if (!checkedVoxels.Add(voxel)) continue;

            var vertexStartIndex = verticesList.Count;

            var normalX = normal.HasFlag(Normal.Right) ? 1 : (normal.HasFlag(Normal.Left) ? -1 : 0);
            var normalY = normal.HasFlag(Normal.Up) ? 1 : (normal.HasFlag(Normal.Down) ? -1 : 0);
            var normalZ = normal.HasFlag(Normal.Forward) ? 1 : (normal.HasFlag(Normal.Back) ? -1 : 0);

            var normalVector = GetNormalVector(normal, true);

            if (step == Vector3Int.one)
            {
                var LBB = new Vector3Int(voxel.x, voxel.y, voxel.z);
                var RBB = new Vector3Int(voxel.x + 1, voxel.y, voxel.z);
                var LTB = new Vector3Int(voxel.x, voxel.y + 1, voxel.z);
                var RTB = new Vector3Int(voxel.x + 1, voxel.y + 1, voxel.z);
                var LBF = new Vector3Int(voxel.x, voxel.y, voxel.z + 1);
                var RBF = new Vector3Int(voxel.x + 1, voxel.y, voxel.z + 1);
                var LTF = new Vector3Int(voxel.x, voxel.y + 1, voxel.z + 1);
                var RTF = new Vector3Int(voxel.x + 1, voxel.y + 1, voxel.z + 1);

                switch (normalX, normalY, normalZ)
                {
                    case (1, 1, 1):
                        verticesList.Add(LTB);
                        verticesList.Add(LBF);
                        verticesList.Add(RBB);
                        break;
                    case (-1, 1, 1):
                        verticesList.Add(RTB);
                        verticesList.Add(LBB);
                        verticesList.Add(RBF);
                        break;
                    case (1, -1, 1):
                        verticesList.Add(LBB);
                        verticesList.Add(RTB);
                        verticesList.Add(LTF);
                        break;
                    case (-1, -1, 1):
                        verticesList.Add(LTB);
                        verticesList.Add(RBB);
                        verticesList.Add(RTF);
                        break;
                    case (1, 1, -1):
                        verticesList.Add(LBB);
                        verticesList.Add(LTF);
                        verticesList.Add(RBF);
                        break;
                    case (-1, 1, -1):
                        verticesList.Add(LBF);
                        verticesList.Add(RTF);
                        verticesList.Add(RBB);
                        break;
                    case (1, -1, -1):
                        verticesList.Add(LBF);
                        verticesList.Add(LTB);
                        verticesList.Add(RTF);
                        break;
                    case (-1, -1, -1):
                        verticesList.Add(RBF);
                        verticesList.Add(LTF);
                        verticesList.Add(RTB);
                        break;
                }

                normalsList.Add(normalVector);

                trianglesList.Add(vertexStartIndex + 0);
                trianglesList.Add(vertexStartIndex + 1);
                trianglesList.Add(vertexStartIndex + 2);
            }
        }

        return new PolygonsData(
            verticesList,
            normalsList,
            trianglesList
        );
    }

    private void ShiftSideVoxelsByNormal(int x, int y, ref Vector2Int from, ref Vector2Int to)
    {
        switch (x, y)
        {
            case (1, 1):
                from = new Vector2Int(from.x, from.y + 1);
                to = new Vector2Int(to.x + 1, to.y);
                break;
            case (-1, 1):
                from = new Vector2Int(from.x, from.y);
                to = new Vector2Int(to.x + 1, to.y + 1);
                break;
            case (1, -1):
                from = new Vector2Int(from.x + 1, from.y + 1);
                to = new Vector2Int(to.x, to.y);
                break;
            case (-1, -1):
                from = new Vector2Int(from.x + 1, from.y);
                to = new Vector2Int(to.x, to.y + 1);
                break;
        }
    }

    private Vector3Int GetStepByNormal(Normal normal)
    {
        var normalX = normal.HasFlag(Normal.Right) ? 1 : (normal.HasFlag(Normal.Left) ? -1 : 0);
        var normalY = normal.HasFlag(Normal.Up) ? 1 : (normal.HasFlag(Normal.Down) ? -1 : 0);
        var normalZ = normal.HasFlag(Normal.Forward) ? 1 : (normal.HasFlag(Normal.Back) ? -1 : 0);

        if (normalX != 0 && normalY != 0 && normalZ != 0)
        {
            return Vector3Int.one;
        }

        if (normalX != 0 && normalY != 0)
        {
            return new Vector3Int(
                normalY,
                -normalX,
                0
            );
        }

        if (normalX != 0 && normalZ != 0)
        {
            return new Vector3Int(
                -normalZ,
                0,
                normalX
            );
        }

        if (normalY != 0 && normalZ != 0)
        {
            return new Vector3Int(
                0,
                -normalZ,
                normalY
            );
        }

        return Vector3Int.zero;
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
        Dictionary<Vector3Int, Normal> smoothNormalsMap,
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
                    var leftBottomVoxel = new Vector3Int(x - 1, y - 1, z);
                    var rightBottomVoxel = new Vector3Int(x, y - 1, z);
                    var rightTopVoxel = new Vector3Int(x, y, z);
                    var leftTopVoxel = new Vector3Int(x - 1, y, z);

                    var containsLeftBottom = x > 0 && y > 0 && voxels.Contains(leftBottomVoxel);
                    var containsRightBottom = x < sizeX && y > 0 && voxels.Contains(rightBottomVoxel);
                    var containsRightTop = x < sizeX && y < sizeY && voxels.Contains(rightTopVoxel);
                    var containsLeftTop = x > 0 && y < sizeY && voxels.Contains(leftTopVoxel);


                    var leftBottomNormal = containsLeftBottom ? smoothNormalsMap[leftBottomVoxel] : 0;
                    var rightBottomNormal = containsRightBottom ? smoothNormalsMap[rightBottomVoxel] : 0;
                    var rightTopNormal = containsRightTop ? smoothNormalsMap[rightTopVoxel] : 0;
                    var leftTopNormal = containsLeftTop ? smoothNormalsMap[leftTopVoxel] : 0;

                    var leftBottom = containsLeftBottom && !leftBottomNormal.HasFlag(Normal.Back);
                    var rightBottom = containsRightBottom && !rightBottomNormal.HasFlag(Normal.Back);
                    var rightTop = containsRightTop && !rightTopNormal.HasFlag(Normal.Back);
                    var leftTop = containsLeftTop && !leftTopNormal.HasFlag(Normal.Back);

                    var leftBottomBackVoxel = new Vector3Int(x - 1, y - 1, z - 1);
                    var rightBottomBackVoxel = new Vector3Int(x, y - 1, z - 1);
                    var rightTopBackVoxel = new Vector3Int(x, y, z - 1);
                    var leftTopBackVoxel = new Vector3Int(x - 1, y, z - 1);

                    var containsLeftBottomBack = z > 0 && x > 0 && y > 0 && voxels.Contains(leftBottomBackVoxel);
                    var containsRightBottomBack = z > 0 && x < sizeX && y > 0 && voxels.Contains(rightBottomBackVoxel);
                    var containsRightTopBack = z > 0 && x < sizeX && y < sizeY && voxels.Contains(rightTopBackVoxel);
                    var containsLeftTopBack = z > 0 && x > 0 && y < sizeY && voxels.Contains(leftTopBackVoxel);


                    var leftBottomBackNormal = containsLeftBottomBack ? smoothNormalsMap[leftBottomBackVoxel] : 0;
                    var rightBottomBackNormal = containsRightBottomBack ? smoothNormalsMap[rightBottomBackVoxel] : 0;
                    var rightTopBackNormal = containsRightTopBack ? smoothNormalsMap[rightTopBackVoxel] : 0;
                    var leftTopBackNormal = containsLeftTopBack ? smoothNormalsMap[leftTopBackVoxel] : 0;

                    leftBottom = leftBottom
                                 && (!containsLeftBottomBack || (isSmoothed(leftBottomBackNormal) && (leftBottomBackNormal.HasFlag(Normal.Forward) || (IsXYCorner(leftBottomBackNormal) && !isSameXYNormal(leftBottomNormal, leftBottomBackNormal)))));
                    rightBottom = rightBottom
                                  && (!containsRightBottomBack || (isSmoothed(rightBottomBackNormal) && (rightBottomBackNormal.HasFlag(Normal.Forward) || (IsXYCorner(rightBottomBackNormal) && !isSameXYNormal(rightBottomNormal, rightBottomBackNormal)))));
                    rightTop = rightTop
                               && (!containsRightTopBack || (isSmoothed(rightTopBackNormal) && (rightTopBackNormal.HasFlag(Normal.Forward) || (IsXYCorner(rightTopBackNormal) && !isSameXYNormal(rightTopNormal, rightTopBackNormal)))));
                    leftTop = leftTop
                              && (!containsLeftTopBack || (isSmoothed(leftTopBackNormal) && (leftTopBackNormal.HasFlag(Normal.Forward) || (IsXYCorner(leftTopBackNormal) && !isSameXYNormal(leftTopNormal, leftTopBackNormal)))));

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
                            toDirection: Direction.Left,
                            shouldSmooth: ShouldSmooth(Normal.Right, Normal.Down, leftTopNormal, leftTopBackNormal)
                        );
                    }

                    if (rightTop && !leftTop && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Right,
                            toDirection: Direction.Up,
                            shouldSmooth: ShouldSmooth(Normal.Left, Normal.Down, rightTopNormal, rightTopBackNormal)
                        );
                    }

                    if (leftBottom && !leftTop && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Left,
                            toDirection: Direction.Down,
                            shouldSmooth: ShouldSmooth(Normal.Right, Normal.Up, leftBottomNormal, leftBottomBackNormal)
                        );
                    }

                    if (rightBottom && !rightTop && !leftBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Down,
                            toDirection: Direction.Right,
                            shouldSmooth: ShouldSmooth(Normal.Left, Normal.Up, rightBottomNormal, rightBottomBackNormal)
                        );
                    }

                    //inner corners
                    if (leftTop && rightTop && leftBottom && !rightBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Right,
                            toDirection: Direction.Down,
                            shouldSmooth: false
                        );
                    }

                    if (rightTop && leftTop && rightBottom && !leftBottom)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Down,
                            toDirection: Direction.Left,
                            shouldSmooth: false
                        );
                    }

                    if (leftBottom && leftTop && rightBottom && !rightTop)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Up,
                            toDirection: Direction.Right,
                            shouldSmooth: false
                        );
                    }

                    if (rightBottom && rightTop && leftBottom && !leftTop)
                    {
                        vertexMap[x][y][z] = new VertexData(
                            fromDirection: Direction.Left,
                            toDirection: Direction.Up,
                            shouldSmooth: false
                        );
                    }
                }
            }
        }

        return vertexMap;
    }

    private bool ShouldSmooth(Normal normalX, Normal normalY, Normal forwardNormal, Normal backNormal)
    {
        var invNormalX = normalX == Normal.Right ? Normal.Left : Normal.Right;
        var invNormalY = normalY == Normal.Up ? Normal.Down : Normal.Up;

        if ((forwardNormal.HasFlag(normalX) && forwardNormal.HasFlag(normalY))
            || (backNormal.HasFlag(invNormalX) && backNormal.HasFlag(invNormalY)))
        {
            return !IsXYCorner(forwardNormal)
                   || !IsXYCorner(backNormal)
                   || (backNormal.HasFlag(invNormalX) && backNormal.HasFlag(invNormalY))
                   || ((forwardNormal & (normalX | invNormalX)) == (backNormal & (normalX | invNormalX)))
                   == ((forwardNormal & (normalY | invNormalY)) == (backNormal & (normalY | invNormalY)));
        }

        return false;
    }

    private bool IsXYCorner(Normal normal)
    {
        if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Up))
        {
            return true;
        }

        if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Up))
        {
            return true;
        }

        if (normal.HasFlag(Normal.Left) && normal.HasFlag(Normal.Down))
        {
            return true;
        }

        if (normal.HasFlag(Normal.Right) && normal.HasFlag(Normal.Down))
        {
            return true;
        }

        return false;
    }

    private bool isSameXYNormal(Normal normalA, Normal normalB)
    {
        var normalsZ = Normal.Forward | Normal.Back;
        return (normalA & ~normalsZ) == (normalB & ~normalsZ);
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
        public readonly bool shouldSmooth;

        public VertexData(
            Direction fromDirection,
            Direction toDirection,
            bool shouldSmooth
        )
        {
            this.fromDirection = fromDirection;
            this.toDirection = toDirection;
            this.shouldSmooth = shouldSmooth;
        }
    }

    private record PolygonsData(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles
    );

    private record VoxelsInfo(
        HashSet<Vector3Int> voxels,
        Dictionary<Vector3Int, Normal> smoothNormalsMap
    );

    private enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }
}
}