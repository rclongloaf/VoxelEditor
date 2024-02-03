using Main.Scripts.Utils;
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
    
    public LayerLayoutHolder(VisualElement root)
    {
        this.root = root;
        activeRB = root.Q<RadioButton>("ActiveRB");
        visibilityToggle = root.Q<Toggle>("VisibilityToggle");
        title = root.Q<Label>("Title");
    }

    public void Bind(
        bool isActive,
        bool isVisible,
        bool needSave
    )
    {
        activeRB.value = isActive;
        activeRB.visible = isActive;

        visibilityToggle.value = isVisible;

        title.style.color = new StyleColor(needSave ? new Color(1f, 0.4f, 0.4f) : Color.white);
    }

    public void SetVisibility(bool visible)
    {
        root.SetVisibility(visible);
    }
}
}