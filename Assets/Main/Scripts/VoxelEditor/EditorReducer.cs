using System;
using System.Collections.Generic;
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
            EditorPatch.SpriteChanges spriteChangesPatch => ApplySpriteChangesPatch(spriteChangesPatch),
            EditorPatch.FileBrowser fileBrowserPatch => ApplyFileBrowserPatch(fileBrowserPatch),
            EditorPatch.Import importPatch => ApplyImportPatch(importPatch),
            EditorPatch.ModelBuffer modelBufferPatch => ApplyModelBufferPatch(modelBufferPatch),
            EditorPatch.VoxLoaded voxLoadedPatch => ApplyVoxLoadedPatch(voxLoadedPatch),
            EditorPatch.TextureLoaded textureLoadedPatch => ApplyTextureLoadedPatch(textureLoadedPatch),
            EditorPatch.Control controlPatch => ApplyControlPatch(controlPatch),
            EditorPatch.VoxelsChanges voxelsChanges => ApplyVoxelsChangesPatch(voxelsChanges),
            EditorPatch.EditMode editModePatch => ApplyEditModePatch(editModePatch),
            EditorPatch.Brush.ChangeType brushPatch => ApplyBrushPatch(brushPatch),
            EditorPatch.Camera cameraPatch => ApplyCameraPatch(cameraPatch),
            EditorPatch.ChangeSpriteIndex changeSpriteIndexPatch => ApplyChangeSpriteIndex(changeSpriteIndexPatch),
            _ => throw new ArgumentOutOfRangeException(nameof(patch), patch, null)
        };

        feature.UpdateState(newState);
    }

    private EditorState ApplySpriteChangesPatch(EditorPatch.SpriteChanges patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        switch (patch)
        {
            case EditorPatch.SpriteChanges.Apply apply:
                var sprites = loadedState.voxData.sprites;
                var newSprites = new Dictionary<SpriteIndex, SpriteData>(sprites);

                newSprites[loadedState.currentSpriteIndex] = loadedState.currentSpriteData;
                
                return loadedState with
                {
                    voxData = loadedState.voxData with
                    {
                        sprites = newSprites
                    },
                    isWaitingForApplyChanges = false
                };
                break;
            case EditorPatch.SpriteChanges.ApplyRequest applyRequest:
                return loadedState with { isWaitingForApplyChanges = true };
                break;
            case EditorPatch.SpriteChanges.Cancel cancel:
                return loadedState with { isWaitingForApplyChanges = false };
                break;
            case EditorPatch.SpriteChanges.Discard discard:
                return loadedState with
                {
                    currentSpriteData = loadedState.voxData.sprites[loadedState.currentSpriteIndex],
                    isWaitingForApplyChanges = false
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyFileBrowserPatch(EditorPatch.FileBrowser patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState with
        {
            isFileBrowserOpened = patch is EditorPatch.FileBrowser.Opened
        };
    }

    private EditorState ApplyImportPatch(EditorPatch.Import patch)
    {
        switch (patch)
        {
            case EditorPatch.Import.TextureSelected textureSelected:
                return new EditorState.SpriteSelecting(
                    texture: textureSelected.texture,
                    isFileBrowserOpened: false
                );
                break;
            case EditorPatch.Import.Cancel cancel:
                return new EditorState.WaitingForProject(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyModelBufferPatch(EditorPatch.ModelBuffer patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return patch switch
        {
            EditorPatch.ModelBuffer.Copy copy => loadedState with { bufferedSpriteData = copy.spriteData },
            EditorPatch.ModelBuffer.Paste paste => loadedState with { currentSpriteData = paste.spriteData },
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
    }

    private EditorState ApplyVoxLoadedPatch(EditorPatch.VoxLoaded patch)
    {
        var texture = state switch
        {
            EditorState.Loaded loaded => loaded.texture,
            EditorState.SpriteSelecting spriteSelecting => spriteSelecting.texture,
            EditorState.WaitingForProject waitingForProject => null,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

        var spriteIndex = new SpriteIndex(0, 0);

        return new EditorState.Loaded(
            voxData: patch.voxData,
            texture: texture,
            currentSpriteIndex: spriteIndex,
            currentSpriteData: patch.voxData.sprites[spriteIndex],
            bufferedSpriteData: null,
            brushType: BrushType.Add,
            freeCameraData: new FreeCameraData(
                pivotPoint: new Vector3(14, 18, 0),
                distance: 30,
                rotation: default
            ),
            isometricCameraData: new IsometricCameraData(new Vector3(12, 48, -30)),
            cameraType: CameraType.Free,
            controlState: ControlState.None,
            editModeState: new EditModeState.EditMode(),
            isWaitingForApplyChanges: false,
            isFileBrowserOpened: false
        );
    }

    private EditorState ApplyTextureLoadedPatch(EditorPatch.TextureLoaded patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState with { texture = patch.texture };
    }

    private EditorState ApplyControlPatch(EditorPatch.Control patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return patch switch
        {
            EditorPatch.Control.Drawing.Start => loadedState with { controlState = ControlState.Drawing },
            EditorPatch.Control.Drawing.Finish => loadedState with { controlState = ControlState.None },
            EditorPatch.Control.Moving.Start => loadedState with { controlState = ControlState.Moving },
            EditorPatch.Control.Moving.Finish => loadedState with { controlState = ControlState.None },
            EditorPatch.Control.Rotating.Start => loadedState with { controlState = ControlState.Rotating },
            EditorPatch.Control.Rotating.Finish => loadedState with { controlState = ControlState.None },

            _ => throw new ArgumentOutOfRangeException(nameof(patch), patch, null)
        };
    }

    private EditorState ApplyVoxelsChangesPatch(EditorPatch.VoxelsChanges patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return patch switch
        {
            EditorPatch.VoxelsChanges.Add addPatch => loadedState with
            {
                currentSpriteData = loadedState.currentSpriteData with
                {
                    voxels = new HashSet<Vector3Int>(loadedState.currentSpriteData.voxels).Plus(addPatch.voxel)
                }
            },
            EditorPatch.VoxelsChanges.Delete deletePatch => loadedState with
            {
                currentSpriteData = loadedState.currentSpriteData with
                {
                    voxels = new HashSet<Vector3Int>(loadedState.currentSpriteData.voxels).Minus(deletePatch.voxel)
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(patch))
        };
    }

    private EditorState ApplyEditModePatch(EditorPatch.EditMode patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        switch (patch)
        {
            case EditorPatch.EditMode.EditModeSelected editModeSelected:
                if (loadedState.editModeState is EditModeState.EditMode) return state;
                return loadedState with
                {
                    editModeState = new EditModeState.EditMode()
                };
                break;
            case EditorPatch.EditMode.RenderModeSelected renderModeSelected:
                if (loadedState.editModeState is EditModeState.RenderMode) return state;
                return loadedState with
                {
                    editModeState = new EditModeState.RenderMode(renderModeSelected.mesh)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyBrushPatch(EditorPatch.Brush.ChangeType patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState with
        {
            brushType = patch.brushType
        };
    }

    private EditorState ApplyCameraPatch(EditorPatch.Camera patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        switch (patch)
        {
            case EditorPatch.Camera.NewPivotPoint newPosition:
                if (loadedState.cameraType is CameraType.Free)
                {
                    return loadedState with
                    {
                        freeCameraData = loadedState.freeCameraData with
                        {
                            pivotPoint = newPosition.position
                        }
                    };
                }

                return loadedState with
                {
                    isometricCameraData = new IsometricCameraData(newPosition.position)
                };
                break;
            case EditorPatch.Camera.NewDistance newDistance:
                if (loadedState.cameraType is CameraType.Free)
                {
                    return loadedState with
                    {
                        freeCameraData = loadedState.freeCameraData with
                        {
                            distance = newDistance.distance
                        }
                    };
                }

                return loadedState;
                break;
            case EditorPatch.Camera.NewRotation newRotation:
                return loadedState with
                {
                    freeCameraData = loadedState.freeCameraData with
                    {
                        rotation = newRotation.rotation
                    }
                };
                break;
            case EditorPatch.Camera.ChangeType changeType:
                return loadedState with
                {
                    cameraType = changeType.cameraType
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(patch));
        }
    }

    private EditorState ApplyChangeSpriteIndex(EditorPatch.ChangeSpriteIndex patch)
    {
        if (state is not EditorState.Loaded loadedState) return state;

        return loadedState with
        {
            currentSpriteData = loadedState.voxData.sprites[patch.spriteIndex],
            currentSpriteIndex = patch.spriteIndex
        };
    }
}
}