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
        var brushAddBtn = root.Q<Button>("BrushAddBtn");
        var brushDeleteBtn = root.Q<Button>("BrushDeleteBtn");

        loadBtn.clicked += listener.OnLoadVoxClicked;
        loadTextureBtn.clicked += listener.OnLoadTextureClicked;
        saveBtn.clicked += listener.OnSaveVoxClicked;
        importBtn.clicked += listener.OnImportClicked;
        exportBtn.clicked += listener.OnExportClicked;
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
        public void OnBrushAddClicked();
        public void OnBrushDeleteClicked();
    }
}
}