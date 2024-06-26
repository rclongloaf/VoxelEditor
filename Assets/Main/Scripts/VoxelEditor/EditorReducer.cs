﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Utils;
using Main.Scripts.VoxelEditor.State;
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
            EditorPatch.Smooth smoothPatch => ApplySmoothPatch(smoothPatch),
            EditorPatch.VoxLoaded voxLoadedPatch => ApplyVoxLoadedPatch(voxLoadedPatch),
            EditorPatch.TextureLoaded textureLoadedPatch => ApplyTextureLoadedPatch(textureLoadedPatch),
            EditorPatch.Control controlPatch => ApplyControlPatch(controlPatch),
            EditorPatch.VoxelsChanges voxelsChanges => ApplyVoxelsChangesPatch(voxelsChanges),
            EditorPatch.EditMode editModePatch => ApplyEditModePatch(editModePatch),
            EditorPatch.PivotPoint pivotPointPatch => ApplyPivotPointPatch(pivotPointPatch),
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

    private EditorState ApplySmoothPatch(EditorPatch.Smooth patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;
        
        var layers = new Dictionary<int, VoxLayerState>(state.layers);

        var voxels = new Dictionary<Vector3Int, VoxelData>(activeLayer.currentSpriteData.voxels);
        
        switch (patch)
        {
            case EditorPatch.Smooth.ChangeMultiple changeMultiple:
                foreach (var (pos, enableSmooth) in changeMultiple.enableSmoothMap)
                {
                    voxels[pos] = voxels[pos] with
                    {
                        isSmooth = enableSmooth
                    };
                }
                break;
            case EditorPatch.Smooth.ChangeSingle changeSingle:
                voxels[changeSingle.voxel] = voxels[changeSingle.voxel] with
                {
                    isSmooth = changeSingle.enable
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
        
        layers[state.activeLayerKey] = activeLayer with
        {
            currentSpriteData = activeLayer.currentSpriteData with
            {
                voxels = voxels
            }
        };

        return state with
        {
            layers = layers
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
                        drawnVoxels: new Dictionary<Vector3Int, VoxelData>(),
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
            case EditorPatch.Control.RotatingVoxels.Start rotatingVoxelsPatch:
            {
                if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;

                Dictionary<Vector3Int, VoxelData> rotatingVoxels;

                if (activeLayer.selectionState is SelectionState.Selected selectedState)
                {
                    rotatingVoxels = selectedState.voxels;
                }
                else
                {
                    rotatingVoxels = activeLayer.currentSpriteData.voxels;
                }

                return state with
                {
                    controlState = new ControlState.RotatingVoxels(
                        rotatingVoxelsPatch.axis,
                        0,
                        rotatingVoxels
                    )
                };
            }
            case EditorPatch.Control.RotatingVoxels.ApplyNewVoxels applyNewVoxels:
            {
                if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;

                var layers = new Dictionary<int, VoxLayerState>(state.layers);

                if (activeLayer.selectionState is SelectionState.Selected selectionState)
                {
                    layers[state.activeLayerKey] = activeLayer with
                    {
                        selectionState = selectionState with
                        {
                            voxels = applyNewVoxels.voxels
                        }
                    };
                }
                else
                {
                    layers[state.activeLayerKey] = activeLayer with
                    {
                        currentSpriteData = activeLayer.currentSpriteData with
                        {
                            voxels = applyNewVoxels.voxels
                        }
                    };
                }

                return state with
                {
                    layers = layers
                };
            }
            case EditorPatch.Control.RotatingVoxels.ChangeAngle changeAnglePatch:
            {
                if (state.controlState is not ControlState.RotatingVoxels rotatingVoxelsState) return state;

                return state with
                {
                    controlState = rotatingVoxelsState with
                    {
                        angle = changeAnglePatch.angle
                    }
                };
            }
            case EditorPatch.Control.RotatingVoxels.Finish:
                return state with { controlState = new ControlState.None() };
            case EditorPatch.Control.RotatingCamera.Start:
                return state with { controlState = new ControlState.RotatingCamera() };
            case EditorPatch.Control.RotatingCamera.Finish:
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
            case EditorPatch.Control.Smoothing.Start smoothingStart:
                return state with
                {
                    controlState = new ControlState.Smoothing(
                        smoothVoxels: new HashSet<Vector3Int>(),
                        enableSmooth: smoothingStart.enableSmooth
                    )
                };
            case EditorPatch.Control.Smoothing.AddSmoothVoxel smoothingAddVoxels:
            {
                if (state.controlState is not ControlState.Smoothing smoothingState) return state;

                var smoothingStateVoxels = new HashSet<Vector3Int>(smoothingState.smoothVoxels);
                smoothingStateVoxels.Add(smoothingAddVoxels.voxel);

                return state with
                {
                    controlState = smoothingState with
                    {
                        smoothVoxels = smoothingStateVoxels
                    }
                };
            }
            case EditorPatch.Control.Smoothing.Finish smoothingFinish:
                return state with
                {
                    controlState = new ControlState.None()
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
        
        Dictionary<Vector3Int, VoxelData> newDrawnVoxels = new Dictionary<Vector3Int, VoxelData>();
        switch (patch)
        {
            case EditorPatch.VoxelsChanges.Add addPatch:
                newDrawnVoxels = addPatch.voxels;
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        voxels = new Dictionary<Vector3Int, VoxelData>(activeLayer.currentSpriteData.voxels).Plus(addPatch.voxels)
                    }
                };
                break;
            case EditorPatch.VoxelsChanges.ChangeSmoothState changeSmoothState:
            {
                var smoothVoxelsMap = changeSmoothState.smoothVoxelsMap;
                var voxels = new Dictionary<Vector3Int, VoxelData>(activeLayer.currentSpriteData.voxels);

                foreach (var (pos, isSmooth) in smoothVoxelsMap)
                {
                    voxels[pos] = voxels[pos] with
                    {
                        isSmooth = isSmooth
                    };
                }

                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        voxels = voxels
                    }
                };
                break;
            }
            case EditorPatch.VoxelsChanges.Delete deletePatch:
                newDrawnVoxels = deletePatch.voxels;
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        voxels = new Dictionary<Vector3Int, VoxelData>(activeLayer.currentSpriteData.voxels).Minus(deletePatch.voxels)
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        };
        
        var controlState = state.controlState;
        if (controlState is ControlState.Drawing drawingState)
        {
            var drawnVoxels = new Dictionary<Vector3Int, VoxelData>(drawingState.drawnVoxels);
            drawnVoxels.Plus(newDrawnVoxels);
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

    private EditorState ApplyPivotPointPatch(EditorPatch.PivotPoint patch)
    {
        if (state.activeLayer is not VoxLayerState.Loaded activeLayer) return state;

        var layers = new Dictionary<int, VoxLayerState>(state.layers);
        
        switch (patch)
        {
            case EditorPatch.PivotPoint.ApplyPivotPointForAll applyPivotPointForAll:
                var currentPivot = activeLayer.currentSpriteData.pivot;

                var sprites = new Dictionary<SpriteIndex, SpriteData>();

                foreach (var (index, sprite) in activeLayer.voxData.sprites)
                {
                    sprites[index] = sprite with
                    {
                        pivot = currentPivot
                    };
                }

                var voxData = activeLayer.voxData with
                {
                    sprites = sprites
                };
                
                layers[state.activeLayerKey] = activeLayer with
                {
                    voxData = voxData
                };
                
                return state with
                {
                    layers = layers
                };
            case EditorPatch.PivotPoint.NewPivotPoint newPivotPoint:
                layers[state.activeLayerKey] = activeLayer with
                {
                    currentSpriteData = activeLayer.currentSpriteData with
                    {
                        pivot = newPivotPoint.pivotPoint
                    }
                };

                return state with
                {
                    layers = layers
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
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