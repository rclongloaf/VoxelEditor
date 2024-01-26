﻿using Main.Scripts.VoxelEditor.State.Vox;
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
        public record Selected(TextureData textureData) : TextureSettings;

        public record Canceled : TextureSettings;
    }

    public interface ApplyChanges : EditorAction
    {
        public record Apply : ApplyChanges;

        public record Discard : ApplyChanges;

        public record Cancel : ApplyChanges;
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

    public interface Brush : EditorAction
    {
        public record OnBrushModeOneClicked : Brush;

        public record OnBrushModeSectionClicked : Brush;
        
        public record OnBrushAddClicked : Brush;

        public record OnBrushDeleteClicked : Brush;
    }

    public interface Shader : EditorAction
    {
        public record OnToggleGridClicked : Shader;

        public record OnToggleTransparentClicked : Shader;
    }

    public record OnToggleCameraClicked : EditorAction;

    public interface Input : EditorAction
    {
        public record OnButtonDown : Input
        {
            public record Draw(bool withCtrl, bool withShift) : OnButtonDown;

            public record Rotate : OnButtonDown;

            public record MoveCamera : OnButtonDown;
        }

        public record OnButtonUp : Input
        {
            public record Draw : OnButtonUp;
            
            public record Rotate : OnButtonUp;

            public record MoveCamera : OnButtonUp;
        }
        
        public record OnMenu : Input;

        public record OnMouseDelta(float deltaX, float deltaY) : Input;

        public record OnWheelScroll(float delta) : Input;
    }
    
    public interface ActionsHistory : EditorAction
    {
        public record OnCancelClicked : ActionsHistory;

        public record OnRestoreClicked : ActionsHistory;
    }

    public record OnApplyPivotClicked(Vector2 pivotPoint) : EditorAction;
}
}