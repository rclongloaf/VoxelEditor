using System;
using System.Collections.Generic;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.Voxels;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class EditorView : MonoBehaviour,
    EditorEventsConsumer,
    EditorUIHolder.Listener
{
    [SerializeField]
    private Transform freeCameraTransform = null!;
    [SerializeField]
    private Transform isometricCameraTransform = null!;
    [SerializeField]
    private UIDocument uiDocument = null!;
    [SerializeField]
    private GameObject voxelPrefab = null!;
    
    private EditorFeature feature = null!;
    private EditorState currentState = null!;
    private EditorVoxelsHolder voxelsHolder = null!;
    private HashSet<KeyCode> pressedKeys = new();
    private HashSet<KeyCode> newPressedKeys = new();
    
    private void Awake()
    {
        feature = new EditorFeature(this, this);
        currentState = feature.state;
        var editorUIHolder = new EditorUIHolder(uiDocument, this);
        voxelsHolder = new EditorVoxelsHolder(voxelPrefab);
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
            case EditorState.WaitingForProject:
                //init state
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
                break;
            case EditorEvent.OpenBrowserForLoadVox openBrowserForLoadVox:
                LoadVoxFile();
                break;
            case EditorEvent.OpenBrowserForSaveVox openBrowserForSaveVox:
                SaveVoxFile();
                break;
        }
    }
    
    
    public void OnLoadClicked()
    {
        feature.ApplyAction(new EditorAction.LoadVox.OnLoadClicked());
    }

    public void OnSaveClicked()
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
        currentState = state;

        voxelsHolder.ApplyVoxels(state.voxels);
        
        freeCameraTransform.position = state.freeCameraData.pivotPoint
                                       + state.freeCameraData.rotation * new Vector3(0, 0, -state.freeCameraData.distance);
        freeCameraTransform.rotation = state.freeCameraData.rotation;
        
        isometricCameraTransform.position = state.isometricCameraData.position;
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