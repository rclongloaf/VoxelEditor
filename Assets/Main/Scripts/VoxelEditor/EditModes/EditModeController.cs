using System;
using System.Collections.Generic;
using Main.Scripts.Helpers.MeshGeneration;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.VoxelEditor.EditModes
{
public class EditModeController
{
    private static readonly int SpriteTexture = Shader.PropertyToID("_SpriteTexture");
    private static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
    private static readonly int SpriteRectPosition = Shader.PropertyToID("_SpriteRectPosition");
    private static readonly int PivotPoint = Shader.PropertyToID("_PivotPoint");
    private static readonly int IsGridEnabled = Shader.PropertyToID("_IsGridEnabled");
    private static readonly int IsSelected = Shader.PropertyToID("_IsSelected");
    private static readonly int IsFillInvisible = Shader.PropertyToID("_IsFillInvisible");

    private GameObject root;
    private SpriteRenderer spriteReference;
    private GameObject voxelPrefab;
    private Material material;
    private Texture2D? texture;
    private TextureData? textureData;
    private SpriteIndex? spriteIndex;
    private Vector2 pivotPoint;
    private Sprite? cachedSpriteRef;
    private bool isActive;
    private ShaderData shaderData;
    private MaterialPropertyBlock materialPropertyBlock = new();

    private Dictionary<Vector3Int, GameObject> currentVoxelsObjects = new();
    private Dictionary<Vector3Int, VoxelData> currentVoxelsData = new();

    private SelectionState cachedSelectionState = new SelectionState.None();
    private Dictionary<Vector3Int, GameObject> selectedVoxels = new();
    private Dictionary<Normal, Mesh> meshByNormal;

    public EditModeController(
        GameObject root,
        SpriteRenderer spriteReference,
        GameObject voxelPrefab,
        Material voxelMaterial
    )
    {
        this.root = root;
        this.spriteReference = spriteReference;
        this.voxelPrefab = voxelPrefab;
        material = new Material(voxelMaterial);
        shaderData = new ShaderData(false, false);
        meshByNormal = VoxelsSmoothHelper.GenerateMeshesPerNormals();
    }
    
    public void Release()
    {
        Object.Destroy(root);
    }

    public void SetVisibility(bool isVisible)
    {
        root.SetActive(isVisible);
    }

    public void ApplyVoxels(Dictionary<Vector3Int, VoxelData> voxels)
    {
        foreach (var (pos, data) in voxels)
        {
            if (!currentVoxelsObjects.ContainsKey(pos))
            {
                AddVoxel(pos, data);
            }
        }

        var voxelsToRemove = new List<Vector3Int>();

        foreach (var (pos, _) in currentVoxelsObjects)
        {
            if (!voxels.ContainsKey(pos))
            {
                voxelsToRemove.Add(pos);
            }
        }

        foreach (var voxel in voxelsToRemove)
        {
            RemoveVoxel(voxel);
        }

        currentVoxelsData = voxels;
        UpdateSmoothState();
    }

    public void ApplySelection(SelectionState selectionState)
    {
        switch (selectionState)
        {
            case SelectionState.None none:
                foreach (var (_, voxelObj) in selectedVoxels)
                {
                    Object.Destroy(voxelObj);
                }

                selectedVoxels.Clear();
                foreach (var (_, voxel) in currentVoxelsObjects)
                {
                    voxel.SetActive(true);
                }
                break;
            case SelectionState.Selected selected:
                foreach (var (_, voxelObj) in selectedVoxels)
                {
                    Object.Destroy(voxelObj);
                }

                selectedVoxels.Clear();
                foreach (var (position, voxelData) in selected.voxels)
                {
                    var voxel = CreateVoxel(position + selected.offset);
                    var renderer = voxel.GetComponentInChildren<MeshRenderer>();
                    renderer.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetFloat(IsSelected, 1);
                    renderer.SetPropertyBlock(materialPropertyBlock);

                    selectedVoxels[position] = voxel;
                }

                foreach (var (position, voxel) in currentVoxelsObjects)
                {
                    voxel.SetActive(!selectedVoxels.ContainsKey(position - selected.offset));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(selectionState));
        }

        cachedSelectionState = selectionState;
    }

    public void ApplySpriteRect(TextureData textureData, SpriteIndex spriteIndex)
    {
        this.textureData = textureData;
        this.spriteIndex = spriteIndex;
        if (texture != null)
        {
            var width = texture.width / textureData.columnsCount;
            var height = texture.height / textureData.rowsCount;
            
            var rectPosition = new Vector2(
                spriteIndex.columnIndex * width,
                (textureData.rowsCount - spriteIndex.rowIndex - 1) * height
            );
            material.SetVector(SpriteRectPosition, rectPosition);
            
            cachedSpriteRef = Sprite.Create(texture, new Rect(rectPosition.x, rectPosition.y, width, height), Vector2.zero, 1);
            spriteReference.sprite = cachedSpriteRef;
            spriteReference.transform.position = new Vector3(0, 20, -20) - (Vector3)pivotPoint;
        }
    }

    public void ApplyPivotPoint(Vector2 pivotPoint)
    {
        this.pivotPoint = pivotPoint;
        material.SetVector(PivotPoint, pivotPoint);
        
        spriteReference.transform.position = new Vector3(0, 20, -20) - (Vector3)pivotPoint;
        foreach (var (pos, voxel) in currentVoxelsObjects)
        {
            voxel.transform.position = pos - (Vector3)pivotPoint;
        }

        if (cachedSelectionState is SelectionState.Selected selectionState)
        {
            foreach (var (pos, voxel) in selectedVoxels)
            {
                voxel.transform.position = pos + selectionState.offset - (Vector3)pivotPoint;
            }
        }
    }

    public void ApplyTexture(Texture2D? texture)
    {
        this.texture = texture;
        material.SetTexture(SpriteTexture, texture);
        if (texture != null)
        {
            material.SetVector(TextureSize, new Vector2(texture.width, texture.height));
            if (textureData != null && spriteIndex != null)
            {
                ApplySpriteRect(textureData, spriteIndex);
            }
        }
    }

    public void ApplyShaderData(ShaderData shaderData)
    {
        this.shaderData = shaderData;
        material.SetFloat(IsGridEnabled, shaderData.isGridEnabled && isActive ? 1 : 0);
        material.SetFloat(IsFillInvisible, shaderData.isTransparentEnabled ? 0 : 1);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        ApplyShaderData(shaderData);
        foreach (var (_, voxel) in currentVoxelsObjects)
        {
            voxel.GetComponentInChildren<Collider>().enabled = isActive;
        }
        foreach (var (_, voxel) in selectedVoxels)
        {
            voxel.GetComponentInChildren<Collider>().enabled = isActive;
        }
    }

    public void SetReferenceVisibility(bool visible)
    {
        if (texture == null
            || textureData == null
            || spriteIndex == null) return;

        if (visible)
        {
            spriteReference.sprite = cachedSpriteRef;
            spriteReference.transform.position = new Vector3(0, 20, -20) - (Vector3)pivotPoint;
        }
        
        spriteReference.gameObject.SetActive(visible);
    }

    private void AddVoxel(Vector3Int position, VoxelData data)
    {
        if (currentVoxelsObjects.ContainsKey(position))
        {
            return;
        }

        var voxel = CreateVoxel(position);
        
        currentVoxelsObjects.Add(position, voxel);
    }

    private GameObject CreateVoxel(Vector3Int position)
    {
        var voxel = Object.Instantiate(voxelPrefab, root.transform);

        var meshRenderer = voxel.GetComponentInChildren<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        voxel.transform.position = position - (Vector3)pivotPoint;

        return voxel;
    }

    private void RemoveVoxel(Vector3Int position)
    {
        if (currentVoxelsObjects.TryGetValue(position, out var voxel))
        {
            currentVoxelsObjects.Remove(position);
            Object.Destroy(voxel);
        }
    }

    private void UpdateSmoothState()
    {
        var smoothNormalMap = VoxelsSmoothHelper.GetSmoothNormalMap(currentVoxelsData);
        foreach (var (pos, normal) in smoothNormalMap)
        {
            var obj = currentVoxelsObjects[pos];
            var meshFilter = obj.GetComponentInChildren<MeshFilter>();
            meshFilter.sharedMesh = meshByNormal[normal];
        }
    }
}
}