using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Brush;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor
{
public interface EditorPatch
{
    public interface FileBrowser : EditorPatch
    {
        public record Opened : FileBrowser;
        public record Closed : FileBrowser;
    }

    public record MenuVisibility(bool visible) : EditorPatch;

    public interface Layers : EditorPatch
    {
        public record Create(int key) : Layers;

        public record Select(int key) : Layers;

        public record ChangeVisibility(int key) : Layers;
        
        public interface Delete : Layers
        {
            public record Request : Delete;

            public record Apply(int key) : Delete;

            public record Cancel : Delete;
        }
    }

    public interface Import : EditorPatch
    {
        public record TextureSelected(Texture2D texture) : Import;
        public record Cancel : Import;
    }

    public record VoxLoaded(VoxData voxData) : EditorPatch;

    public record TextureLoaded(Texture2D texture) : EditorPatch;
    
    public interface SpriteChanges : EditorPatch
    {
        public record ApplyRequest : SpriteChanges;
        
        public record Apply : SpriteChanges;

        public record Discard : SpriteChanges;

        public record Cancel : SpriteChanges;
    }

    public interface EditMode : EditorPatch
    {
        public record EditModeSelected : EditMode;
        public record RenderModeSelected : EditMode;
    }

    public interface VoxelsChanges : EditorPatch
    {
        public record Add(IEnumerable<Vector3Int> voxels) : VoxelsChanges;
        public record Delete(IEnumerable<Vector3Int> voxels) : VoxelsChanges;
    }
    
    public interface ModelBuffer : EditorPatch
    {
        public record Copy(SpriteData spriteData) : ModelBuffer;

        public record Paste(SpriteData spriteData) : ModelBuffer;
    }

    public interface Control : EditorPatch
    {
        public interface Drawing : Control
        {
            public record Start(
                Vector3Int position,
                Vector3Int normal,
                bool deleting,
                bool bySection,
                bool withProjection
            ) : Drawing;
            
            public record Finish : Drawing;
        }
        public interface Moving : Control
        {
            public record Start : Drawing;
            public record Finish : Drawing;
        }
        public interface Selection : Control
        {
            public record Start(
                Vector3 mousePos
            ) : Selection;

            public record Finish : Selection;
        }

        public interface SelectionMoving : Control
        {
            public record Start(Vector3Int normal, Vector3 fromPosition) : SelectionMoving;

            public record ChangeSelectionOffset(Vector3Int deltaOffset) : SelectionMoving;

            public record Finish : SelectionMoving;
        }
        
        public interface Rotating : Control
        {
            public record Start : Drawing;
            public record Finish : Drawing;
        }
    }
    
    public interface Selection : EditorPatch
    {
        public record CancelSelection : Selection;

        public record Select(
            IEnumerable<Vector3Int> voxels,
            Vector3Int offset
        ) : Selection;
    }
    public interface Camera : EditorPatch
    {
        public record NewPivotPoint(Vector3 position) : Camera;

        public record NewDistance(float distance) : Camera;

        public record NewRotation(Quaternion rotation) : Camera;
        
        public record ChangeType(CameraType cameraType) : Camera;
    }

    public interface Brush : EditorPatch
    {
        public record ChangeType(BrushType brushType) : Brush;

        public record ChangeMode(BrushMode brushMode) : Brush;
    }

    public record ChangeSpriteIndex(SpriteIndex spriteIndex) : EditorPatch;
    
    public interface Shader : EditorPatch
    {
        public record ChangeGridEnabled(bool enabled) : Shader;

        public record ChangeTransparentEnabled(bool enabled) : Shader;
    }

    public interface ActionsHistory : EditorPatch
    {
        public record CancelAction : ActionsHistory;

        public record RestoreAction : ActionsHistory;

        public record NewAction(EditAction action) : ActionsHistory;
    }

    public record NewPivotPoint(Vector2 pivotPoint) : EditorPatch;

    public record ChangeSpriteRefVisibility(bool visible) : EditorPatch;
}
}