using System.Collections.Generic;
using Main.Scripts.Helpers;
using Main.Scripts.Helpers.MeshGeneration;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.EditModes.Render
{
public class RenderModeController
{
    private GameObject root;
    private GameObject modelPrefab;
    private Material modelMaterial;
    private Material transparentModelMaterial;

    private Dictionary<int, RenderData> renderDataMap = new();
    private Dictionary<int, GameObject> models = new();

    private bool isTransparent;

    public RenderModeController(
        GameObject root,
        GameObject modelPrefab,
        Material modelMaterial,
        Material transparentModelMaterial
    )
    {
        this.root = root;
        this.modelPrefab = modelPrefab;
        this.modelMaterial = modelMaterial;
        this.transparentModelMaterial = transparentModelMaterial;
    }

    public void ApplyLayersData(Dictionary<int, VoxLayerState> layers)
    {
        root.SetActive(true);

        foreach (var (_, model) in models)
        {
            model.SetActive(false);
        }

        foreach (var (key, layer) in layers)
        {
            if (layer is VoxLayerState.Loaded loadedLayer)
            {
                if (!renderDataMap.ContainsKey(key)
                    || renderDataMap[key].spriteData != loadedLayer.currentSpriteData
                    || renderDataMap[key].texture != loadedLayer.texture)
                {
                    var meshGenerator = new MeshFromVoxelsGenerator(
                        loadedLayer.currentSpriteData.voxels,
                        loadedLayer.voxData.textureData.TextureWidth,
                        loadedLayer.voxData.textureData.TextureHeight,
                        loadedLayer.voxData.textureData,
                        loadedLayer.currentSpriteIndex,
                        loadedLayer.currentSpriteData.pivot,
                        1,
                        false
                    );

                    var mesh = meshGenerator.GenerateMesh();

                    renderDataMap[key] = new RenderData(
                        spriteData: loadedLayer.currentSpriteData,
                        mesh: mesh,
                        texture: loadedLayer.texture
                    );
                    
                    MeshRenderer meshRenderer;
                    if (!models.TryGetValue(key, out var model))
                    {
                        model = Object.Instantiate(modelPrefab, root.transform);
                        models[key] = model;
                        meshRenderer = model.GetComponent<MeshRenderer>();
                        meshRenderer.sharedMaterial = isTransparent ? transparentModelMaterial : modelMaterial;
                    }
                    else
                    {
                        meshRenderer = model.GetComponent<MeshRenderer>();
                    }

                    var renderData = renderDataMap[key];
                    model.GetComponent<MeshFilter>().sharedMesh = renderData.mesh;
                    meshRenderer.material.mainTexture = renderData.texture;
                }
                
                models[key].SetActive(loadedLayer.isVisible);
            }
        }
    }

    public void SetVisibility(bool visible)
    {
        root.SetActive(visible);
    }

    public void SetTransparentMaterial(bool isTransparent)
    {
        foreach (var (_, model) in models)
        {
            var meshRenderer = model.GetComponent<MeshRenderer>();
            var texture = meshRenderer.material.mainTexture;
            meshRenderer.sharedMaterial = isTransparent ? transparentModelMaterial : modelMaterial;
            meshRenderer.material.mainTexture = texture;
        }
    }
}
}