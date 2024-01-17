using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SpriteSettingsActionDelegate : ActionDelegate<EditorAction.SpriteSettings>
{
    public SpriteSettingsActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.SpriteSettings action)
    {
        if (state is not EditorState.SpriteSelecting spriteState) return;

        switch (action)
        {
            case EditorAction.SpriteSettings.Selected selected:
                ApplySpriteSelected(spriteState, selected);
                break;
            case EditorAction.SpriteSettings.Canceled canceled:
                reducer.ApplyPatch(new EditorPatch.Import.Cancel());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void ApplySpriteSelected(EditorState.SpriteSelecting state, EditorAction.SpriteSettings.Selected action)
    {
        var spriteData = action.spriteRectData;
        var texture = state.texture;
        
        var width = texture.width / spriteData.columnsCount;
        var height = texture.height / spriteData.rowsCount;
        var offsetX = width * spriteData.columnIndex;
        var offsetY = height * (spriteData.rowsCount - spriteData.rowIndex - 1);

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
        
        reducer.ApplyPatch(new EditorPatch.VoxLoaded(new VoxData(voxels, spriteData)));
    }
}
}