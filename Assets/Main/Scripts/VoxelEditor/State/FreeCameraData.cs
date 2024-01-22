using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
public record FreeCameraData(
    Vector3 pivotPoint,
    float distance,
    Quaternion rotation
);
}