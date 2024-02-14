using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class ApplyPivotPointForAllUIHolder
{
    private UIDocument doc;
    private Label title;
    private int layerKey;

    public ApplyPivotPointForAllUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;
        var root = doc.rootVisualElement;

        title = root.Q<Label>("TitleLabel");
        var applyBtn = root.Q<Button>("ApplyBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        applyBtn.clicked += listener.OnApplyClicked;
        cancelBtn.clicked += listener.OnCancelClicked;
    }

    public void SetVisibility(bool visible)
    {
        doc.rootVisualElement.SetVisibility(visible);
    }

    public bool IsVisible()
    {
        return doc.rootVisualElement.style.display == DisplayStyle.Flex;
    }

    public interface Listener
    {
        public void OnApplyClicked();
        public void OnCancelClicked();
    }
}
}