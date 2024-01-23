using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class ApplyChangesUIHolder
{
    private UIDocument doc;

    public ApplyChangesUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;
        var root = doc.rootVisualElement;

        var applyBtn = root.Q<Button>("ApplyBtn");
        var discardBtn = root.Q<Button>("DiscardBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        applyBtn.clicked += listener.OnApplyClicked;
        discardBtn.clicked += listener.OnDiscardClicked;
        cancelBtn.clicked += listener.OnCancelClicked;
    }

    public void SetVisibility(bool visible)
    {
        doc.rootVisualElement.SetVisibility(visible);
    }

    public interface Listener
    {
        public void OnApplyClicked();
        public void OnDiscardClicked();
        public void OnCancelClicked();
    }
}
}