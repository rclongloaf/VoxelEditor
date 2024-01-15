using System;
using System.Collections.Generic;
using System.IO;
using Main.Scripts.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using SimpleFileBrowser;

namespace Main.VoxelEditor
{
public class VoxelEditor : MonoBehaviour
{
    private static readonly int PixelsPerUnit = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int SpriteTexture = Shader.PropertyToID("_SpriteTexture");
    private static readonly int SpritePivot = Shader.PropertyToID("_SpritePivot");
    private static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
    private static readonly int SpriteRectPosition = Shader.PropertyToID("_SpriteRectPosition");
    private static readonly int IsSelected = Shader.PropertyToID("_IsSelected");

    [SerializeField]
    private Sprite sprite = null!;
    [SerializeField]
    private SpriteRenderer spriteRefRenderer = null!;
    [SerializeField]
    private float referenceOffset = 3;
    [SerializeField]
    private Material material = null!;
    [SerializeField]
    private Camera freeCamera = null!;
    [SerializeField]
    private Camera isometricCamera = null!;
    [SerializeField]
    private float cameraSpeed = 1f;

    private Dictionary<Vector3Int, GameObject> voxelsMap = new();
    private Stack<(bool, HashSet<Vector3Int>)> actionsHistory = new();

    private EditorState state;
    private bool isFreeCamera = true;
    private MaterialPropertyBlock selectedBlock;
    private MaterialPropertyBlock notSelectedBlock;
    private MeshRenderer lastSelectedRenderer;
    private Vector3Int actionStartPosition;
    private Vector3Int lastActionPosition;

    private void Awake()
    {
        spriteRefRenderer.sprite = sprite;
        spriteRefRenderer.transform.position = new Vector3(sprite.pivot.x, sprite.pivot.y - referenceOffset, referenceOffset);
        spriteRefRenderer.transform.localScale *= sprite.pixelsPerUnit;
        
        selectedBlock = new MaterialPropertyBlock();
        selectedBlock.SetFloat(IsSelected, 1f);
        notSelectedBlock = new MaterialPropertyBlock();
        notSelectedBlock.SetFloat(IsSelected, 0f);
        
        material.SetTexture(SpriteTexture, sprite.texture);
        // material.SetFloat(PixelsPerUnit, sprite.pixelsPerUnit);
        // material.SetVector(SpritePivot, sprite.pivot);
        material.SetFloat(PixelsPerUnit, 1);
        material.SetVector(SpritePivot, Vector2.zero);
        material.SetVector(TextureSize, sprite.texture.Size());
        material.SetVector(SpriteRectPosition, sprite.rect.position);

        OpenVoxFile();
    }

    public void Update()
    {
        var isLeftClick = Input.GetKey(KeyCode.Mouse0);
        var isRightClick = Input.GetKey(KeyCode.Mouse1);
        var isMiddleClick = Input.GetKey(KeyCode.Mouse2);
        var mouseDeltaX = Input.GetAxis("Mouse X");
        var mouseDeltaY = Input.GetAxis("Mouse Y");
        var mouseWheel = Input.GetAxisRaw("Mouse ScrollWheel") * cameraSpeed;

        var moveDelta = Vector3.zero;
        if (!isMiddleClick && mouseWheel == 0f)
        {
            if (Input.GetKey(KeyCode.A))
            {
                moveDelta += Vector3.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                moveDelta += Vector3.right;
            }

            if (Input.GetKey(KeyCode.W))
            {
                moveDelta += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                moveDelta += Vector3.back;
            }

            if (Input.GetKey(KeyCode.E))
            {
                moveDelta += Vector3.up;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                moveDelta += Vector3.down;
            }
        }

        var isMove = moveDelta != Vector3.zero || isMiddleClick || mouseWheel != 0;

        isFreeCamera = (isFreeCamera || Input.GetKey(KeyCode.Alpha1)) && !Input.GetKey(KeyCode.Alpha2);

        freeCamera.enabled = isFreeCamera;
        isometricCamera.enabled = !isFreeCamera;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spriteRefRenderer.enabled = !spriteRefRenderer.enabled;
        }

        material.SetFloat(IsSelected, Input.GetKey(KeyCode.F) ? 1f : 0f);

        var isBlockSelected = false;
        Vector3Int selectedPosition = default;
        Vector3 selectedNormal = default;
        if (state == EditorState.Free)
        {
            isBlockSelected = GetVoxelUnderCursor(out selectedPosition, out selectedNormal);
        }
        else if (state is EditorState.Adding or EditorState.Removing)
        {
            isBlockSelected = true;
            CursorUtils.GetCursorWorldPosition(
                GetCurrentCamera(),
                new Plane(Vector3.forward, actionStartPosition + Vector3.forward * 0.5f),
                out var cursorPosition
            );
            selectedPosition = Vector3Int.RoundToInt(cursorPosition - new Vector3(0.5f, 0.5f, 1f));
        }

