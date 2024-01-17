namespace Main.Scripts.VoxelEditor.State
{
public class SpriteRectData
{
    public readonly int rowsCount;
    public readonly int columnsCount;
    public readonly int rowIndex;
    public readonly int columnIndex;

    public SpriteRectData(
        int rowsCount,
        int columnsCount,
        int rowIndex,
        int columnIndex
    )
    {
        this.rowsCount = rowsCount;
        this.columnsCount = columnsCount;
        this.rowIndex = rowIndex;
        this.columnIndex = columnIndex;
    }
}
}