using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class SmoothActionDelegate : ActionDelegate<EditorAction.Smooth>
{
    public SmoothActionDelegate(EditorFeature feature, EditorReducer reducer) : base(feature, reducer) { }
    
    public override void ApplyAction(EditorState state, EditorAction.Smooth action)
    {
        if (state.activeLayer is not VoxLayerState.Loaded loadedLayer) return;

        switch (action)
        {
            case EditorAction.Smooth.Auto auto:
                OnAutoSmooth(loadedLayer);
                break;
            case EditorAction.Smooth.Clear clear:
                OnClearSmooth(loadedLayer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnAutoSmooth(VoxLayerState.Loaded layer)
    {
        var enableSmoothMap = new Dictionary<Vector3Int, bool>();
        //todo generate smoothing from texture
        foreach (var (pos, voxelData) in layer.currentSpriteData.voxels)
        {
            if (!voxelData.isSmooth)
            {
                enableSmoothMap[pos] = true;
            }
        }
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(
            new EditAction.ChangeSmooth(enableSmoothMap)
        ));
        reducer.ApplyPatch(new EditorPatch.Smooth.ChangeMultiple(enableSmoothMap));
    }

    private void OnClearSmooth(VoxLayerState.Loaded layer)
    {
        var enableSmoothMap = new Dictionary<Vector3Int, bool>();
        foreach (var (pos, voxelData) in layer.currentSpriteData.voxels)
        {
            if (voxelData.isSmooth)
            {
                enableSmoothMap[pos] = false;
            }
        }
        
        reducer.ApplyPatch(new EditorPatch.ActionsHistory.NewAction(
            new EditAction.ChangeSmooth(enableSmoothMap)
        ));
        reducer.ApplyPatch(new EditorPatch.Smooth.ChangeMultiple(enableSmoothMap));
    }
}
}