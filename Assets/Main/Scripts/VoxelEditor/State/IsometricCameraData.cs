using UnityEngine;

namespace Main.Scripts.VoxelEditor.State
{
    public class IsometricCameraData
    {
        public readonly Vector3 position;

        public IsometricCameraData(Vector3 position)
        {
            this.position = position;
        }

        public IsometricCameraData Copy(Vector3 position)
        {
            return new IsometricCameraData(position);
        }
    }
}