﻿using Main.Scripts.VoxelEditor.ActionDelegates;
using Main.Scripts.VoxelEditor.ActionDelegates.Input;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.View;

namespace Main.Scripts.VoxelEditor
{
    public class EditorFeature
    {
        internal EditorState state = new EditorState.WaitingForProject();
        private EditorView view;

        private EditorReducer reducer;

        private LoadVoxActionDelegate loadVoxActionDelegate;
        private SaveVoxActionDelegate saveVoxActionDelegate;
        private ImportActionDelegate importActionDelegate;
        private ExportActionDelegate exportActionDelegate;
        private BrushActionDelegate brushActionDelegate;
        private InputActionDelegate inputActionDelegate;

        public EditorFeature(EditorView view, EditorEventsConsumer eventsConsumer)
        {
            this.view = view;
            
            reducer = new EditorReducer(this);
            var repository = new EditorRepository();
            loadVoxActionDelegate = new LoadVoxActionDelegate(this, reducer, repository, eventsConsumer);
            saveVoxActionDelegate = new SaveVoxActionDelegate(this, reducer, eventsConsumer);
            importActionDelegate = new ImportActionDelegate(this, reducer, eventsConsumer);
            exportActionDelegate = new ExportActionDelegate(this, reducer, eventsConsumer);
            brushActionDelegate = new BrushActionDelegate(this, reducer);
            inputActionDelegate = new InputActionDelegate(this, reducer);
        }

        public void ApplyAction(EditorAction action)
        {
            switch (action)
            {
                case EditorAction.LoadVox loadVoxAction:
                    loadVoxActionDelegate.ApplyAction(state, loadVoxAction);
                    break;
                case EditorAction.SaveVox saveVoxAction:
                    saveVoxActionDelegate.ApplyAction(state, saveVoxAction);
                    break;
                case EditorAction.Import importAction:
                    importActionDelegate.ApplyAction(state, importAction);
                    break;
                case EditorAction.Export exportAction:
                    exportActionDelegate.ApplyAction(state, exportAction);
                    break;
                case EditorAction.Brush brushAction:
                    brushActionDelegate.ApplyAction(state, brushAction);
                    break;
                case EditorAction.Input inputAction:
                    inputActionDelegate.ApplyAction(state, inputAction);
                    break;
            }
        }

        internal void UpdateState(EditorState state)
        {
            this.state = state;
            
            view.ApplyState(state);
        }
    }
}