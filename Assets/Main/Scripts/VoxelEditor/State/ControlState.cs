using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface ControlState
{
    public record None : ControlState;

    public record Drawing(
        Vector3Int position,
        Vector3Int normal,
        bool deleting,
        bool bySection,
        bool withProjection
    ) : ControlState;

    public record Moving : ControlState;

    public record Rotating : ControlState;
}
}