namespace Main.Scripts.VoxelEditor.State
{
public interface UIState
{
    public record None : UIState;

    public record Menu : UIState;

    public record ApplySpriteChanges(EditorAction actionOnApply) : UIState;

    public record ApplyLayerDelete : UIState;

    public record TextureImport : UIState;

    public record FileBrowser : UIState;
}
}