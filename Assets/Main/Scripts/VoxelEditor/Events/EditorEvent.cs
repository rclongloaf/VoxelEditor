namespace Main.Scripts.VoxelEditor.Events
{
public interface EditorEvent
{
    public record OpenBrowserForLoadVox : EditorEvent;

    public record OpenBrowserForLoadTexture : EditorEvent;

    public record OpenBrowserForSaveVox(bool exitOnSave) : EditorEvent;

    public record OpenBrowserForImport : EditorEvent;

    public record OpenBrowserForExportSingle : EditorEvent;

    public record OpenBrowserForExportAll : EditorEvent;

    public record DeleteLayerRequest(int key) : EditorEvent;
}
}