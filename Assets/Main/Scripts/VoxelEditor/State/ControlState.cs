﻿using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public interface ControlState
{
    public record None : ControlState;

    public record Drawing(
        List<Vector3Int> drawnVoxels,
        Vector3Int position,
        Vector3Int normal,
        bool deleting,
        bool bySection,
        bool withProjection
    ) : ControlState;

    public record Selection(Vector3 startMousePos) : ControlState;

    public record SelectionMoving(
        Vector3Int normal,
        Vector3 fromPosition,
        Vector3Int deltaOffset
    ) : ControlState;

    public record CameraMoving : ControlState;

    public record Rotating : ControlState;
}
}