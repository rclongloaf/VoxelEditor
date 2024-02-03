using Main.Scripts.VoxelEditor.State.Vox;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.EditModes.Render
{
public record RenderData(
    SpriteData spriteData,
    Mesh mesh,
    Texture2D? texture
);
}