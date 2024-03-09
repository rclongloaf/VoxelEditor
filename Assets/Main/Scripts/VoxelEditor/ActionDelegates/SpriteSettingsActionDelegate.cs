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
        if (state.activeLayer is not VoxLayerState.SpriteSelecting activeLayer) return;

        switch (action)
        {
            case EditorAction.TextureSettings.Selected selected:
                ApplySpriteSelected(activeLayer, selected);
                break;
            case EditorAction.TextureSettings.Canceled canceled:
                reducer.ApplyPatch(new EditorPatch.Import.Cancel());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void ApplySpriteSelected(VoxLayerState.SpriteSelecting activeLayer, EditorAction.TextureSettings.Selected action)
    {
        var texture = activeLayer.texture;
        
        var width = texture.width / action.columnsCount;
        var height = texture.height / action.rowsCount;

        var sprites = new Dictionary<SpriteIndex, SpriteData>();

        var textureData = new TextureData(
            rowsCount: action.rowsCount,
            columnsCount: action.columnsCount,
            spriteWidth: texture.width / action.columnsCount,
            spriteHeight: texture.height / action.rowsCount
        );

        for (var columnIndex = 0; columnIndex < action.columnsCount; columnIndex++)
        {
            for (var rowIndex = 0; rowIndex < action.rowsCount; rowIndex++)
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

        var voxels = new Dictionary<Vector3Int, VoxelData>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var pixel = texture.GetPixel(offsetX + x, offsetY + y);
                if (pixel.a > 0.5f)
                {
                    var pos = new Vector3Int(x, y, 0);
                    voxels[pos] = new VoxelData(false);
                }
            }
        }

        return new SpriteData(
            pivot: new Vector2(width / 2, height / 2),
            voxels: voxels
        );
    }
}
}