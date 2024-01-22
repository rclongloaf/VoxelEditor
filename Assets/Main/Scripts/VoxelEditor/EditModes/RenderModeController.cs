using UnityEngine;

namespace Main.Scripts.VoxelEditor.EditModes
{
public class RenderModeController
{
    private GameObject root;
    private MeshFilter modelMeshFilter;
    private MeshRenderer modelMeshRenderer;
    private Material modelMaterial;
    private static readonly int MainTexture = Shader.PropertyToID("_MainTex");

    public RenderModeController(
        GameObject root,
        MeshFilter modelMeshFilter,
        MeshRenderer modelMeshRenderer,
        Material modelMaterial
    )
    {
        this.root = root;
        this.modelMeshFilter = modelMeshFilter;
        this.modelMeshRenderer = modelMeshRenderer;
        this.modelMaterial = modelMaterial;
    }

    public void Show(Mesh mesh, Texture2D? texture)
    {
        root.SetActive(true);
        modelMeshFilter.sharedMesh = mesh;
        modelMaterial.mainTexture = texture;
        modelMeshRenderer.sharedMaterial = modelMaterial;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
}