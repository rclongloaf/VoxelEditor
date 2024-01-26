using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using Unity.VisualScripting;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.EditModes
{
public class EditModeController
{
    private static readonly int SpriteTexture = Shader.PropertyToID("_SpriteTexture");
    private static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
    private static readonly int SpriteRectPosition = Shader.PropertyToID("_SpriteRectPosition");
    private static readonly int IsSelected = Shader.PropertyToID("_IsSelected");
    private static readonly int IsFillInvisible = Shader.PropertyToID("_IsFillInvisible");

    private GameObject root;
    private SpriteRenderer spriteReference;
    private GameObject voxelPrefab;
    private Material material;
    private Texture2D? texture;
    private TextureData? textureData;
    private SpriteIndex? spriteIndex;

    private Dictionary<Vector3Int, GameObject> currentVoxels = new();

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
        material = voxelMaterial;
    }

    public void SetVisibility(bool isVisible)
    {
        root.SetActive(isVisible);
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
            
            spriteReference.sprite = Sprite.Create(texture, new Rect(rectPosition.x, rectPosition.y, width, height), Vector2.zero, 1);
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
        material.SetFloat(IsSelected, shaderData.isGridEnabled ? 1 : 0);
        material.SetFloat(IsFillInvisible, shaderData.isTransparentEnabled ? 0 : 1);
    }

    public void SetReferenceVisibility(bool visible)
    {
        spriteReference.gameObject.SetActive(visible);
    }

    private void AddVoxel(Vector3Int position)
    {
        if (currentVoxels.ContainsKey(position))
        {
            return;
        }
        
        var voxel = Object.Instantiate(voxelPrefab, root.transform);

        currentVoxels.Add(position, voxel);
        var meshRenderer = voxel.GetComponentInChildren<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        voxel.transform.position = position;
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