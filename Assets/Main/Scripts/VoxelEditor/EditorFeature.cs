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

        private LoadVoxActionDelegate loadVoxActionDelegate;
        private LoadTextureActionDelegate loadTextureActionDelegate;
        private ApplyChangesActionDelegate applyChangesActionDelegate;
        private SaveVoxActionDelegate saveVoxActionDelegate;
        private ImportActionDelegate importActionDelegate;
        private ExportActionDelegate exportActionDelegate;
        private EditModeActionDelegate editModeActionDelegate;
        private BrushActionDelegate brushActionDelegate;
        private InputActionDelegate inputActionDelegate;
        private SpriteSettingsActionDelegate spriteSettingsActionDelegate;
        private SpriteSelectingActionDelegate spriteSelectingActionDelegate;
        private ModelBufferActionDelegate modelBufferActionDelegate;
        private ToggleCameraActionDelegate toggleCameraActionDelegate;
        private ShaderActionDelegate shaderActionDelegate;
        private ActionsHistoryActionDelegate actionsHistoryActionDelegate;
        private ApplyPivotActionDelegate applyPivotActionDelegate;

        public EditorFeature(EditorView view, EditorEventsConsumer eventsConsumer)
        {
            this.view = view;
            
            var reducer = new EditorReducer(this);
            var repository = new EditorRepository();
            loadVoxActionDelegate = new LoadVoxActionDelegate(this, reducer, repository, eventsConsumer);
            loadTextureActionDelegate = new LoadTextureActionDelegate(this, reducer, repository, eventsConsumer);
            applyChangesActionDelegate = new ApplyChangesActionDelegate(this, reducer);
            saveVoxActionDelegate = new SaveVoxActionDelegate(this, reducer, repository, eventsConsumer);
            importActionDelegate = new ImportActionDelegate(this, reducer, repository, eventsConsumer);
            exportActionDelegate = new ExportActionDelegate(this, reducer, eventsConsumer);
            editModeActionDelegate = new EditModeActionDelegate(this, reducer, repository);
            brushActionDelegate = new BrushActionDelegate(this, reducer);
            inputActionDelegate = new InputActionDelegate(this, reducer);
            spriteSettingsActionDelegate = new SpriteSettingsActionDelegate(this, reducer);
            spriteSelectingActionDelegate = new SpriteSelectingActionDelegate(this, reducer);
            modelBufferActionDelegate = new ModelBufferActionDelegate(this, reducer);
            toggleCameraActionDelegate = new ToggleCameraActionDelegate(this, reducer);
            shaderActionDelegate = new ShaderActionDelegate(this, reducer);
            actionsHistoryActionDelegate = new ActionsHistoryActionDelegate(this, reducer);
            applyPivotActionDelegate = new ApplyPivotActionDelegate(this, reducer);
        }

        public void ApplyAction(EditorAction action)
        {
            switch (action)
            {
                case EditorAction.ActionsHistory actionsHistory:
                    actionsHistoryActionDelegate.ApplyAction(state, actionsHistory);
                    break;
                case EditorAction.LoadVox loadVoxAction:
                    loadVoxActionDelegate.ApplyAction(state, loadVoxAction);
                    break;
                case EditorAction.LoadTexture loadTextureAction:
                    loadTextureActionDelegate.ApplyAction(state, loadTextureAction);
                    break;
                case EditorAction.ApplyChanges applyChangesAction:
                    applyChangesActionDelegate.ApplyAction(state, applyChangesAction);
                    break;
                case EditorAction.SaveVox saveVoxAction:
                    saveVoxActionDelegate.ApplyAction(state, saveVoxAction);
                    break;
                case EditorAction.Shader shaderAction:
                    shaderActionDelegate.ApplyAction(state, shaderAction);
                    break;
                case EditorAction.SpriteSelecting spriteSelecting:
                    spriteSelectingActionDelegate.ApplyAction(state, spriteSelecting);
                    break;
                case EditorAction.ModelBuffer modelBufferAction:
                    modelBufferActionDelegate.ApplyAction(state, modelBufferAction);
                    break;
                case EditorAction.OnApplyPivotClicked onApplyPivotClicked:
                    applyPivotActionDelegate.ApplyAction(state, onApplyPivotClicked);
                    break;
                case EditorAction.TextureSettings spriteSettings:
                    spriteSettingsActionDelegate.ApplyAction(state, spriteSettings);
                    break;
                case EditorAction.Import importAction:
                    importActionDelegate.ApplyAction(state, importAction);
                    break;
                case EditorAction.Export exportAction:
                    exportActionDelegate.ApplyAction(state, exportAction);
                    break;
                case EditorAction.EditMode editModeAction:
                    editModeActionDelegate.ApplyAction(state, editModeAction);
                    break;
                case EditorAction.Brush brushAction:
                    brushActionDelegate.ApplyAction(state, brushAction);
                    break;
                case EditorAction.Input inputAction:
                    inputActionDelegate.ApplyAction(state, inputAction);
                    break;
                case EditorAction.OnToggleCameraClicked onToggleCameraClickedAction:
                    toggleCameraActionDelegate.ApplyAction(state, onToggleCameraClickedAction);
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