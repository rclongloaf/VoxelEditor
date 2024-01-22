using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor
{
public interface EditorPatch
{
    public interface FileBrowser : EditorPatch
    {
        public record Opened : FileBrowser;
        public record Closed : FileBrowser;
    }

    public interface Import : EditorPatch
    {
        public record TextureSelected(Texture2D texture) : Import;
        public record Cancel : Import;
    }

    public record VoxLoaded(VoxData voxData) : EditorPatch;

    public record TextureLoaded(Texture2D texture) : EditorPatch;

    public interface EditMode : EditorPatch
    {
        public record EditModeSelected : EditMode;
        public record RenderModeSelected(Mesh mesh) : EditMode;
    }

    public interface VoxelsChanges : EditorPatch
    {
        public record Add(Vector3Int voxel) : VoxelsChanges;
        public record Delete(Vector3Int voxel) : VoxelsChanges;
    }

    public interface Control : EditorPatch
    {
        public interface Drawing : Control
        {
            public record Start : Drawing;
            public record Finish : Drawing;
        }
        public interface Moving : Control
        {
            public record Start : Drawing;
            public record Finish : Drawing;
        }
        public interface Rotating : Control
        {
            public record Start : Drawing;
            public record Finish : Drawing;
        }
    }

    public interface Camera : EditorPatch
    {
        public record NewPivotPoint(Vector3 position) : Camera;

        public record NewDistance(float distance) : Camera;

        public record NewRotation(Quaternion rotation) : Camera;
    }

    public interface Brush : EditorPatch
    {
        public record ChangeType(BrushType brushType) : Brush;
    }
}
}