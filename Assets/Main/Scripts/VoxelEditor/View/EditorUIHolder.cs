using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class EditorUIHolder
{
    private VisualElement root;
    
    public EditorUIHolder(UIDocument doc, Listener listener)
    {
        root = doc.rootVisualElement;
        var loadBtn = root.Q<Button>("LoadVoxBtn");
        var loadTextureBtn = root.Q<Button>("LoadTextureBtn");
        var saveBtn = root.Q<Button>("SaveVoxBtn");
        var importBtn = root.Q<Button>("ImportBtn");
        var exportBtn = root.Q<Button>("ExportBtn");
        var editModeBtn = root.Q<Button>("EditModeBtn");
        var renderModeBtn = root.Q<Button>("RenderModeBtn");
        var brushAddBtn = root.Q<Button>("BrushAddBtn");
        var brushDeleteBtn = root.Q<Button>("BrushDeleteBtn");

        loadBtn.clicked += listener.OnLoadVoxClicked;
        loadTextureBtn.clicked += listener.OnLoadTextureClicked;
        saveBtn.clicked += listener.OnSaveVoxClicked;
        importBtn.clicked += listener.OnImportClicked;
        exportBtn.clicked += listener.OnExportClicked;
        editModeBtn.clicked += listener.OnEditModeClicked;
        renderModeBtn.clicked += listener.OnRenderModeClicked;
        brushAddBtn.clicked += listener.OnBrushAddClicked;
        brushDeleteBtn.clicked += listener.OnBrushDeleteClicked;
    }

    public void SetVisibility(bool visible)
    {
        root.SetVisibility(visible);
    }

    public interface Listener
    {
        public void OnLoadVoxClicked();
        public void OnLoadTextureClicked();
        public void OnSaveVoxClicked();
        public void OnImportClicked();
        public void OnExportClicked();
        public void OnEditModeClicked();
        public void OnRenderModeClicked();
        public void OnBrushAddClicked();
        public void OnBrushDeleteClicked();
    }
}
}