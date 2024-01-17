using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public class FreeCameraData
    {
        public readonly Vector3 pivotPoint;
        public readonly float distance;
        public readonly Quaternion rotation;

        public FreeCameraData(
            Vector3 pivotPoint,
            float distance,
            Quaternion rotation
        )
        {
            this.pivotPoint = pivotPoint;
            this.distance = distance;
            this.rotation = rotation;
        }

        public FreeCameraData Copy(
            Vector3? pivotPoint = null,
            float? distance = null,
            Quaternion? rotation = null
        )
        {
            return new FreeCameraData(
                pivotPoint: pivotPoint ?? this.pivotPoint,
                distance: distance ?? this.distance,
                rotation: rotation ?? this.rotation
            );
        }
        
    }
}