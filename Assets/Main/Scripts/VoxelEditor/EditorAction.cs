using Main.Scripts.VoxelEditor.State.Vox;
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

    public interface TextureSettings : EditorAction
    {
        public record Selected(int rowsCount, int columnsCount) : TextureSettings;

        public record Canceled : TextureSettings;
    }

    public interface ApplyChanges : EditorAction
    {
        public record Apply() : ApplyChanges;

        public record Discard : ApplyChanges;

        public record Cancel : ApplyChanges;
    }

    public interface Export : EditorAction
    {
        public interface Single : Export
        {
            public record OnClicked : Single;

            public record OnPathSelected(string path) : Single;

            public record OnCanceled : Single;
        }
        
        public interface All : Export
        {
            public record OnClicked : All;

            public record OnPathSelected(string path) : All;

            public record OnCanceled : All;
        }
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
    
    public interface SpriteSelecting : EditorAction
    {
        public record OnNextClicked : SpriteSelecting;

        public record OnPreviousClicked : SpriteSelecting;
    }
    
    public interface ModelBuffer : EditorAction
    {
        public record OnCopyClicked : ModelBuffer;

        public record OnPasteClicked : ModelBuffer;
    }

    public interface EditMode : EditorAction
    {
        public record OnEditModeClicked : EditMode;

        public record OnRenderModeClicked : EditMode;
    }

    public interface Shader : EditorAction
    {
        public record OnToggleGridClicked : Shader;

        public record OnToggleTransparentClicked : Shader;
    }

    public record OnToggleCameraClicked : EditorAction;

    public interface Input : EditorAction
    {
        public interface OnButtonDown : Input
        {
            public record Draw(bool withDelete, bool withSection, bool withProjection) : OnButtonDown;

            public record Select : OnButtonDown;

            public record Delete : OnButtonDown;

            public record MoveSelection : OnButtonDown;

            public record Rotate : OnButtonDown;

            public record MoveCamera : OnButtonDown;
        }

        public interface OnButtonUp : Input
        {
            public record Draw : OnButtonUp;

            public record Select : OnButtonUp;

            public record MoveSelection : OnButtonUp;
            
            public record Rotate : OnButtonUp;

            public record MoveCamera : OnButtonUp;
        }

        public record OnButtonDraw : Input;

        public record OnToggleSpriteRef : Input;
        
        public record OnMenu : Input;

        public record UpdateMoveSelection : Input;

        public record OnMouseDelta(float deltaX, float deltaY) : Input;

        public record OnWheelScroll(float delta) : Input;
    }
    
    public interface Layers : EditorAction
    {
        public record OnSelected(int key) : Layers;

        public interface Delete : Layers
        {
            public record OnRequest(int key) : Delete;

            public record OnApply(int key) : Delete;

            public record OnCancel : Delete;
        }

        public record OnChangeVisibility(int key) : Layers;
    }
    
    public interface ActionsHistory : EditorAction
    {
        public record OnCancelClicked : ActionsHistory;

        public record OnRestoreClicked : ActionsHistory;
    }
    
    public interface PivotPoint : EditorAction
    {
        public record OnApplyPivotClicked(Vector2 pivotPoint) : PivotPoint;
       
        public record OnApplyPivotPointForAllSpritesClicked : PivotPoint;
    }
}
}