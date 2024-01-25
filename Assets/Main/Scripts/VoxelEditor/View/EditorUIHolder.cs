﻿using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.VoxelEditor.View
{
public class EditorUIHolder
{
    private VisualElement root;
    private Label spriteIndexLabel;
    private IntegerField pivotXField;
    private IntegerField pivotYField;
    
    public EditorUIHolder(UIDocument doc, Listener listener)
    {
        root = doc.rootVisualElement;
        var loadBtn = root.Q<Button>("LoadVoxBtn");
        var loadTextureBtn = root.Q<Button>("LoadTextureBtn");
        var saveBtn = root.Q<Button>("SaveVoxBtn");
        var importBtn = root.Q<Button>("ImportBtn");
        var exportBtn = root.Q<Button>("ExportBtn");
        var copyModelBtn = root.Q<Button>("CopyModelBtn");
        var pasteModelBtn = root.Q<Button>("PasteModelBtn");
        var editModeBtn = root.Q<Button>("EditModeBtn");
        var renderModeBtn = root.Q<Button>("RenderModeBtn");
        var brushAddBtn = root.Q<Button>("BrushAddBtn");
        var brushDeleteBtn = root.Q<Button>("BrushDeleteBtn");
        spriteIndexLabel = root.Q<Label>("SpriteIndexLabel");
        var spritePreviousBtn = root.Q<Button>("PreviousSpriteBtn");
        var spriteNextBtn = root.Q<Button>("NextSpriteBtn");
        var toggleCameraBtn = root.Q<Button>("ToggleCameraBtn");
        var toggleGridBtn = root.Q<Button>("ToggleGridBtn");
        var toggleTransparentBtn = root.Q<Button>("ToggleTransparentBtn");
        var cancelLastActionBtn = root.Q<Button>("CancelActionBtn");
        var restoreCanceledActionBtn = root.Q<Button>("RestoreActionBtn");
        pivotXField = root.Q<IntegerField>("PivotXField");
        pivotYField = root.Q<IntegerField>("PivotYField");
        var applyPivotBtn = root.Q<Button>("ApplyPivotBtn");

        loadBtn.clicked += listener.OnLoadVoxClicked;
        loadTextureBtn.clicked += listener.OnLoadTextureClicked;
        saveBtn.clicked += listener.OnSaveVoxClicked;
        importBtn.clicked += listener.OnImportClicked;
        exportBtn.clicked += listener.OnExportClicked;
        editModeBtn.clicked += listener.OnEditModeClicked;
        copyModelBtn.clicked += listener.OnCopyModelClicked;
        pasteModelBtn.clicked += listener.OnPasteModelClicked;
        renderModeBtn.clicked += listener.OnRenderModeClicked;
        brushAddBtn.clicked += listener.OnBrushAddClicked;
        brushDeleteBtn.clicked += listener.OnBrushDeleteClicked;
        spritePreviousBtn.clicked += listener.OnPreviousSpriteClicked;
        spriteNextBtn.clicked += listener.OnNextSpriteClicked;
        toggleCameraBtn.clicked += listener.OnToggleCameraClicked;
        toggleGridBtn.clicked += listener.OnToggleGridClicked;
        toggleTransparentBtn.clicked += listener.OnToggleTransparentClicked;
        cancelLastActionBtn.clicked += listener.OnCancelActionClicked;
        restoreCanceledActionBtn.clicked += listener.OnRestoreActionClicked;
        applyPivotBtn.clicked += () =>
        {
            listener.OnApplyPivotClicked(new Vector2(pivotXField.value, pivotYField.value));
        };
    }

    public void SetVisibility(bool visible)
    {
        root.SetVisibility(visible);
    }

    public void SetSpriteIndex(SpriteIndex spriteIndex)
    {
        spriteIndexLabel.text = $"row: {spriteIndex.rowIndex + 1}, column: {spriteIndex.columnIndex + 1}";
    }

    public void SetPivotPoint(Vector2 pivotPoint)
    {
        pivotXField.value = (int)pivotPoint.x;
        pivotYField.value = (int)pivotPoint.y;
    }

    public interface Listener
    {
        public void OnLoadVoxClicked();
        public void OnLoadTextureClicked();
        public void OnSaveVoxClicked();
        public void OnImportClicked();
        public void OnExportClicked();
        public void OnEditModeClicked();
        public void OnRenderModeClicked();
        public void OnBrushAddClicked();
        public void OnBrushDeleteClicked();

        public void OnPreviousSpriteClicked();

        public void OnNextSpriteClicked();
        public void OnCopyModelClicked();
        public void OnPasteModelClicked();
        public void OnToggleCameraClicked();
        public void OnToggleGridClicked();
        public void OnToggleTransparentClicked();
        public void OnCancelActionClicked();
        public void OnRestoreActionClicked();

        public void OnApplyPivotClicked(Vector2 pivotPoint);
    }
}
}