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
        var loadBtn = root.Q<Button>("LoadBtn");
        var saveBtn = root.Q<Button>("SaveBtn");
        var importBtn = root.Q<Button>("ImportBtn");
        var exportBtn = root.Q<Button>("ExportBtn");
        var brushAddBtn = root.Q<Button>("BrushAddBtn");
        var brushDeleteBtn = root.Q<Button>("BrushDeleteBtn");

        loadBtn.clicked += listener.OnLoadClicked;
        saveBtn.clicked += listener.OnSaveClicked;
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
        public void OnLoadClicked();
        public void OnSaveClicked();
        public void OnImportClicked();
        public void OnExportClicked();
        public void OnBrushAddClicked();
        public void OnBrushDeleteClicked();
    }
}
}