using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.EditModes.Render
{
public class RenderModeController
{
    private GameObject root;
    private GameObject modelPrefab;
    private Material modelMaterial;

    private Dictionary<int, RenderData> renderDataMap = new();
    private Dictionary<int, GameObject> models = new();

    public RenderModeController(
        GameObject root,
        GameObject modelPrefab,
        Material modelMaterial
    )
    {
        this.root = root;
        this.modelPrefab = modelPrefab;
        this.modelMaterial = modelMaterial;
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
                    renderDataMap[key] = new RenderData(
                        spriteData: loadedLayer.currentSpriteData,
                        mesh: MeshGenerationHelper.GenerateMesh(
                            spriteData: loadedLayer.currentSpriteData,
                            pixelsPerUnit: 1,
                            textureData: loadedLayer.voxData.textureData,
                            spriteIndex: loadedLayer.currentSpriteIndex,
                            textureWidth: loadedLayer.texture?.width ?? 1,
                            textureHeight: loadedLayer.texture?.height ?? 1
                        ),
                        texture: loadedLayer.texture
                    );
                    
                    MeshRenderer meshRenderer;
                    if (!models.TryGetValue(key, out var model))
                    {
                        model = Object.Instantiate(modelPrefab, root.transform);
                        models[key] = model;
                        meshRenderer = model.GetComponent<MeshRenderer>();
                        // meshRenderer.sharedMaterial = modelMaterial;
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
}
}