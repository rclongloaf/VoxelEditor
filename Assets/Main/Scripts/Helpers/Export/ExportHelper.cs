using System.Text;
using UnityEngine;

namespace Main.Scripts.Helpers.Export
{
public static class ExportHelper
{
    public static void ExportMeshAsObj(Mesh mesh, string path)
    {
        var sb = new StringBuilder();

        foreach (var v in mesh.vertices)
        {
            sb.AppendLine($"v {-v.x} {v.y} {v.z}");
        }

        foreach (var v in mesh.uv)
        {
            sb.AppendLine($"vt {v.x} {v.y}");
        }

        foreach (var v in mesh.normals)
        {
            sb.AppendLine($"vn {v.x} {v.y} {v.z}");
        }
        
        var triangles = mesh.triangles;

        for (var i = 0; i < triangles.Length; i += 3)
        {
            sb.AppendLine($"f {ConstructOBJString(triangles[i + 2] + 1)} {ConstructOBJString(triangles[i + 1] + 1)} {ConstructOBJString(triangles[i] + 1)}");
        }

        System.IO.File.WriteAllText(path, sb.ToString());
    }
    
    private static string ConstructOBJString(int index)
    {
        var idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }
}
}