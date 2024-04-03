using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface ControlState
{
    public record None : ControlState;

    public record Drawing(
        Dictionary<Vector3Int, VoxelData> drawnVoxels,
        Vector3Int position,
        Vector3Int normal,
        bool deleting,
        bool bySection,
        bool withProjection
    ) : ControlState;

    public record Smoothing(
        HashSet<Vector3Int> smoothVoxels,
        bool enableSmooth
    ) : ControlState;
    
    public record Selection(Vector3 startMousePos) : ControlState;

    public record SelectionMoving(
        Vector3Int normal,
        Vector3 fromPosition,
        Vector3Int deltaOffset
    ) : ControlState;
    
    public record RotatingVoxels(
        Axis axis,
        float angle,
        Dictionary<Vector3Int, VoxelData> voxels
    ) : ControlState;

    public record CameraMoving : ControlState;
    
    public record RotatingCamera : ControlState;
}
}