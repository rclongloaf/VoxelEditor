﻿using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.EditModes;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class EditorView : MonoBehaviour,
    EditorEventsConsumer,
    EditorUIHolder.Listener,
    SpriteImportUIHolder.Listener
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
    private GameObject editModeRoot = null!;
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
    private SpriteImportUIHolder spriteImportUIHolder = null!;
    
    private HashSet<KeyCode> pressedKeys = new();
    private HashSet<KeyCode> newPressedKeys = new();
    
    private void Awake()
    {
        feature = new EditorFeature(this, this);
        currentState = feature.state;
        editorUIHolder = new EditorUIHolder(editorUIDocument, this);
        spriteImportUIHolder = new SpriteImportUIHolder(spriteImportUIDocument, this);
        spriteImportUIHolder.SetVisibility(false);
        editModeController = new EditModeController(editModeRoot, voxelPrefab, voxelMaterial);
        editModeController.SetVisibility(true);
        renderModeController = new RenderModeController(renderModeRoot, renderModelMeshFilter, renderModelMeshRenderer, renderModelMaterial);
        renderModeController.Hide();
    }

    private void Update()
    {
        newPressedKeys.Clear();

        UpdateKeyPressedStatus(KeyCode.Mouse0);
        UpdateKeyPressedStatus(KeyCode.Mouse1);
        UpdateKeyPressedStatus(KeyCode.Mouse2);

        foreach (var key in newPressedKeys)
        {
            if (!pressedKeys.Contains(key))
            {
                feature.ApplyAction(new EditorAction.Input.OnButtonDown(key));
            }
        }

        foreach (var key in pressedKeys)
        {
            if (!newPressedKeys.Contains(key))
            {
                feature.ApplyAction(new EditorAction.Input.OnButtonUp(key));
            }
        }

        if (currentState is EditorState.Loaded loadedState)
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
                editorUIHolder.SetVisibility(true);
                spriteImportUIHolder.SetVisibility(false);
                break;
        }
    }

    public void Consume(EditorEvent editorEvent)
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

    public void OnApplyImportSettings(SpriteRectData spriteRectData)
    {
        feature.ApplyAction(new EditorAction.SpriteSettings.Selected(spriteRectData));
    }

    public void OnCancel()
    {
        feature.ApplyAction(new EditorAction.SpriteSettings.Canceled());
    }
    
    public void OnLoadVoxClicked()
    {
        feature.ApplyAction(new EditorAction.LoadVox.OnLoadClicked());
    }

    public void OnLoadTextureClicked()
    {
        feature.ApplyAction(new EditorAction.LoadTexture.OnLoadClicked());
    }

    public void OnSaveVoxClicked()
    {
        feature.ApplyAction(new EditorAction.SaveVox.OnSaveClicked());
    }

    public void OnImportClicked()
    {
        feature.ApplyAction(new EditorAction.Import.OnImportClicked());
    }

    public void OnExportClicked()
    {
        feature.ApplyAction(new EditorAction.Export.OnExportClicked());
    }

    public void OnEditModeClicked()
    {
        feature.ApplyAction(new EditorAction.EditMode.OnEditModeClicked());
    }

    public void OnRenderModeClicked()
    {
        feature.ApplyAction(new EditorAction.EditMode.OnRenderModeClicked());
    }

    public void OnBrushAddClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushAddClicked());
    }

    public void OnBrushDeleteClicked()
    {
        feature.ApplyAction(new EditorAction.Brush.OnBrushDeleteClicked());
    }

    private void ApplyLoadedState(EditorState.Loaded state)
    {
        if (currentState == state) return;
        
        editorUIHolder.SetVisibility(true);
        spriteImportUIHolder.SetVisibility(false);

        editModeController.ApplyVoxels(state.voxels);

        if (currentState is EditorState.Loaded curLoadedState
            && curLoadedState.editModeState != state.editModeState)
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
        
        if (currentState is not EditorState.Loaded curLoaded2
            || curLoaded2.texture != state.texture)
        {
            editModeController.ApplyTexture(state.texture);
        }
        if (currentState is not EditorState.Loaded curLoaded1
            || curLoaded1.spriteRectData != state.spriteRectData)
        {
            editModeController.ApplySpriteRect(state.spriteRectData);
        }

        freeCameraTransform.position = state.freeCameraData.pivotPoint
                                       + state.freeCameraData.rotation * new Vector3(0, 0, -state.freeCameraData.distance);
        freeCameraTransform.rotation = state.freeCameraData.rotation;
        
        isometricCameraTransform.position = state.isometricCameraData.position;
        
        currentState = state;
    }

    private void ApplySpriteSelectingState(EditorState.SpriteSelecting state)
    {
        editorUIHolder.SetVisibility(false);
        spriteImportUIHolder.SetVisibility(true);
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