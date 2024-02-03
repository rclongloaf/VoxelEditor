using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface EditModeState
{
    public record EditMode : EditModeState;

    public record RenderMode : EditModeState;
}
}