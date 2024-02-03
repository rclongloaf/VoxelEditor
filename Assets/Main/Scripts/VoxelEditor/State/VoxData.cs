using System.Collections.Generic;
using Main.Scripts.VoxelEditor.State.Vox;

namespace Main.Scripts.VoxelEditor.State
{
public record VoxData(
    TextureData textureData,
    Dictionary<SpriteIndex, SpriteData> sprites
);
}