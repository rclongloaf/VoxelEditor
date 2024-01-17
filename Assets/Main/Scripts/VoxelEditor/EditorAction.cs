using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor
{
public interface EditorAction
{
    public interface Import : EditorAction
    {
        public class OnImportClicked : Import { }

        public class OnFileSelected : Import
        {
            public readonly string path;

            public OnFileSelected(string path)
            {
                this.path = path;
            }
        }

        public class OnCanceled : Import { }
    }

    public interface SpriteSettings : EditorAction
    {
        public class Selected : SpriteSettings
        {
            public readonly SpriteRectData spriteRectData;

            public Selected(
                SpriteRectData spriteRectData
            )
            {
                this.spriteRectData = spriteRectData;
            }
        }

        public class Canceled : SpriteSettings { }
    }
    

    public interface Export : EditorAction
    {
        public class OnExportClicked : Export { }

        public class OnPathSelected : Export
        {
            public readonly string path;

            public OnPathSelected(string path)
            {
                this.path = path;
            }
        }

        public class OnCanceled : Export { }
    }

    public interface LoadVox : EditorAction
    {
        public class OnLoadClicked : LoadVox { }

        public class OnFileSelected : LoadVox
        {
            public readonly string path;

            public OnFileSelected(string path)
            {
                this.path = path;
            }
        }

        public class OnCanceled : LoadVox { }
    }

    public interface SaveVox : EditorAction
    {
        public class OnSaveClicked : SaveVox { }

        public class OnPathSelected : SaveVox
        {
            public readonly string path;

            public OnPathSelected(string path)
            {
                this.path = path;
            }
        }

        public class OnCanceled : SaveVox { }
    }
    
    public interface Brush : EditorAction
    {
        public class OnBrushAddClicked : Brush { }

        public class OnBrushDeleteClicked : Brush { }
    }
    
    public interface Input : EditorAction
    {
        public class OnButtonDown : Input
        {
            public readonly KeyCode keyCode;

            public OnButtonDown(KeyCode keyCode)
            {
                this.keyCode = keyCode;
            }
        }

        public class OnButtonUp : Input
        {
            public readonly KeyCode keyCode;

            public OnButtonUp(KeyCode keyCode)
            {
                this.keyCode = keyCode;
            }
        }

        public class OnMouseDelta : Input
        {
            public readonly float deltaX;
            public readonly float deltaY;

            public OnMouseDelta(float deltaX, float deltaY)
            {
                this.deltaX = deltaX;
                this.deltaY = deltaY;
            }
        }

        public class OnWheelScroll : Input
        {
            public readonly float delta;

            public OnWheelScroll(float delta)
            {
                this.delta = delta;
            }
        }
    }
}
}