using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor
{
public interface EditorAction
{
    public interface Import : EditorAction
    {
        public record OnImportClicked : Import;

        public record OnFileSelected(string path) : Import;

        public record OnCanceled : Import;
    }

    public interface SpriteSettings : EditorAction
    {
        public record Selected(SpriteRectData spriteRectData) : SpriteSettings;

        public record Canceled : SpriteSettings;
    }


    public interface Export : EditorAction
    {
        public record OnExportClicked : Export;

        public record OnPathSelected(string path) : Export;

        public record OnCanceled : Export;
    }

    public interface LoadVox : EditorAction
    {
        public record OnLoadClicked : LoadVox;

        public record OnFileSelected(string path) : LoadVox;

        public record OnCanceled : LoadVox;
    }

    public interface LoadTexture : EditorAction
    {
        public record OnLoadClicked : LoadTexture;

        public record OnPathSelected(string path) : LoadTexture;

        public record OnCancel : LoadTexture;
    }

    public interface SaveVox : EditorAction
    {
        public record OnSaveClicked : SaveVox;

        public record OnPathSelected(string path) : SaveVox;

        public record OnCanceled : SaveVox;
    }

    public interface EditMode : EditorAction
    {
        public record OnEditModeClicked : EditMode;

        public record OnRenderModeClicked : EditMode;
    }

    public interface Brush : EditorAction
    {
        public record OnBrushAddClicked : Brush;

        public record OnBrushDeleteClicked : Brush;
    }

    public interface Input : EditorAction
    {
        public record OnButtonDown(KeyCode keyCode) : Input;

        public record OnButtonUp(KeyCode keyCode) : Input;

        public record OnMouseDelta(float deltaX, float deltaY) : Input;

        public record OnWheelScroll(float delta) : Input;
    }
}
}