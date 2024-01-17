using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class SpriteImportUIHolder
{
    private UIDocument doc;

    public SpriteImportUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;

        var root = doc.rootVisualElement;
        var rowsCountInput = root.Q<IntegerField>("RowsCountInput");
        var columnsCountInput = root.Q<IntegerField>("ColumnsCountInput");
        var rowIndexInput = root.Q<IntegerField>("RowIndexInput");
        var columnIndexInput = root.Q<IntegerField>("ColumnIndexInput");
        
        var applyBtn = root.Q<Button>("ApplyBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        applyBtn.clicked += () =>
        {
            listener.OnApplyImportSettings(new SpriteRectData(
                rowsCount: rowsCountInput.value,
                columnsCount: columnsCountInput.value,
                rowIndex: rowIndexInput.value - 1,
                columnIndex: columnIndexInput.value - 1
            ));
        };
        cancelBtn.clicked += listener.OnCancel;
    }

    public void SetVisibility(bool visible)
    {
        doc.rootVisualElement.SetVisibility(visible);
    }
    
    public interface Listener
    {
        public void OnApplyImportSettings(SpriteRectData spriteRectData);
        public void OnCancel();
    }
}
}