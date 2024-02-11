using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Brush;
using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;
using CameraType = Main.Scripts.VoxelEditor.State.CameraType;

namespace Main.Scripts.VoxelEditor
{
public class EditorReducer
{
    private EditorFeature feature;
    private EditorState state => feature.state;

    public EditorReducer(EditorFeature feature)
    {
        this.feature = feature;
    }

    public void ApplyPatch(EditorPatch patch)
    {
        var newState = patch switch
        {
            EditorPatch.ActionsHistory actionsHistoryPatch => ApplyActionsHistoryPatch(actionsHistoryPatch),
            EditorPatch.SpriteChanges spriteChangesPatch => ApplySpriteChangesPatch(spriteChangesPatch),
            EditorPatch.FileBrowser fileBrowserPatch => ApplyFileBrowserPatch(fileBrowserPatch),
            EditorPatch.Import importPatch => ApplyImportPatch(importPatch),
            EditorPatch.Layers layersPatch => ApplyLayersPatch(layersPatch),
            EditorPatch.MenuVisibility menuVisibilityPatch => ApplyMenuVisibilityPatch(menuVisibilityPatch),
            EditorPatch.ModelBuffer modelBufferPatch => ApplyModelBufferPatch(modelBufferPatch),
            EditorPatch.Shader shaderPatch => ApplyShaderPatch(shaderPatch),
            EditorPatch.VoxLoaded voxLoadedPatch => ApplyVoxLoadedPatch(voxLoadedPatch),
            EditorPatch.TextureLoaded textureLoadedPatch => ApplyTextureLoadedPatch(textureLoadedPatch),
            EditorPatch.Control controlPatch => ApplyControlPatch(controlPatch),
            EditorPatch.VoxelsChanges voxelsChanges => ApplyVoxelsChangesPatch(voxelsChanges),
            EditorPatch.EditMode editModePatch => ApplyEditModePatch(editModePatch),
            EditorPatch.Brush brushPatch => ApplyBrushPatch(brushPatch),
            EditorPatch.NewPivotPoint newPivotPointPatch => ApplyNewPivotPointPatch(newPivotPointPatch),
            EditorPatch.Camera cameraPatch => ApplyCameraPatch(cameraPatch),
            EditorPatch.Selection selection => ApplySelectionPatch(selection),
            EditorPatch.ChangeSpriteIndex changeSpriteIndexPatch => ApplyChangeSpriteIndex(changeSpriteIndexPatch),
            EditorPatch.ChangeSpriteRefVisibility changeSpriteRefVisibilityPatch => ApplyChangeSpriteRefVisibilityPatch(changeSpriteRefVisibilityPatch),
            _ => throw new ArgumentOutOfRangeException(nameof(patch), patch, null)
        };

        feature.UpdateState(newState);
    }

