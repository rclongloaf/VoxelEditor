using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class LayerLayoutHolder
{
    private VisualElement root;
    private RadioButton activeRB;
    private Toggle visibilityToggle;
    private Label title;
    private Label spriteIndexLabel;
    
    public LayerLayoutHolder(VisualElement root)
    {
        this.root = root;
        activeRB = root.Q<RadioButton>("ActiveRB");
        visibilityToggle = root.Q<Toggle>("VisibilityToggle");
        title = root.Q<Label>("Title");
        spriteIndexLabel = root.Q<Label>("SpriteIndexLabel");
    }

    public void Bind(
        bool isActive,
        bool isVisible,
        bool needSave,
        SpriteIndex? spriteIndex
    )
    {
        activeRB.value = isActive;
        activeRB.visible = isActive;

        visibilityToggle.value = isVisible;

        title.style.color = new StyleColor(needSave ? new Color(1f, 0.4f, 0.4f) : Color.white);

        spriteIndexLabel.text = spriteIndex != null ? $"{spriteIndex.rowIndex + 1}-{spriteIndex.columnIndex + 1}" : "";
    }

    public void SetVisibility(bool visible)
    {
        root.SetVisibility(visible);
    }
}
}