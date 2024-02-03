using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class LayersInfoUIHolder
{
    private UIDocument doc;
    
    private Dictionary<int,  LayerLayoutHolder> layerLayoutHolders = new();
    
    public LayersInfoUIHolder(UIDocument doc)
    {
        this.doc = doc;
        var root = doc.rootVisualElement;
        
        layerLayoutHolders[1] = new LayerLayoutHolder(root.Q<VisualElement>("LayerLayout1"));
        layerLayoutHolders[2] = new LayerLayoutHolder(root.Q<VisualElement>("LayerLayout2"));
        layerLayoutHolders[3] = new LayerLayoutHolder(root.Q<VisualElement>("LayerLayout3"));
        layerLayoutHolders[4] = new LayerLayoutHolder(root.Q<VisualElement>("LayerLayout4"));
        layerLayoutHolders[5] = new LayerLayoutHolder(root.Q<VisualElement>("LayerLayout5"));

        foreach (var (_, holder) in layerLayoutHolders)
        {
            holder.SetVisibility(false);
        }
    }

    public void Bind(EditorState state)
    {
        foreach (var (_, holder) in layerLayoutHolders)
        {
            holder.SetVisibility(false);
        }
        
        foreach (var (key, layer) in state.layers)
        {
            var holder = layerLayoutHolders[key];

            if (layer is VoxLayerState.Loaded loaded)
            {
                holder.Bind(
                    isActive: key == state.activeLayerKey,
                    isVisible: loaded.isVisible,
                    needSave: loaded.voxData.sprites[loaded.currentSpriteIndex] != loaded.currentSpriteData
                );
            }
            else
            {
                holder.Bind(
                    isActive: key == state.activeLayerKey,
                    isVisible: false,
                    needSave: false
                );
            }
            
            holder.SetVisibility(true);
        }
    }
}
}