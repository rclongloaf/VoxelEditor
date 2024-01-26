using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.EditModes;
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
    ApplyChangesUIHolder.Listener
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
    private GameObject editModeRoot = null!;
    [SerializeField]
    private SpriteRenderer spriteReference = null!;
    [SerializeField]
    private Transform pivotPointTransform = null!;
    [SerializeField]
    private GameObject voxelPrefab = null!;
    [SerializeField]
    private Material voxelMaterial = null!;
    [SerializeField]
    private GameObject renderModeRoot = null!;
    [SerializeField]
    private Material renderModelMaterial = null!;
    [SerializeField]
    private MeshFilter renderModelMeshFilter = null!;
    [SerializeField]
    private MeshRenderer renderModelMeshRenderer = null!;
    
    private EditorFeature feature = null!;
    private EditorState currentState = null!;
    private EditModeController editModeController = null!;
    private RenderModeController renderModeController = null!;
    private EditorUIHolder editorUIHolder = null!;
    private TextureImportUIHolder textureImportUIHolder = null!;
    private ApplyChangesUIHolder applyChangesUIHolder = null!;
    
    private HashSet<KeyCode> pressedKeys = new();
    private HashSet<KeyCode> newPressedKeys = new();
    
    private void Awake()
    {
        feature = new EditorFeature(this, this);
        currentState = feature.state;
        editorUIHolder = new EditorUIHolder(editorUIDocument, this);
        editorUIHolder.SetLoadedState(false);
        textureImportUIHolder = new TextureImportUIHolder(spriteImportUIDocument, this);
        textureImportUIHolder.SetVisibility(false);
        applyChangesUIHolder = new ApplyChangesUIHolder(applyChangesUIDocument, this);
        applyChangesUIHolder.SetVisibility(false);
        editModeController = new EditModeController(editModeRoot, spriteReference, voxelPrefab, voxelMaterial);
        editModeController.SetVisibility(true);
        renderModeController = new RenderModeController(renderModeRoot, renderModelMeshFilter, renderModelMeshRenderer, renderModelMaterial);
        renderModeController.Hide();
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

        var withCtrl = Input.GetKey(KeyCode.LeftControl);
        var withShift = Input.GetKey(KeyCode.LeftShift);

        var editorUIHolderListener = (EditorUIHolder.Listener)this;

        var loadedState = currentState as EditorState.Loaded;
        
        if (newPressedKeys.Contains(KeyCode.Escape) && !pressedKeys.Contains(KeyCode.Escape))
        {
            feature.ApplyAction(new EditorAction.Input.OnMenu());
            (pressedKeys, newPressedKeys) = (newPressedKeys, pressedKeys);
            return;
        }

        if (loadedState != null && loadedState.uiState is not UIState.None)
        {
            (pressedKeys, newPressedKeys) = (newPressedKeys, pressedKeys);
            return;
        }


        foreach (var key in newPressedKeys)
        {
            if (!pressedKeys.Contains(key))
            {
                switch (key)
                {
                    case KeyCode.Mouse0 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Draw(withCtrl, withShift));
                        break;
                    case KeyCode.Mouse1 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.Rotate());
                        break;
                    case KeyCode.Mouse2 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonDown.MoveCamera());
                        break;
                    case KeyCode.LeftArrow when loadedState != null:
                        editorUIHolderListener.OnPreviousSpriteClicked();
                        break;
                    case KeyCode.RightArrow when loadedState != null:
                        editorUIHolderListener.OnNextSpriteClicked();
                        break;
                    case KeyCode.Tab when loadedState != null:
                        editorUIHolderListener.OnToggleCameraClicked();
                        break;
                    case KeyCode.R when loadedState != null:
                        if (loadedState.editModeState is EditModeState.EditMode)
                        {
                            editorUIHolderListener.OnRenderModeClicked();
                        }
                        else
                        {
                            editorUIHolderListener.OnEditModeClicked();
                        }
                        break;
                    case KeyCode.C when loadedState != null && withCtrl:
                        editorUIHolderListener.OnCopyModelClicked();
                        break;
                    case KeyCode.V when loadedState != null && withCtrl:
                        editorUIHolderListener.OnPasteModelClicked();
                        break;
                    case KeyCode.Z when loadedState != null && withCtrl:
                        editorUIHolderListener.OnCancelActionClicked();
                        break;
                    case KeyCode.Z when loadedState != null && withCtrl && withShift:
                    case KeyCode.Y when loadedState != null && withCtrl:
                        editorUIHolderListener.OnRestoreActionClicked();
                        break;
                    case KeyCode.G when loadedState != null:
                        editorUIHolderListener.OnToggleGridClicked();
                        break;
                    case KeyCode.T when loadedState != null:
                        editorUIHolderListener.OnToggleTransparentClicked();
                        break;
                    case KeyCode.Q when loadedState != null:
                        editorUIHolderListener.OnToggleSpriteRefClicked();
                        break;
                    case KeyCode.I when loadedState != null:
                        editorUIHolderListener.OnApplyPivotClicked(loadedState.currentSpriteData.pivot + Vector2.up);
                        break;
                    case KeyCode.J when loadedState != null:
                        editorUIHolderListener.OnApplyPivotClicked(loadedState.currentSpriteData.pivot + Vector2.left);
                        break;
                    case KeyCode.K when loadedState != null:
                        editorUIHolderListener.OnApplyPivotClicked(loadedState.currentSpriteData.pivot + Vector2.down);
                        break;
                    case KeyCode.L when loadedState != null:
                        editorUIHolderListener.OnApplyPivotClicked(loadedState.currentSpriteData.pivot + Vector2.right);
                        break;
                }
            }
        }

        foreach (var key in pressedKeys)
        {
            if (!newPressedKeys.Contains(key))
            {
                switch (key)
                {
                    case KeyCode.Mouse0 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.Draw());
                        break;
                    case KeyCode.Mouse1 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.Rotate());
                        break;
                    case KeyCode.Mouse2 when loadedState != null:
                        feature.ApplyAction(new EditorAction.Input.OnButtonUp.MoveCamera());
                        break;
                }
            }
        }

        if (loadedState != null)
        {
            switch (loadedState.controlState)
            {
                case ControlState.None:
                    var wheelDelta = Input.GetAxis("Mouse ScrollWheel");
                    if (Math.Abs(wheelDelta) > 0)
                    {
                        feature.ApplyAction(new EditorAction.Input.OnWheelScroll(wheelDelta));
                    }
                    break;
                case ControlState.Drawing:
                    break;
                case ControlState.Moving:
                case ControlState.Rotating:
                    feature.ApplyAction(new EditorAction.Input.OnMouseDelta(
                        deltaX: Input.GetAxis("Mouse X"),
                        deltaY: Input.GetAxis("Mouse Y")
                    ));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

    public void ApplyState(EditorState state)
    {
        switch (state)
        {
            case EditorState.Loaded loadedState:
                ApplyLoadedState(loadedState);
                break;
            case EditorState.SpriteSelecting spriteSelectingState:
                ApplySpriteSelectingState(spriteSelectingState);
                break;
            case EditorState.WaitingForProject:
                editorUIHolder.SetLoadedState(false);
                editorUIHolder.SetVisibility(true);
                textureImportUIHolder.SetVisibility(false);
                applyChangesUIHolder.SetVisibility(false);
                break;
        }
    }

    void EditorEventsConsumer.Consume(EditorEvent editorEvent)
    {
        switch (editorEvent)
        {
            case EditorEvent.OpenBrowserForExport openBrowserForExport:
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

    void TextureImportUIHolder.Listener.OnApplyImportSettings(TextureData textureData)
    {
        feature.ApplyAction(new EditorAction.TextureSettings.Selected(textureData));
    }

    void TextureImportUIHolder.Listener.OnCancel()
    {
        feature.ApplyAction(new EditorAction.TextureSettings.Canceled());
    }

    void ApplyChangesUIHolder.Listener.OnApplyClicked()
    {
        feature.ApplyAction(new EditorAction.ApplyChanges.Apply());
    }

    void ApplyChangesUIHolder.Listener.OnDiscardClicked()
    {
        feature.ApplyAction(new EditorAction.ApplyChanges.Discard());
    }

    void ApplyChangesUIHolder.Listener.OnCancelClicked()
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

    private void ApplyLoadedState(EditorState.Loaded state)
    {
        if (currentState == state) return;
        
        editorUIHolder.SetLoadedState(true);
        editorUIHolder.SetVisibility(state.uiState is UIState.Menu);
        textureImportUIHolder.SetVisibility(state.uiState is UIState.TextureImport);
        applyChangesUIHolder.SetVisibility(state.uiState is UIState.ApplyChanges);

        editModeController.ApplyVoxels(state.currentSpriteData.voxels);

        var curLoadedState = currentState as EditorState.Loaded;

        if (curLoadedState != null)
        {
            if (curLoadedState.editModeState != state.editModeState)
            {
                switch (state.editModeState)
                {
                    case EditModeState.EditMode editMode:
                        renderModeController.Hide();
                        editModeController.SetVisibility(true);
                        break;
                    case EditModeState.RenderMode renderMode:
                        editModeController.SetVisibility(false);
                        renderModeController.Show(renderMode.mesh, state.texture);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        if (curLoadedState == null || curLoadedState.currentSpriteIndex != state.currentSpriteIndex)
        {
            editorUIHolder.SetSpriteIndex(state.currentSpriteIndex);
        }
        
        if (curLoadedState == null
            || curLoadedState.texture != state.texture)
        {
            editModeController.ApplyTexture(state.texture);
        }
        if (curLoadedState == null
            || curLoadedState.currentSpriteIndex != state.currentSpriteIndex)
        {
            editModeController.ApplySpriteRect(state.voxData.textureData, state.currentSpriteIndex);
        }

        if (curLoadedState == null
            || curLoadedState.shaderData != state.shaderData)
        {
            editModeController.ApplyShaderData(state.shaderData);
            pivotPointTransform.gameObject.SetActive(state.shaderData.isGridEnabled);
        }

        if (curLoadedState == null
            || curLoadedState.currentSpriteData.pivot != state.currentSpriteData.pivot)
        {
            var pivotPoint = state.currentSpriteData.pivot;
            editorUIHolder.SetPivotPoint(pivotPoint);
            pivotPointTransform.transform.position = new Vector3(pivotPoint.x, pivotPoint.y, 0);
        }

        if (curLoadedState == null
            || curLoadedState.isSpriteRefVisible != state.isSpriteRefVisible)
        {
            editModeController.SetReferenceVisibility(state.isSpriteRefVisible);
        }

        freeCameraTransform.position = state.freeCameraData.pivotPoint
                                       - (state.editModeState is EditModeState.RenderMode ? state.currentSpriteData.pivot : Vector3.zero)
                                       + state.freeCameraData.rotation * new Vector3(0, 0, -state.freeCameraData.distance);
        freeCameraTransform.rotation = state.freeCameraData.rotation;
        freeCameraTransform.gameObject.SetActive(state.cameraType is CameraType.Free);
        
        isometricCameraTransform.position = state.isometricCameraData.position
                                            - (state.editModeState is EditModeState.RenderMode ? state.currentSpriteData.pivot : Vector3.zero);
        isometricCameraTransform.gameObject.SetActive(state.cameraType is CameraType.Isometric);
        
        currentState = state;
    }

    private void ApplySpriteSelectingState(EditorState.SpriteSelecting state)
    {
        editorUIHolder.SetVisibility(false);
        textureImportUIHolder.SetVisibility(true);
        applyChangesUIHolder.SetVisibility(false);
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
            onCancel: OnLoadVoxCancel,
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