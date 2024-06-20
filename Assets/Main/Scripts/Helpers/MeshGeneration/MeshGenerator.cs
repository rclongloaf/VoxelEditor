using UnityEngine;

namespace Main.Scripts.Helpers.MeshGeneration
{
public interface MeshGenerator
{
    public Mesh? GenerateMesh();
}
}