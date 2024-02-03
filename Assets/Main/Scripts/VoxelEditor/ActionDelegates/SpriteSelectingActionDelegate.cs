using System;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SpriteSelectingActionDelegate : ActionDelegate<EditorAction.SpriteSelecting>
{
    public SpriteSelectingActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }

    public override void ApplyAction(EditorState state, EditorAction.SpriteSelecting action)
    {
        if (state is not EditorState.Loaded loadedState) return;

        var curIndex = loadedState.currentSpriteIndex;
        var textureData = loadedState.voxData.textureData;

        if (loadedState.currentSpriteData != loadedState.voxData.sprites[loadedState.currentSpriteIndex])
        {
            reducer.ApplyPatch(new EditorPatch.SpriteChanges.ApplyRequest());
            return;
        }

        var spriteIndex = action switch
        {
            EditorAction.SpriteSelecting.OnNextClicked onNextClicked => new SpriteIndex(rowIndex: (curIndex.rowIndex + (curIndex.columnIndex + 1) / textureData.columnsCount) % textureData.rowsCount, columnIndex: (curIndex.columnIndex + 1) % textureData.columnsCount),
            EditorAction.SpriteSelecting.OnPreviousClicked onPreviousClicked => new SpriteIndex(rowIndex: (curIndex.rowIndex + (curIndex.columnIndex - 1 + textureData.columnsCount) / textureData.columnsCount - 1 + textureData.rowsCount) % textureData.rowsCount, columnIndex: (curIndex.columnIndex - 1 + textureData.columnsCount) % textureData.columnsCount),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
        reducer.ApplyPatch(new EditorPatch.ChangeSpriteIndex(spriteIndex));
    }
}
}