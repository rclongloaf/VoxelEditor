using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class SaveOnQuitUIHolder
{
    private UIDocument doc;

    public SaveOnQuitUIHolder(UIDocument doc, Listener listener)
    {
        this.doc = doc;
        var root = doc.rootVisualElement;

        var saveBtn = root.Q<Button>("SaveBtn");
        var discardBtn = root.Q<Button>("DiscardBtn");
        var cancelBtn = root.Q<Button>("CancelBtn");

        saveBtn.clicked += listener.OnApplyClicked;
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