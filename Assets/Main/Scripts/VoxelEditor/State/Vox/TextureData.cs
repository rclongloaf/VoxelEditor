namespace Main.Scripts.VoxelEditor.State.Vox
{
public record TextureData(
    int rowsCount,
    int columnsCount,
    int spriteWidth,
    int spriteHeight
)
{
    public int TextureWidth => spriteWidth * columnsCount;
    public int TextureHeight => spriteHeight * rowsCount;
}
}