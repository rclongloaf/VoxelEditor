using System;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;

namespace Main.Scripts.VoxelEditor.ActionDelegates
{
public class LayersActionDelegate : ActionDelegate<EditorAction.Layers>
{
    private EditorEventsConsumer eventsConsumer;

    public LayersActionDelegate(
        EditorFeature feature,
        EditorReducer reducer,
        EditorEventsConsumer eventsConsumer
    ) : base(feature, reducer)
    {
        this.eventsConsumer = eventsConsumer;
    }
    
    public override void ApplyAction(EditorState state, EditorAction.Layers action)
    {
        switch (action)
        {
            case EditorAction.Layers.OnChangeVisibility onChangeVisibility:
                OnChangeVisibility(state, onChangeVisibility);
                break;
            case EditorAction.Layers.Delete delete:
                OnDelete(state, delete);
                break;
            case EditorAction.Layers.OnSelected onSelected:
                OnSelected(state, onSelected);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void OnSelected(EditorState state, EditorAction.Layers.OnSelected action)
    {
        if (state.layers.ContainsKey(action.key))
        {
            reducer.ApplyPatch(new EditorPatch.Layers.Select(action.key));
        }
        else
        {
            reducer.ApplyPatch(new EditorPatch.Layers.Create(action.key));
        }
    }

    private void OnChangeVisibility(EditorState state, EditorAction.Layers.OnChangeVisibility action)
    {
        if (!state.layers.ContainsKey(action.key)) return;
        
        reducer.ApplyPatch(new EditorPatch.Layers.ChangeVisibility(action.key));
    }

    private void OnDelete(EditorState state, EditorAction.Layers.Delete action)
    {
        switch (action)
        {
            case EditorAction.Layers.Delete.OnApply onApply:
                if (!state.layers.ContainsKey(onApply.key)) return;
                
                reducer.ApplyPatch(new EditorPatch.Layers.Delete.Apply(onApply.key));
                break;
            case EditorAction.Layers.Delete.OnCancel onCancel:
                reducer.ApplyPatch(new EditorPatch.Layers.Delete.Cancel());
                break;
            case EditorAction.Layers.Delete.OnRequest onRequest:
                if (state.activeLayerKey == onRequest.key
                    || !state.layers.ContainsKey(onRequest.key)) return;
                
                reducer.ApplyPatch(new EditorPatch.Layers.Delete.Request());
                eventsConsumer.Consume(new EditorEvent.DeleteLayerRequest(onRequest.key));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
}