using System;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class EditModeActionDelegate : ActionDelegate<EditorAction.EditMode>
{
    private EditorRepository repository;
    
    public EditModeActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorRepository repository
    ) : base(feature, reducer)
    {
        this.repository = repository;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.EditMode action)
    {
        if (state is not EditorState.Loaded loadedSate) return;

        switch (action)
        {
            case EditorAction.EditMode.OnEditModeClicked onEditModeClicked:
                reducer.ApplyPatch(new EditorPatch.EditMode.EditModeSelected());
                break;
            case EditorAction.EditMode.OnRenderModeClicked onRenderModeClicked:
                var mesh = repository.GenerateMesh(
                    loadedSate.voxels,
                    Vector2.zero,
                    1,
                    loadedSate.spriteRectData,
                    loadedSate.texture?.width ?? 1,
                    loadedSate.texture?.height ?? 1
                );
                reducer.ApplyPatch(new EditorPatch.EditMode.RenderModeSelected(mesh));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}