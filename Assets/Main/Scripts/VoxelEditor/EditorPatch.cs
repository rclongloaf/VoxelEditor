using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor
{
public interface EditorPatch
{
    public interface FileBrowser : EditorPatch
    {
        public class Opened : FileBrowser { }
        public class Closed : FileBrowser { }
    }

    public interface Import : EditorPatch
    {
        public class TextureSelected : Import
        {
            public readonly Texture2D texture;

            public TextureSelected(Texture2D texture)
            {
                this.texture = texture;
            }
        }

        public class Cancel : Import { }
    }

    public class VoxLoaded : EditorPatch
    {
        public readonly VoxData voxData;

        public VoxLoaded(VoxData voxData)
        {
            this.voxData = voxData;
        }
    }

    public interface VoxelsChanges : EditorPatch
    {
        public class Add : VoxelsChanges
        {
            public readonly Vector3Int voxel;

            public Add(Vector3Int voxel)
            {
                this.voxel = voxel;
            }
        }

        public class Delete : VoxelsChanges
        {
            public readonly Vector3Int voxel;

            public Delete(Vector3Int voxel)
            {
                this.voxel = voxel;
            }
        }
    }

    public interface Control : EditorPatch
    {
        public interface Drawing : Control
        {
            public class Start : Drawing {}
            public class Finish : Drawing {}
        }
        public interface Moving : Control
        {
            public class Start : Drawing {}
            public class Finish : Drawing {}
        }
        public interface Rotating : Control
        {
            public class Start : Drawing {}
            public class Finish : Drawing {}
        }
    }

    public interface Camera : EditorPatch
    {
        public class NewPivotPoint : Camera
        {
            public readonly Vector3 position;

            public NewPivotPoint(Vector3 position)
            {
                this.position = position;
            }
        }

        public class NewDistance : Camera
        {
            public readonly float distance;

            public NewDistance(float distance)
            {
                this.distance = distance;
            }
        }

        public class NewRotation : Camera
        {
            public readonly Quaternion rotation;

            public NewRotation(Quaternion rotation)
            {
                this.rotation = rotation;
            }
        }
    }

    public interface Brush : EditorPatch
    {
        public class ChangeType : Brush
        {
            public readonly BrushType brushType;

            public ChangeType(BrushType brushType)
            {
                this.brushType = brushType;
            }
        }
    }
}
}