        if (lastSelectedRenderer != null)
        {
            lastSelectedRenderer.SetPropertyBlock(null);
            if (!isBlockSelected)
            {
                lastSelectedRenderer = null;
            }
        }

        if (isBlockSelected && voxelsMap.TryGetValue(selectedPosition, out var voxel))
        {
            lastSelectedRenderer = voxel.GetComponent<MeshRenderer>();
            
            lastSelectedRenderer.SetPropertyBlock(selectedBlock);
        }

        switch (state)
        {
            case EditorState.Free:
                if (isMove || (!isBlockSelected && isRightClick))
                {
                    state = EditorState.Moving;
                }
                else if (isLeftClick && isBlockSelected)
                {
                    state = EditorState.Adding;
                    var addPosition = Vector3Int.RoundToInt(selectedPosition + selectedNormal);
                    actionStartPosition = addPosition;
                    lastActionPosition = actionStartPosition;
                    actionsHistory.Push((true, new HashSet<Vector3Int>()));
                    AddVoxel(addPosition);
                }
                else if (isRightClick && isBlockSelected)
                {
                    state = EditorState.Removing;
                    actionStartPosition = selectedPosition;
                    lastActionPosition = actionStartPosition;
                    actionsHistory.Push((false, new HashSet<Vector3Int>()));
                    RemoveVoxel(selectedPosition);
                }
                else if (Input.GetKey(KeyCode.Y))
                {
                    SaveVoxFile();
                    SaveMesh();
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (actionsHistory.TryPop(out var t))
                    {
                        var isAddAction = t.Item1;
                        foreach (var voxPosition in t.Item2)
                        {
                            if (isAddAction)
                            {
                                RemoveVoxel(voxPosition);
                            }
                            else
                            {
                                AddVoxel(voxPosition);
                            }
                        }
                    }
                }
                break;
            case EditorState.Adding:
                if (!isLeftClick)
                {
                    state = EditorState.Free;
                }
                else if (isBlockSelected)
                {
                    // RemoveVoxels(actionStartPosition, lastActionPosition);
                    var curActionPosition =
                        new Vector3Int(selectedPosition.x, selectedPosition.y, actionStartPosition.z);
                    AddVoxels(actionStartPosition, curActionPosition);
                    lastActionPosition = curActionPosition;
                }

                break;
            case EditorState.Removing:
                if (!isRightClick)
                {
                    state = EditorState.Free;
                }
                else if (isBlockSelected)
                {
                    // AddVoxels(actionStartPosition, lastActionPosition);
                    var curActionPosition =
                        new Vector3Int(selectedPosition.x, selectedPosition.y, actionStartPosition.z);
                    RemoveVoxels(actionStartPosition, curActionPosition);
                    lastActionPosition = curActionPosition;
                }

                break;
            case EditorState.Moving:
                var currentCamera = GetCurrentCamera();
                if (!isMove && !isRightClick)
                {
                    state = EditorState.Free;
                }
                else
                {
                    if (isMove)
                    {
                        Vector3 deltaPosition;
                        if (isMiddleClick)
                        {
                            deltaPosition = new Vector3(-mouseDeltaX, -mouseDeltaY, 0) * 0.1f;
                        }
                        else if (mouseWheel != 0f)
                        {
                            deltaPosition = mouseWheel * Vector3.forward;
                        }
                        else
                        {
                            var moveDeltaNormalized = moveDelta.normalized;
                            deltaPosition = moveDeltaNormalized * cameraSpeed * Time.deltaTime;
                        }
                        
                        currentCamera.transform.position += currentCamera.transform.rotation * deltaPosition;
                    }

                    if (isRightClick && isFreeCamera)
                    {
                        var eulerAngles = currentCamera.transform.rotation.eulerAngles;

                        var rotationX = eulerAngles.y;
                        var rotationY = eulerAngles.x;
                        
                        rotationX += mouseDeltaX * 1f;
                        rotationY -= mouseDeltaY * 1f;
                        
                        rotationX = ClampAngle(rotationX, -180, 180);
                        rotationY = ClampAngle(rotationY, -90, 90);

                        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
                        
                        currentCamera.transform.rotation = xQuaternion * yQuaternion;
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OpenVoxFile()
    {
        FileBrowser.ShowLoadDialog(
            onSuccess: OnOpenFileSuccess,
            onCancel: OnOpenFileCancel,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Load",
            loadButtonText: "Select"
        );
    }

    private void SaveVoxFile()
    {
        FileBrowser.ShowSaveDialog(
            onSuccess: OnSaveFileSuccess,
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Save",
            saveButtonText: "Save"
        );
    }

    private void OnOpenFileSuccess(string[] paths)
    {
        if (!TryReadVox(paths[0]))
        {
            GenerateVoxFromSprite();
        }
    }

    private void OnOpenFileCancel()
    {
        GenerateVoxFromSprite();
    }

    private void GenerateVoxFromSprite()
    {
        var rect = sprite.rect;
        var texture = sprite.texture;
        
        for (var x = 0; x < rect.width; x++)
        {
            for (var y = 0; y < rect.height; y++)
            {
                var pixel = texture.GetPixel((int)rect.x + x, (int)rect.y + y);
                if (pixel.a > 0.5f)
                {
                    AddVoxel(new Vector3Int(x, y, 0));
                }
            }
        }
    }

    private void OnSaveFileSuccess(string[] paths)
    {
        var jObject = new JObject();
        var voxelsList = new JArray();

        foreach (var (voxelPosition, _) in voxelsMap)
        {
            var jPos = new JObject();
            jPos.Add("x", voxelPosition.x);
            jPos.Add("y", voxelPosition.y);
            jPos.Add("z", voxelPosition.z);
            voxelsList.Add(jPos);
        }

        jObject.Add("version", 1);
        jObject.Add("data", voxelsList);

        var path = $"{paths[0]}_vox.json";
        using var streamWriter = File.CreateText(path);
        using var jsonWriter = new JsonTextWriter(streamWriter);
        jObject.WriteTo(jsonWriter);
    }

    private bool TryReadVox(string path)
    {
        // var path = $"{AssetDatabase.GetAssetPath(sprite)}_vox.json";

        if (!File.Exists(path))
        {
            return false;
        }
        
        using var streamReader = File.OpenText(path);
        using var jsonReader = new JsonTextReader(streamReader);

        var jObject = (JObject)JToken.ReadFrom(jsonReader);

        var voxelsList = (JArray)jObject.GetValue("data");
        foreach (var jVoxelData in voxelsList)
        {
            var x = (int)((JObject)jVoxelData).GetValue("x");
            var y = (int)((JObject)jVoxelData).GetValue("y");
            var z = (int)((JObject)jVoxelData).GetValue("z");
            AddVoxel(new Vector3Int(x, y, z));
        }

        return true;
    }

    private void SaveMesh()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        var normals = new List<Vector3>();

        var pivot = sprite.pivot;
        var voxSize = 1 / sprite.pixelsPerUnit;
        var centerOffset = new Vector3(pivot.x, pivot.y, 0);
        var rect = sprite.rect;
        var texWidth = sprite.texture.width;
        var texHeight = sprite.texture.height;
        
        foreach (var (voxelPosition, _) in voxelsMap)
        {
            AddCubeVert(voxelPosition, 1, vertices, triangles);
        }

        for (var i = 0; i < vertices.Count; i++)
        {
            var vert = vertices[i];
            var pixelPos = new Vector2(
                vert.x + rect.x,
                (vert.y + vert.z) + rect.y
            );
            uv.Add(new Vector2(pixelPos.x / texWidth, pixelPos.y / texHeight));
            normals.Add(Vector3.back);
        }

        var mesh = new Mesh();
        mesh.vertices = vertices.ConvertAll(vert => (vert - centerOffset) * voxSize).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();
        
        var path = $"{AssetDatabase.GetAssetPath(sprite)}_3Dmesh.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    private void AddCubeVert(Vector3 position, float size, List<Vector3> vertices, List<int> triangles)
    {
        var halfSize = size / 2f;
        var vertStartIndex = vertices.Count;
        
        vertices.Add(new Vector3(position.x - halfSize, position.y - halfSize, position.z - halfSize));
        vertices.Add(new Vector3(position.x - halfSize, position.y + halfSize, position.z - halfSize));
        vertices.Add(new Vector3(position.x - halfSize, position.y - halfSize, position.z + halfSize));
        vertices.Add(new Vector3(position.x - halfSize, position.y + halfSize, position.z + halfSize));
        vertices.Add(new Vector3(position.x + halfSize, position.y - halfSize, position.z - halfSize));
        vertices.Add(new Vector3(position.x + halfSize, position.y + halfSize, position.z - halfSize));
        vertices.Add(new Vector3(position.x + halfSize, position.y - halfSize, position.z + halfSize));
        vertices.Add(new Vector3(position.x + halfSize, position.y + halfSize, position.z + halfSize));

        var LBB = vertStartIndex + 0;
        var LTB = vertStartIndex + 1;
        var LBF = vertStartIndex + 2;
        var LTF = vertStartIndex + 3;
        var RBB = vertStartIndex + 4;
        var RTB = vertStartIndex + 5;
        var RBF = vertStartIndex + 6;
        var RTF = vertStartIndex + 7;
        
        triangles.Add(LTB); triangles.Add(RTB); triangles.Add(RBB);
        triangles.Add(LTB); triangles.Add(RBB); triangles.Add(LBB);
        
        triangles.Add(RTF); triangles.Add(LTF); triangles.Add(LBF);
        triangles.Add(RTF); triangles.Add(LBF); triangles.Add(RBF);
        
        triangles.Add(LTF); triangles.Add(LTB); triangles.Add(LBB);
        triangles.Add(LTF); triangles.Add(LBB); triangles.Add(LBF);
        
        triangles.Add(RTB); triangles.Add(RTF); triangles.Add(RBF);
        triangles.Add(RTB); triangles.Add(RBF); triangles.Add(RBB);
        
        triangles.Add(LTF); triangles.Add(RTF); triangles.Add(RTB);
        triangles.Add(LTF); triangles.Add(RTB); triangles.Add(LTB);
        
        triangles.Add(LBB); triangles.Add(RBB); triangles.Add(RBF);
        triangles.Add(LBB); triangles.Add(RBF); triangles.Add(LBF);
    }

    private void AddVoxels(Vector3Int from, Vector3Int to)
    {
        var fromX = Math.Min(from.x, to.x);
        var toX = Math.Max(from.x, to.x);
                    
        var fromY = Math.Min(from.y, to.y);
        var toY = Math.Max(from.y, to.y);
                    
        var fromZ = Math.Min(from.z, to.z);
        var toZ = Math.Max(from.z, to.z);

        for (var x = fromX; x <= toX; x++)
        {
            for (var y = fromY; y <= toY; y++)
            {
                for (var z = fromZ; z <= toZ; z++)
                {
                    AddVoxel(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private void RemoveVoxels(Vector3Int from, Vector3Int to)
    {
        var fromX = Math.Min(from.x, to.x);
        var toX = Math.Max(from.x, to.x);
                    
        var fromY = Math.Min(from.y, to.y);
        var toY = Math.Max(from.y, to.y);
                    
        var fromZ = Math.Min(from.z, to.z);
        var toZ = Math.Max(from.z, to.z);

        for (var x = fromX; x <= toX; x++)
        {
            for (var y = fromY; y <= toY; y++)
            {
                for (var z = fromZ; z <= toZ; z++)
                {
                    RemoveVoxel(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private bool GetVoxelUnderCursor(out Vector3Int position, out Vector3 normal)
    {
        var ray = GetCurrentCamera().ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo))
        {
            position = Vector3Int.RoundToInt(hitInfo.transform.position - Vector3.one * 0.5f);
            normal = hitInfo.normal;
            return true;
        }

        position = Vector3Int.zero;
        normal = Vector3.zero;

        return false;
    }

    private Camera GetCurrentCamera()
    {
        return isFreeCamera ? freeCamera : isometricCamera;
    }

    private void AddVoxel(Vector3Int position)
    {
        if (voxelsMap.ContainsKey(position))
        {
            return;
        }
        if (actionsHistory.TryPeek(out var t))
        {
            t.Item2.Add(position);
        }
        var voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var meshRenderer = voxel.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        voxelsMap.Add(position, voxel);

        var pivot = sprite.pivot;
        var pixelsPerUnit = sprite.pixelsPerUnit;

        var centerOffset = new Vector3(pivot.x, pivot.y, 0);

        voxel.transform.position = Vector3.one * 0.5f + position;
        // voxel.transform.position = (position - centerOffset) / pixelsPerUnit;
        // voxel.transform.localScale /= pixelsPerUnit;
    }

    private void RemoveVoxel(Vector3Int position)
    {
        if (voxelsMap.TryGetValue(position, out var voxel))
        {
            if (actionsHistory.TryPeek(out var t))
            {
                t.Item2.Add(position);
            }
            voxelsMap.Remove(position);
            Destroy(voxel);
        }
    }
    
    private static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -180F)
            angle += 360F;
        if (angle > 180F)
            angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }

    private enum EditorState
    {
        Free,
        Adding,
        Removing,
        Moving
    }
}
}