using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class TextureImportUIHolder
{
    private UIDocument doc;

    public TextureImportUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;

        var root = doc.rootVisualElement;
        var rowsCountInput = root.Q<IntegerField>("RowsCountInput");
        var columnsCountInput = root.Q<IntegerField>("ColumnsCountInput");
        
        var applyBtn = root.Q<Button>("ApplyBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        applyBtn.clicked += () =>
        {
            listener.OnApplyImportSettings(
                rowsCountInput.value,
                columnsCountInput.value
            );
        };
        cancelBtn.clicked += listener.OnCancel;
    }

    public void SetVisibility(bool visible)
    {
        doc.rootVisualElement.SetVisibility(visible);
    }
    
    public interface Listener
    {
        public void OnApplyImportSettings(int rowsCount, int columnsCount);
        public void OnCancel();
    }
}
}