using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SpriteSettingsActionDelegate : ActionDelegate<EditorAction.TextureSettings>
{
    public SpriteSettingsActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.TextureSettings action)
    {
        if (state is not EditorState.SpriteSelecting spriteState) return;

        switch (action)
        {
            case EditorAction.TextureSettings.Selected selected:
                ApplySpriteSelected(spriteState, selected);
                break;
            case EditorAction.TextureSettings.Canceled canceled:
                reducer.ApplyPatch(new EditorPatch.Import.Cancel());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void ApplySpriteSelected(EditorState.SpriteSelecting state, EditorAction.TextureSettings.Selected action)
    {
        var textureData = action.textureData;
        var texture = state.texture;
        
        var width = texture.width / textureData.columnsCount;
        var height = texture.height / textureData.rowsCount;

        var sprites = new Dictionary<SpriteIndex, SpriteData>();

        for (var columnIndex = 0; columnIndex < textureData.columnsCount; columnIndex++)
        {
            for (var rowIndex = 0; rowIndex < textureData.rowsCount; rowIndex++)
            {
                var spriteIndex = new SpriteIndex(rowIndex, columnIndex);
                var spriteData = GenerateSprite(
                    spriteIndex: spriteIndex,
                    textureData: textureData,
                    texture: texture,
                    width: width,
                    height: height
                );
                sprites[spriteIndex] = spriteData;
            }
        }

        reducer.ApplyPatch(new EditorPatch.VoxLoaded(new VoxData(
            textureData: textureData,
            sprites: sprites
        )));
    }

    private SpriteData GenerateSprite(
        SpriteIndex spriteIndex,
        TextureData textureData,
        Texture2D texture,
        int width,
        int height
    )
    {
        var offsetX = width * spriteIndex.columnIndex;
        var offsetY = height * (textureData.rowsCount - spriteIndex.rowIndex - 1);

        var voxels = new HashSet<Vector3Int>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var pixel = texture.GetPixel(offsetX + x, offsetY + y);
                if (pixel.a > 0.5f)
                {
                    voxels.Add(new Vector3Int(x, y, 0));
                }
            }
        }

        return new SpriteData(
            pivot: Vector2.zero,
            voxels: voxels
        );
    }
}
}