namespace Main.Scripts.VoxelEditor.Events
{
public interface EditorEvent
{
    public class OpenBrowserForLoadVox : EditorEvent { }
    public class OpenBrowserForSaveVox : EditorEvent { }
    public class OpenBrowserForImport : EditorEvent { }
    public class OpenBrowserForExport : EditorEvent { }
}
}