    private EditorState ApplySpriteChangesPatch(EditorPatch.SpriteChanges patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;

        switch (patch)
        {
            case EditorPatch.SpriteChanges.Apply apply:
            {
                var sprites = activeLayer.voxData.sprites;
                var newSprites = new Dictionary<SpriteIndex, SpriteData>(sprites);

                newSprites[activeLayer.currentSpriteIndex] = activeLayer.currentSpriteData;

                var layers = new Dictionary<int, VoxLayerState>(state.layers);
                layers[state.activeLayerKey] = activeLayer with
                {
                    voxData = activeLayer.voxData with
                    {
                        sprites = newSprites
                    }
                };

                return state with
                {
                    layers = layers,
                    uiState = new UIState.None()
                };
            }
            case EditorPatch.SpriteChanges.ApplyRequest applyRequest:
                return state with { uiState = new UIState.ApplySpriteChanges(applyRequest.actionOnApply) };
            case EditorPatch.SpriteChanges.Cancel cancel:
                return state with { uiState = new UIState.None() };
            case EditorPatch.SpriteChanges.Discard discard:
            {
                var layers = new Dictionary<int, VoxLayerState>(state.layers);
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.voxData.sprites[activeLayer.currentSpriteIndex],
                    actionsHistory = new Stack<EditAction>(),
                    canceledActionsHistory = new Stack<EditAction>(),
                };
                return state with
                {
                    layers = layers,
                    uiState = new UIState.None()
                };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyFileBrowserPatch(EditorPatch.FileBrowser patch)
    {
        return state with
        {
            uiState = patch is EditorPatch.FileBrowser.Opened ? new UIState.FileBrowser() : new UIState.None()
        };
    }

    private EditorState ApplyImportPatch(EditorPatch.Import patch)
    {
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        layers[state.activeLayerKey] = patch switch
        {
            EditorPatch.Import.TextureSelected textureSelected => new VoxLayerState.SpriteSelecting(texture: textureSelected.texture),
            EditorPatch.Import.Cancel cancel => new VoxLayerState.Init(),
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
        
        return state with
        {
            layers = layers
        };
    }

    private EditorState ApplyLayersPatch(EditorPatch.Layers patch)
    {
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        
        switch (patch)
        {
            case EditorPatch.Layers.ChangeVisibility changeVisibility:
                if (layers[changeVisibility.key] is not VoxLayerState.Loaded layer) return state;
                layers[changeVisibility.key] = layer with
                {
                    isVisible = !layer.isVisible,
                };
                return state with
                {
                    layers = layers,
                };
                break;
            case EditorPatch.Layers.Create create:
                layers[create.key] = new VoxLayerState.Init();
                return state with
                {
                    layers = layers,
                    activeLayerKey = create.key,
                    isSpriteRefVisible = false,
                    uiState = new UIState.Menu()
                };
                break;
            case EditorPatch.Layers.Delete.Apply apply:
                layers.Remove(apply.key);
                return state with
                {
                    layers = layers,
                    isSpriteRefVisible = false,
                    uiState = new UIState.None()
                };
                break;
            case EditorPatch.Layers.Delete.Cancel cancel:
                return state with
                {
                    uiState = new UIState.None()
                };
                break;
            case EditorPatch.Layers.Delete.Request request:
                return state with
                {
                    uiState = new UIState.ApplyLayerDelete()
                };
                break;
            case EditorPatch.Layers.Select select:
                return state with
                {
                    activeLayerKey = select.key,
                    isSpriteRefVisible = false,
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyMenuVisibilityPatch(EditorPatch.MenuVisibility patch)
    {
        if (state.uiState is not UIState.None and not UIState.Menu) return state;

        return state with
        {
            uiState = patch.visible ? new UIState.Menu() : new UIState.None()
        };
    }

    private EditorState ApplyModelBufferPatch(EditorPatch.ModelBuffer patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;

        switch (patch)
        {
            case EditorPatch.ModelBuffer.Copy copy:
                return state with { bufferedSpriteData = copy.spriteData };
            default: throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyShaderPatch(EditorPatch.Shader patch)
    {
        return patch switch
        {
            EditorPatch.Shader.ChangeGridEnabled changeGridEnabled => state with
            {
                shaderData = state.shaderData with
                {
                    isGridEnabled = changeGridEnabled.enabled
                }
            },
            EditorPatch.Shader.ChangeTransparentEnabled changeTransparentEnabled => state with
            {
                shaderData = state.shaderData with
                {
                    isTransparentEnabled = changeTransparentEnabled.enabled
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
    }

    private EditorState ApplyChangeSpriteRefVisibilityPatch(EditorPatch.ChangeSpriteRefVisibility patch)
    {
        return state with
        {
            isSpriteRefVisible = patch.visible
        };
    }

    private EditorState ApplyVoxLoadedPatch(EditorPatch.VoxLoaded patch)
    {
        var texture = state.activeLayer switch
        {
            VoxLayerState.Loaded loaded => loaded.texture,
            VoxLayerState.SpriteSelecting spriteSelecting => spriteSelecting.texture,
            VoxLayerState.Init waitingForProject => null,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

        var spriteIndex = new SpriteIndex(0, 0);

        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        layers[state.activeLayerKey] = new VoxLayerState.Loaded(
            isVisible: true,
            voxData: patch.voxData,
            texture: texture,
            currentSpriteIndex: spriteIndex,
            currentSpriteData: patch.voxData.sprites[spriteIndex],
            selectionState: new SelectionState.None(),
            actionsHistory: new Stack<EditAction>(),
            canceledActionsHistory: new Stack<EditAction>()
        );

        return state with
        {
            layers = layers,
            uiState = new UIState.None()
        };
    }

    private EditorState ApplyTextureLoadedPatch(EditorPatch.TextureLoaded patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        layers[state.activeLayerKey] = activeLayer with
        {
            texture = patch.texture
        };

        return state with { layers = layers };
    }

    private EditorState ApplyControlPatch(EditorPatch.Control patch)
    {
        switch (patch)
        {
            case EditorPatch.Control.Drawing.Start drawingStartAction:
                return state with
                {
                    controlState = new ControlState.Drawing(
                        drawnVoxels: new List<Vector3Int>(),
                        position: drawingStartAction.position,
                        normal: drawingStartAction.normal,
                        deleting: drawingStartAction.deleting,
                        bySection: drawingStartAction.bySection,
                        withProjection: drawingStartAction.withProjection
                    )
                };
                case EditorPatch.Control.Drawing.Finish:
                return state with { controlState = new ControlState.None() };
            case EditorPatch.Control.Moving.Start:
                return state with { controlState = new ControlState.CameraMoving() };
            case EditorPatch.Control.Moving.Finish:
                return state with { controlState = new ControlState.None() };
            case EditorPatch.Control.Rotating.Start:
                return state with { controlState = new ControlState.Rotating() };
            case EditorPatch.Control.Rotating.Finish:
                return state with { controlState = new ControlState.None() };
            case EditorPatch.Control.Selection.Start start:
                return state with
                {
                    controlState = new ControlState.Selection(start.mousePos)
                };
            case EditorPatch.Control.SelectionMoving.ChangeSelectionOffset changeSelectionOffset:
            {
                if (state.activeLayer is not VoxLayerState.Loaded
                    {
                        selectionState: SelectionState.Selected selectionState
                    } activeLayer)
                {
                    return state;
                }
                var layers = new Dictionary<int, VoxLayerState>(state.layers);
                
                layers[state.activeLayerKey] = activeLayer with
                {
                    selectionState = selectionState with
                    {
                        offset = selectionState.offset + changeSelectionOffset.deltaOffset
                    }
                };

                var controlState = (state.controlState is not ControlState.SelectionMoving selectionMoving)
                    ? state.controlState
                    : selectionMoving with
                    {
                        deltaOffset = selectionMoving.deltaOffset + changeSelectionOffset.deltaOffset
                    };
                
                return state with
                {
                    layers = layers,
                    controlState = controlState
                };
            }
            case EditorPatch.Control.SelectionMoving.Finish:
                return state with { controlState = new ControlState.None() };
            case EditorPatch.Control.SelectionMoving.Start selectionMoving:
                return state with
                {
                    controlState = new ControlState.SelectionMoving(
                        selectionMoving.normal,
                        selectionMoving.fromPosition,
                        Vector3Int.zero
                    )
                };
            case EditorPatch.Control.Selection.Finish finish:
            {
                return state with
                {
                    controlState = new ControlState.None()
                };
            }
            default: throw new ArgumentOutOfRangeException(nameof(patch), patch, null);
        }
    }

    private EditorState ApplyVoxelsChangesPatch(EditorPatch.VoxelsChanges patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        
        IEnumerable<Vector3Int> voxels;
        switch (patch)
        {
            case EditorPatch.VoxelsChanges.Add addPatch:
                voxels = addPatch.voxels;
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        voxels = new HashSet<Vector3Int>(activeLayer.currentSpriteData.voxels).Plus(addPatch.voxels)
                    }
                };
                break;
            case EditorPatch.VoxelsChanges.Delete deletePatch:
                voxels = deletePatch.voxels;
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        voxels = new HashSet<Vector3Int>(activeLayer.currentSpriteData.voxels).Minus(deletePatch.voxels)
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        };
        
        var controlState = state.controlState;
        if (controlState is ControlState.Drawing drawingState)
        {
            var drawnVoxels = new List<Vector3Int>(drawingState.drawnVoxels);
            drawnVoxels.AddRange(voxels);
            controlState = drawingState with
            {
                drawnVoxels = drawnVoxels
            };
        }

        return state with
        {
            layers = layers,
            controlState = controlState
        };
    }

    private EditorState ApplyActionsHistoryPatch(EditorPatch.ActionsHistory patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);

        switch (patch)
        {
            case EditorPatch.ActionsHistory.NewAction newAction:
            {
                var newActionHistory = new Stack<EditAction>(activeLayer.actionsHistory.Reverse());
                newActionHistory.Push(newAction.action);
                
                var canceledActionsHistory = activeLayer.canceledActionsHistory;

                layers[state.activeLayerKey] = activeLayer with
                {
                    actionsHistory = newActionHistory,
                    canceledActionsHistory = canceledActionsHistory.Count > 0 ? new Stack<EditAction>() : canceledActionsHistory
                };
                
                return state with
                {
                    layers = layers
                };
            }
            case EditorPatch.ActionsHistory.CancelAction cancelAction:
            {
                var newActionHistory = new Stack<EditAction>(activeLayer.actionsHistory.Reverse());
                var lastAction = newActionHistory.Pop();

                var newCanceledActionsHistory = new Stack<EditAction>(activeLayer.canceledActionsHistory.Reverse());
                newCanceledActionsHistory.Push(lastAction);
                
                layers[state.activeLayerKey] = activeLayer with
                {
                    actionsHistory = newActionHistory,
                    canceledActionsHistory = newCanceledActionsHistory
                };

                return state with
                {
                    layers = layers
                };
            }
            case EditorPatch.ActionsHistory.RestoreAction restoreAction:
            {
                var newCanceledActionsHistory = new Stack<EditAction>(activeLayer.canceledActionsHistory.Reverse());
                var lastCanceledAction = newCanceledActionsHistory.Pop();

                var newActionsHistory = new Stack<EditAction>(activeLayer.actionsHistory.Reverse());
                newActionsHistory.Push(lastCanceledAction);
                
                layers[state.activeLayerKey] = activeLayer with
                {
                    actionsHistory = newActionsHistory,
                    canceledActionsHistory = newCanceledActionsHistory
                };

                return state with
                {
                    layers = layers
                };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyEditModePatch(EditorPatch.EditMode patch)
    {
        switch (patch)
        {
            case EditorPatch.EditMode.EditModeSelected editModeSelected:
                if (state.editModeState is EditModeState.EditMode) return state;
                return state with
                {
                    editModeState = new EditModeState.EditMode()
                };
                break;
            case EditorPatch.EditMode.RenderModeSelected renderModeSelected:
                if (state.editModeState is EditModeState.RenderMode) return state;
                return state with
                {
                    editModeState = new EditModeState.RenderMode()
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyBrushPatch(EditorPatch.Brush patch)
    {
        return patch switch
        {
            EditorPatch.Brush.ChangeMode changeMode => state with
            {
                brushData = state.brushData with
                {
                    mode = changeMode.brushMode
                }
            },
            EditorPatch.Brush.ChangeType changeType => state with
            {
                brushData = state.brushData with
                {
                    type = changeType.brushType
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
    }

    private EditorState ApplyNewPivotPointPatch(EditorPatch.NewPivotPoint patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        layers[state.activeLayerKey] = activeLayer with
        {
            currentSpriteData = activeLayer.currentSpriteData with
            {
                pivot = patch.pivotPoint
            }
        };

        return state with
        {
            layers = layers
        };
    }

    private EditorState ApplyCameraPatch(EditorPatch.Camera patch)
    {
        switch (patch)
        {
            case EditorPatch.Camera.NewPivotPoint newPosition:
                if (state.cameraType is CameraType.Free)
                {
                    return state with
                    {
                        freeCameraData = state.freeCameraData with
                        {
                            pivotPoint = newPosition.position
                        }
                    };
                }

                return state with
                {
                    isometricCameraData = new IsometricCameraData(newPosition.position)
                };
                break;
            case EditorPatch.Camera.NewDistance newDistance:
                if (state.cameraType is CameraType.Free)
                {
                    return state with
                    {
                        freeCameraData = state.freeCameraData with
                        {
                            distance = newDistance.distance
                        }
                    };
                }

                return state;
                break;
            case EditorPatch.Camera.NewRotation newRotation:
                return state with
                {
                    freeCameraData = state.freeCameraData with
                    {
                        rotation = newRotation.rotation
                    }
                };
                break;
            case EditorPatch.Camera.ChangeType changeType:
                return state with
                {
                    cameraType = changeType.cameraType
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplySelectionPatch(EditorPatch.Selection patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) 
        {
            return state;
        }
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);

        switch (patch)
        {
            case EditorPatch.Selection.CancelSelection cancelSelection:
                layers[state.activeLayerKey] = activeLayer with
                {
                    selectionState = new SelectionState.None()
                };

                return state with
                {
                    layers = layers
                };
            case EditorPatch.Selection.Select select:
                layers[state.activeLayerKey] = activeLayer with
                {
                    selectionState = new SelectionState.Selected(
                        voxels: select.voxels,
                        offset: select.offset
                    )
                };
                return state with
                {
                    layers = layers
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyChangeSpriteIndex(EditorPatch.ChangeSpriteIndex patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        layers[state.activeLayerKey] = activeLayer with
        {
            currentSpriteData = activeLayer.voxData.sprites[patch.spriteIndex],
            currentSpriteIndex = patch.spriteIndex,
            actionsHistory = new Stack<EditAction>(),
            canceledActionsHistory = new Stack<EditAction>()
        };

        return state with
        {
            layers = layers,
        };
    }
}
}