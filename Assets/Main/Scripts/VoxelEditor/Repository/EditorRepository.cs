using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.Repository
{
public class EditorRepository
{
    public bool LoadVoxFile(string path, out HashSet<Vector3Int> voxels)
    {
        voxels = new HashSet<Vector3Int>();
        if (!File.Exists(path))
        {
            return false;
        }
        
        using var streamReader = File.OpenText(path);
        using var jsonReader = new JsonTextReader(streamReader);

        var jObject = (JObject)JToken.ReadFrom(jsonReader);

        var voxelsList = (JArray)jObject.GetValue("data");
        foreach (var jVoxelData in voxelsList)
        {
            var x = (int)((JObject)jVoxelData).GetValue("x");
            var y = (int)((JObject)jVoxelData).GetValue("y");
            var z = (int)((JObject)jVoxelData).GetValue("z");
            voxels.Add(new Vector3Int(x, y, z));
        }

        return true;
    }
}
}