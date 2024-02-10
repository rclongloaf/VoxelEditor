using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.EditModes;
using Main.Scripts.VoxelEditor.EditModes.Render;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor.View
{
public class EditorView : MonoBehaviour,
    EditorEventsConsumer,
    EditorUIHolder.Listener,
    TextureImportUIHolder.Listener,
    ApplySpriteChangesUIHolder.Listener,
    ApplyDeleteLayerUIHolder.Listener
{
    [SerializeField]
    private Transform freeCameraTransform = null!;
    [SerializeField]
    private Transform isometricCameraTransform = null!;
    [SerializeField]
    private UIDocument editorUIDocument = null!;
    [SerializeField]
    private UIDocument spriteImportUIDocument = null!;
    [SerializeField]
    private UIDocument applyChangesUIDocument = null!;
    [SerializeField]
    private UIDocument applyDeleteLayerUIDocument = null!;
    [SerializeField]
    private UIDocument layersInfoUIDocument = null!;
    [SerializeField]
    private GameObject editModeRoot = null!;
    [SerializeField]
    private SpriteRenderer spriteReference = null!;
    [SerializeField]
    private GameObject pivotPointObject = null!;
    [SerializeField]
    private GameObject voxelPrefab = null!;
    [SerializeField]
    private Material voxelMaterial = null!;
    [SerializeField]
    private GameObject renderModeRoot = null!;
    [SerializeField]
    private GameObject renderModelPrefab = null!;
    [SerializeField]
    private Material renderModelMaterial = null!;
    
    private EditorFeature feature = null!;
    private EditorState? currentState;
    private Dictionary<int, EditModeController> editModeControllers = new();
    private RenderModeController renderModeController = null!;
    private EditorUIHolder editorUIHolder = null!;
    private TextureImportUIHolder textureImportUIHolder = null!;
    private ApplySpriteChangesUIHolder applySpriteChangesUIHolder = null!;
    private ApplyDeleteLayerUIHolder applyDeleteLayerUIHolder = null!;
    private LayersInfoUIHolder layersInfoUIHolder = null!;
    
    private HashSet<KeyCode> pressedKeys = new();
    private HashSet<KeyCode> newPressedKeys = new();
    
    private void Awake()
    {
        feature = new EditorFeature(this, this);
        editorUIHolder = new EditorUIHolder(editorUIDocument, this);
        editorUIHolder.SetLoadedState(false);
        textureImportUIHolder = new TextureImportUIHolder(spriteImportUIDocument, this);
        textureImportUIHolder.SetVisibility(false);
        applySpriteChangesUIHolder = new ApplySpriteChangesUIHolder(applyChangesUIDocument, this);
        applySpriteChangesUIHolder.SetVisibility(false);
        applyDeleteLayerUIHolder = new ApplyDeleteLayerUIHolder(applyDeleteLayerUIDocument, this);
        applyDeleteLayerUIHolder.SetVisibility(false);
        layersInfoUIHolder = new LayersInfoUIHolder(layersInfoUIDocument);
        renderModeController = new RenderModeController(
            renderModeRoot, 
            renderModelPrefab, 
            renderModelMaterial
        );
        
        ApplyState(feature.state);
    }

    private void Start()
    {
        renderModeController.SetVisibility(false);
    }

    private void Update()
    {
        newPressedKeys.Clear();

        UpdateKeyPressedStatus(KeyCode.Escape);
        UpdateKeyPressedStatus(KeyCode.Mouse0);
        UpdateKeyPressedStatus(KeyCode.Mouse1);
        UpdateKeyPressedStatus(KeyCode.Mouse2);
        UpdateKeyPressedStatus(KeyCode.LeftArrow);
        UpdateKeyPressedStatus(KeyCode.RightArrow);
        UpdateKeyPressedStatus(KeyCode.Tab);
        UpdateKeyPressedStatus(KeyCode.R);
        UpdateKeyPressedStatus(KeyCode.C);
        UpdateKeyPressedStatus(KeyCode.V);
        UpdateKeyPressedStatus(KeyCode.Z);
        UpdateKeyPressedStatus(KeyCode.Y);
        UpdateKeyPressedStatus(KeyCode.G);
        UpdateKeyPressedStatus(KeyCode.T);
        UpdateKeyPressedStatus(KeyCode.Q);
        UpdateKeyPressedStatus(KeyCode.I);
        UpdateKeyPressedStatus(KeyCode.J);
        UpdateKeyPressedStatus(KeyCode.K);
        UpdateKeyPressedStatus(KeyCode.L);
        UpdateKeyPressedStatus(KeyCode.Delete);
        UpdateKeyPressedStatus(KeyCode.Alpha1);
        UpdateKeyPressedStatus(KeyCode.Alpha2);
        UpdateKeyPressedStatus(KeyCode.Alpha3);
        UpdateKeyPressedStatus(KeyCode.Alpha4);
        UpdateKeyPressedStatus(KeyCode.Alpha5);

        var withCtrl = Input.GetKey(KeyCode.LeftControl);
        var withShift = Input.GetKey(KeyCode.LeftShift);
        var withX = Input.GetKey(KeyCode.X);
        var withSelection = Input.GetKey(KeyCode.A);

        var editorUIHolderListener = (EditorUIHolder.Listener)this;

        if (newPressedKeys.Contains(KeyCode.Escape) && !pressedKeys.Contains(KeyCode.Escape))
        {
            feature.ApplyAction(new EditorAction.Input.OnMenu());
            (pressedKeys, newPressedKeys) = (newPressedKeys, pressedKeys);
            return;
        }

        if (currentState is not { uiState: UIState.None })
        {
            (pressedKeys, newPressedKeys) = (newPressedKeys, pressedKeys);
            return;
        }

        var activeLayer = currentState.activeLayer as VoxLayerState.Loaded;
        var layerActionKey = 0;

        foreach (var key in newPressedKeys)
        {
            if (!pressedKeys.Contains(key))
            {
                switch (key)
                {
                    case KeyCode.Mouse0 when activeLayer?.selectionState is SelectionState.Selected:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.MoveSelection());
                        break;
                    case KeyCode.Mouse0 when activeLayer != null && withSelection:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Select());
                        break;
                    case KeyCode.Mouse0 when activeLayer != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Draw(withCtrl, withShift, withX));
                        break;
                    case KeyCode.Mouse1:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Rotate());
                        break;
                    case KeyCode.Mouse2:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.MoveCamera());
                        break;
                    case KeyCode.LeftArrow when activeLayer != null:
                        editorUIHolderListener.OnPreviousSpriteClicked();
                        break;
                    case KeyCode.RightArrow when activeLayer != null:
                        editorUIHolderListener.OnNextSpriteClicked();
                        break;
                    case KeyCode.Tab:
                        editorUIHolderListener.OnToggleCameraClicked();
                        break;
                    case KeyCode.R:
                        if (currentState.editModeState is EditModeState.EditMode)
                        {
                            editorUIHolderListener.OnRenderModeClicked();
                        }
                        else
                        {
                            editorUIHolderListener.OnEditModeClicked();
                        }
                        break;
                    case KeyCode.C when activeLayer != null && withCtrl:
                        editorUIHolderListener.OnCopyModelClicked();
                        break;
                    case KeyCode.V when activeLayer != null && withCtrl:
                        editorUIHolderListener.OnPasteModelClicked();
                        break;
                    case KeyCode.Z when activeLayer != null && withCtrl:
                        editorUIHolderListener.OnCancelActionClicked();
                        break;
                    case KeyCode.Z when activeLayer != null && withCtrl && withShift:
                    case KeyCode.Y when activeLayer != null && withCtrl:
                        editorUIHolderListener.OnRestoreActionClicked();
                        break;
                    case KeyCode.G:
                        editorUIHolderListener.OnToggleGridClicked();
                        break;
                    case KeyCode.T:
                        editorUIHolderListener.OnToggleTransparentClicked();
                        break;
                    case KeyCode.Q when activeLayer != null:
                        editorUIHolderListener.OnToggleSpriteRefClicked();
                        break;
                    case KeyCode.I when activeLayer != null:
                        editorUIHolderListener.OnApplyPivotClicked(activeLayer.currentSpriteData.pivot + Vector2.down);
                        break;
                    case KeyCode.J when activeLayer != null:
                        editorUIHolderListener.OnApplyPivotClicked(activeLayer.currentSpriteData.pivot + Vector2.right);
                        break;
                    case KeyCode.K when activeLayer != null:
                        editorUIHolderListener.OnApplyPivotClicked(activeLayer.currentSpriteData.pivot + Vector2.up);
                        break;
                    case KeyCode.L when activeLayer != null:
                        editorUIHolderListener.OnApplyPivotClicked(activeLayer.currentSpriteData.pivot + Vector2.left);
                        break;
                    case KeyCode.Delete:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Delete());
                        break;
                    case KeyCode.Alpha1:
                        layerActionKey = 1;
                        break;
                    case KeyCode.Alpha2:
                        layerActionKey = 2;
                        break;
                    case KeyCode.Alpha3:
                        layerActionKey = 3;
                        break;
                    case KeyCode.Alpha4:
                        layerActionKey = 4;
                        break;
                    case KeyCode.Alpha5:
                        layerActionKey = 5;
                        break;
                }
            }
        }

        if (layerActionKey != 0)
        {
            if (withShift)
            {
                feature.ApplyAction(new EditorAction.Layers.OnChangeVisibility(layerActionKey));
            }
            else if (withCtrl)
            {
                feature.ApplyAction(new EditorAction.Layers.Delete.OnRequest(layerActionKey));
            }
            else
            {
                feature.ApplyAction(new EditorAction.Layers.OnSelected(layerActionKey));
            }
        }

        foreach (var key in pressedKeys)
        {
            if (!newPressedKeys.Contains(key))
            {
                switch (key)
                {
                    case KeyCode.Mouse0 when activeLayer?.selectionState is SelectionState.Selected:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.MoveSelection());
                        break;
                    case KeyCode.Mouse0 when activeLayer != null && currentState.controlState is ControlState.Selection:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.Select());
                        break;
                    case KeyCode.Mouse0 when activeLayer != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.Draw());
                        break;
                    case KeyCode.Mouse1:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.Rotate());
                        break;
                    case KeyCode.Mouse2:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.MoveCamera());
                        break;
                }
            }
        }

        switch (currentState.controlState)
        {
            case ControlState.None:
                var wheelDelta = Input.GetAxis("Mouse ScrollWheel");
                if (Math.Abs(wheelDelta) > 0)
                {
                    feature.ApplyAction(new EditorAction.Input.OnWheelScroll(wheelDelta));
                }
                break;
            case ControlState.Drawing:
                if (activeLayer != null && Input.GetKey(KeyCode.Mouse0))
                {
                    feature.ApplyAction(new EditorAction.Input.OnButtonDraw());
                }
                break;
            case ControlState.CameraMoving:
            case ControlState.Rotating:
                feature.ApplyAction(new EditorAction.Input.OnMouseDelta(
                    deltaX: Input.GetAxis("Mouse X"),
                    deltaY: Input.GetAxis("Mouse Y")
                ));
                break;
            case ControlState.Selection:
                break;
            case ControlState.SelectionMoving:
                feature.ApplyAction(new EditorAction.Input.UpdateMoveSelection());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        (pressedKeys, newPressedKeys) = (newPressedKeys, pressedKeys);
    }

    private void UpdateKeyPressedStatus(KeyCode keyCode)
    {
        if (Input.GetKey(keyCode))
        {
            newPressedKeys.Add(keyCode);
        }
    }

    void EditorEventsConsumer.Consume(EditorEvent editorEvent)
    {
        switch (editorEvent)
        {
            case EditorEvent.DeleteLayerRequest deleteLayerRequest:
                OnDeleteLayerRequest(deleteLayerRequest.key);
                break;
            case EditorEvent.OpenBrowserForExport openBrowserForExport:
                ExportMesh();
                break;
            case EditorEvent.OpenBrowserForImport openBrowserForImport:
                ImportTexture();
                break;
            case EditorEvent.OpenBrowserForLoadVox openBrowserForLoadVox:
                LoadVoxFile();
                break;
            case EditorEvent.OpenBrowserForLoadTexture openBrowserForLoadTexture:
                LoadTexture();
                break;
            case EditorEvent.OpenBrowserForSaveVox openBrowserForSaveVox:
                SaveVoxFile();
                break;
        }
    }

    void TextureImportUIHolder.Listener.OnApplyImportSettings(int rowsCount, int columnsCount)
    {
        feature.ApplyAction(new EditorAction.TextureSettings.Selected(rowsCount, columnsCount));
    }

    void TextureImportUIHolder.Listener.OnCancel()
    {
        feature.ApplyAction(new EditorAction.TextureSettings.Canceled());
    }

    void ApplyDeleteLayerUIHolder.Listener.OnApplyClicked(int layerKey)
    {
        feature.ApplyAction(new EditorAction.Layers.Delete.OnApply(layerKey));
    }

    void ApplyDeleteLayerUIHolder.Listener.OnCancelClicked()
    {
        feature.ApplyAction(new EditorAction.Layers.Delete.OnCancel());
    }

    void ApplySpriteChangesUIHolder.Listener.OnApplyClicked()
    {
        feature.ApplyAction(new EditorAction.ApplyChanges.Apply());
    }

    void ApplySpriteChangesUIHolder.Listener.OnDiscardClicked()
    {
        feature.ApplyAction(new EditorAction.ApplyChanges.Discard());
    }

    void ApplySpriteChangesUIHolder.Listener.OnCancelClicked()
    {
        feature.ApplyAction(new EditorAction.ApplyChanges.Cancel());
    }

    void EditorUIHolder.Listener.OnLoadVoxClicked()
    {
        feature.ApplyAction(new EditorAction.LoadVox.OnLoadClicked());
    }

    void EditorUIHolder.Listener.OnLoadTextureClicked()
    {
        feature.ApplyAction(new EditorAction.LoadTexture.OnLoadClicked());
    }

    void EditorUIHolder.Listener.OnSaveVoxClicked()
    {
        feature.ApplyAction(new EditorAction.SaveVox.OnSaveClicked());
    }

    void EditorUIHolder.Listener.OnImportClicked()
    {
        feature.ApplyAction(new EditorAction.Import.OnImportClicked());
    }

    void EditorUIHolder.Listener.OnExportClicked()
    {
        feature.ApplyAction(new EditorAction.Export.OnExportClicked());
    }

    void EditorUIHolder.Listener.OnEditModeClicked()
    {
        feature.ApplyAction(new EditorAction.EditMode.OnEditModeClicked());
    }

    void EditorUIHolder.Listener.OnRenderModeClicked()
    {
        feature.ApplyAction(new EditorAction.EditMode.OnRenderModeClicked());
    }

    void EditorUIHolder.Listener.OnBrushAddClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushAddClicked());
    }

    void EditorUIHolder.Listener.OnBrushDeleteClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushDeleteClicked());
    }

    void EditorUIHolder.Listener.OnPreviousSpriteClicked()
    {
        feature.ApplyAction(new EditorAction.SpriteSelecting.OnPreviousClicked());
    }

    void EditorUIHolder.Listener.OnNextSpriteClicked()
    {
        feature.ApplyAction(new EditorAction.SpriteSelecting.OnNextClicked());
    }

    void EditorUIHolder.Listener.OnCopyModelClicked()
    {
        feature.ApplyAction(new EditorAction.ModelBuffer.OnCopyClicked());
    }

    void EditorUIHolder.Listener.OnPasteModelClicked()
    {
        feature.ApplyAction(new EditorAction.ModelBuffer.OnPasteClicked());
    }

    void EditorUIHolder.Listener.OnToggleCameraClicked()
    {
        feature.ApplyAction(new EditorAction.OnToggleCameraClicked());
    }

    void EditorUIHolder.Listener.OnToggleGridClicked()
    {
        feature.ApplyAction(new EditorAction.Shader.OnToggleGridClicked());
    }

    void EditorUIHolder.Listener.OnToggleTransparentClicked()
    {
        feature.ApplyAction(new EditorAction.Shader.OnToggleTransparentClicked());
    }

    void EditorUIHolder.Listener.OnCancelActionClicked()
    {
        feature.ApplyAction(new EditorAction.ActionsHistory.OnCancelClicked());
    }

    void EditorUIHolder.Listener.OnRestoreActionClicked()
    {
        feature.ApplyAction(new EditorAction.ActionsHistory.OnRestoreClicked());
    }

    void EditorUIHolder.Listener.OnApplyPivotClicked(Vector2 pivotPoint)
    {
        feature.ApplyAction(new EditorAction.OnApplyPivotClicked(pivotPoint));
    }

    void EditorUIHolder.Listener.OnBrushModeOneClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushModeOneClicked());
    }

    void EditorUIHolder.Listener.OnBrushModeSectionClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushModeSectionClicked());
    }

    public void OnToggleSpriteRefClicked()
    {
        feature.ApplyAction(new EditorAction.Input.OnToggleSpriteRef());
    }

    public void ApplyState(EditorState state)
    {
        if (currentState == state) return;
        
        textureImportUIHolder.SetVisibility(state.uiState is UIState.TextureImport);
        applySpriteChangesUIHolder.SetVisibility(state.uiState is UIState.ApplySpriteChanges);
        applyDeleteLayerUIHolder.SetVisibility(state.uiState is UIState.ApplyLayerDelete);
        layersInfoUIHolder.Bind(state);
        
        foreach (var (key, _) in state.layers)
        {
            if (currentState == null || !currentState.layers.ContainsKey(key))
            {
                var layerRoot = Instantiate(new GameObject(), editModeRoot.transform);
                var editModeController = new EditModeController(layerRoot, spriteReference, voxelPrefab, voxelMaterial);
                editModeController.SetActive(true);
                editModeController.ApplyShaderData(state.shaderData);
                editModeControllers.Add(key, editModeController);
            }
        }

        if (currentState != null)
        {
            foreach (var (key, _) in currentState.layers)
            {
                if (!state.layers.ContainsKey(key))
                {
                    editModeControllers[key].Release();
                    editModeControllers.Remove(key);
                }
            }
        }

        switch (state.editModeState)
        {
            case EditModeState.EditMode editMode:
                renderModeController.SetVisibility(false);
                foreach (var (key, controller) in editModeControllers)
                {
                    controller.SetVisibility(state.layers[key] is VoxLayerState.Loaded loadedLayer && loadedLayer.isVisible);
                }
                break;
            case EditModeState.RenderMode renderMode:
                foreach (var (_, controller) in editModeControllers)
                {
                    controller.SetVisibility(false);
                }

                renderModeController.ApplyLayersData(state.layers);
                renderModeController.SetVisibility(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (currentState == null || currentState.activeLayerKey != state.activeLayerKey)
        {
            if (currentState != null)
            {
                editModeControllers[currentState.activeLayerKey].SetActive(false);
            }
            
            editModeControllers[state.activeLayerKey].SetActive(true);
        }
        
        if (currentState == null || currentState.shaderData != state.shaderData)
        {
            foreach (var (_, controller) in editModeControllers)
            {
                controller.ApplyShaderData(state.shaderData);
            }
            pivotPointObject.SetActive(state.shaderData.isGridEnabled);
        }

        var activeLayer = state.activeLayer;
        
        editorUIHolder.SetLoadedState(activeLayer is VoxLayerState.Loaded);
        editorUIHolder.SetVisibility(state.uiState is UIState.Menu);
        spriteReference.gameObject.SetActive(false);

        switch (activeLayer)
        {
            case VoxLayerState.Loaded loaded:
                ApplyLoadedActiveLayer(state, loaded);
                break;
            case VoxLayerState.SpriteSelecting spriteSelecting:
                ApplySpriteSelectingState(spriteSelecting);
                break;
            case VoxLayerState.Init waitingForProject:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(activeLayer));
        }
        
        freeCameraTransform.position = state.freeCameraData.pivotPoint
                                       + state.freeCameraData.rotation * new Vector3(0, 0, -state.freeCameraData.distance);
        freeCameraTransform.rotation = state.freeCameraData.rotation;
        freeCameraTransform.gameObject.SetActive(state.cameraType is CameraType.Free);
        
        isometricCameraTransform.position = state.isometricCameraData.position;
        isometricCameraTransform.gameObject.SetActive(state.cameraType is CameraType.Isometric);
        
        currentState = state;
    }

    private void ApplyLoadedActiveLayer(EditorState state, VoxLayerState.Loaded activeLayer)
    {
        if (currentState == null) return;

        var activeLayerKey = state.activeLayerKey;
        var curActiveLayer = currentState.activeLayer as VoxLayerState.Loaded;
        
        editModeControllers[activeLayerKey].ApplyVoxels(activeLayer.currentSpriteData.voxels);
        editModeControllers[activeLayerKey].SetReferenceVisibility(state.isSpriteRefVisible && activeLayer.isVisible);

        if (curActiveLayer == null || curActiveLayer.currentSpriteIndex != activeLayer.currentSpriteIndex)
        {
            editorUIHolder.SetSpriteIndex(activeLayer.currentSpriteIndex);
        }

        if (curActiveLayer == null || curActiveLayer.selectionState != activeLayer.selectionState)
        {
            editModeControllers[activeLayerKey].ApplySelection(activeLayer.selectionState);
        }
        
        if (curActiveLayer == null || curActiveLayer.texture != activeLayer.texture)
        {
            editModeControllers[activeLayerKey].ApplyTexture(activeLayer.texture);
        }
        
        if (curActiveLayer == null || curActiveLayer.currentSpriteIndex != activeLayer.currentSpriteIndex)
        {
            editModeControllers[activeLayerKey].ApplySpriteRect(activeLayer.voxData.textureData, activeLayer.currentSpriteIndex);
        }

        if (curActiveLayer == null || curActiveLayer.currentSpriteData.pivot != activeLayer.currentSpriteData.pivot)
        {
            var pivotPoint = activeLayer.currentSpriteData.pivot;
            editorUIHolder.SetPivotPoint(pivotPoint);
            editModeControllers[activeLayerKey].ApplyPivotPoint(pivotPoint);
        }
    }

    private void ApplySpriteSelectingState(VoxLayerState.SpriteSelecting state)
    {
        editorUIHolder.SetVisibility(false);
        textureImportUIHolder.SetVisibility(true);
        applySpriteChangesUIHolder.SetVisibility(false);
    }

    private void OnDeleteLayerRequest(int key)
    {
        applyDeleteLayerUIHolder.Bind(key);
        applyDeleteLayerUIHolder.SetVisibility(true);
    }

    private void ImportTexture()
    {
        FileBrowser.ShowLoadDialog(
            onSuccess: OnImportTextureSuccess,
            onCancel: OnImportTextureCancel,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Import",
            loadButtonText: "Select"
        );
    }

    private void OnImportTextureSuccess(string[] paths)
    {
        feature.ApplyAction(new EditorAction.Import.OnFileSelected(paths[0]));
    }

    private void OnImportTextureCancel()
    {
        feature.ApplyAction(new EditorAction.Import.OnCanceled());
    }

    private void ExportMesh()
    {
        FileBrowser.ShowSaveDialog(
            onSuccess: OnExportMeshSuccess,
            onCancel: OnExportMeshCancel,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Export mesh"
        );
    }

    private void OnExportMeshSuccess(string[] paths)
    {
        feature.ApplyAction(new EditorAction.Export.OnPathSelected(paths[0]));
    }

    private void OnExportMeshCancel()
    {
        feature.ApplyAction(new EditorAction.Export.OnCanceled());
    }
    
    private void LoadVoxFile()
    {
        FileBrowser.ShowLoadDialog(
            onSuccess: OnLoadVoxSuccess,
            onCancel: OnLoadVoxCancel,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Load",
            loadButtonText: "Select"
        );
    }

    private void OnLoadVoxSuccess(string[] paths)
    {
        feature.ApplyAction(new EditorAction.LoadVox.OnFileSelected(paths[0]));
    }

    private void OnLoadVoxCancel()
    {
        feature.ApplyAction(new EditorAction.LoadVox.OnCanceled());
    }

    private void LoadTexture()
    {
        FileBrowser.ShowLoadDialog(
            onSuccess: OnLoadTextureSuccess,
            onCancel: OnLoadTextureCancel,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Load",
            loadButtonText: "Select"
        );
    }

    private void OnLoadTextureSuccess(string[] paths)
    {
        feature.ApplyAction(new EditorAction.LoadTexture.OnPathSelected(paths[0]));
    }

    private void OnLoadTextureCancel()
    {
        feature.ApplyAction(new EditorAction.LoadTexture.OnCancel());
    }

    private void SaveVoxFile()
    {
        FileBrowser.ShowSaveDialog(
            onSuccess: OnSaveVoxSuccess,
            onCancel: OnSaveVoxCanceled,
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Save",
            saveButtonText: "Save"
        );
    }

    private void OnSaveVoxSuccess(string[] paths)
    {
        feature.ApplyAction(new EditorAction.SaveVox.OnPathSelected(paths[0]));
    }

    private void OnSaveVoxCanceled()
    {
        feature.ApplyAction(new EditorAction.SaveVox.OnCanceled());
    }
}
}