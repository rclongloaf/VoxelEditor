using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class ApplyDeleteLayerUIHolder
{
    private UIDocument doc;
    private Label title;
    private int layerKey;

    public ApplyDeleteLayerUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;
        var root = doc.rootVisualElement;

        title = root.Q<Label>("TitleLabel");
        var applyBtn = root.Q<Button>("ApplyBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        applyBtn.clicked += () =>
        {
            listener.OnApplyClicked(layerKey);
        };
        cancelBtn.clicked += listener.OnCancelClicked;
    }

    public void Bind(int layerKey)
    {
        this.layerKey = layerKey;
        title.text = $"Do apply deleting layer {layerKey}?";
    }

    public void SetVisibility(bool visible)
    {
        doc.rootVisualElement.SetVisibility(visible);
    }

    public interface Listener
    {
        public void OnApplyClicked(int layerKey);
        public void OnCancelClicked();
    }
}
}