using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.ActionDelegates;
using Main.Scripts.VoxelEditor.ActionDelegates.Input;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Brush;
using Main.Scripts.VoxelEditor.State.Vox;
using Main.Scripts.VoxelEditor.View;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor
{
    public class EditorFeature
    {
        internal EditorState state;
        private EditorView view;

        private LoadVoxActionDelegate loadVoxActionDelegate;
        private LayersActionDelegate layersActionDelegate;
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
            var layers = new Dictionary<int, VoxLayerState>();
            layers[1] = new VoxLayerState.Init();
            state = new EditorState(
                layers: layers,
                activeLayerKey: 1,
                bufferedSpriteData: null,
                brushData: new BrushData(BrushMode.One, BrushType.Add),
                shaderData: new ShaderData(true, false),
                isSpriteRefVisible: false,
                freeCameraData: new FreeCameraData(
                    pivotPoint: new Vector3(0, 0, 0),
                    distance: 30,
                    rotation: Quaternion.Euler(30, 0, 0)
                ),
                isometricCameraData: new IsometricCameraData(new Vector3(0, 35, -35)),
                cameraType: CameraType.Free,
                controlState: new ControlState.None(),
                editModeState: new EditModeState.EditMode(),
                uiState: UIState.Menu
            );
            this.view = view;
            
            var reducer = new EditorReducer(this);
            var repository = new EditorRepository();
            loadVoxActionDelegate = new LoadVoxActionDelegate(this, reducer, repository, eventsConsumer);
            layersActionDelegate = new LayersActionDelegate(this, reducer, eventsConsumer);
            loadTextureActionDelegate = new LoadTextureActionDelegate(this, reducer, repository, eventsConsumer);
            applyChangesActionDelegate = new ApplyChangesActionDelegate(this, reducer);
            saveVoxActionDelegate = new SaveVoxActionDelegate(this, reducer, repository, eventsConsumer);
            importActionDelegate = new ImportActionDelegate(this, reducer, repository, eventsConsumer);
            exportActionDelegate = new ExportActionDelegate(this, reducer, repository, eventsConsumer);
            editModeActionDelegate = new EditModeActionDelegate(this, reducer);
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
                case EditorAction.Layers layersAction:
                    layersActionDelegate.ApplyAction(state, layersAction);
                    break;
                case EditorAction.OnToggleCameraClicked onToggleCameraClickedAction:
                    toggleCameraActionDelegate.ApplyAction(state, onToggleCameraClickedAction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        internal void UpdateState(EditorState state)
        {
            this.state = state;
            
            view.ApplyState(state);
        }
    }
}