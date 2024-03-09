using System;

namespace Main.Scripts.Helpers.MeshGeneration
{
[Flags]
public enum Normal
{
    Up = 1,
    Down = 2,
    Forward = 4,
    Back = 8,
    Left = 16,
    Right = 32
}
}