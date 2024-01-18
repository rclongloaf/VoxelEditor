using System.Collections.Generic;
using System.IO;
using Main.Scripts.VoxelEditor.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.Repository
{
public class EditorRepository
{
    public VoxData? LoadVoxFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var voxels = new HashSet<Vector3Int>();
        
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

        var rowsCount = (int)jObject.GetValue("rows_count");
        var columnsCount = (int)jObject.GetValue("columns_count");
        var rowIndex = (int)jObject.GetValue("row_index");
        var columnIndex = (int)jObject.GetValue("column_index");

        return new VoxData(
            voxels: voxels,
            spriteRectData: new SpriteRectData(
                rowsCount: rowsCount,
                columnsCount: columnsCount,
                rowIndex: rowIndex,
                columnIndex: columnIndex
            )
        );
    }

    public void SaveVoxFile(string path, HashSet<Vector3Int> voxels, SpriteRectData spriteRectData)
    {
        var jObject = new JObject();
        var voxelsList = new JArray();

        foreach (var voxelPosition in voxels)
        {
            var jPos = new JObject();
            jPos.Add("x", voxelPosition.x);
            jPos.Add("y", voxelPosition.y);
            jPos.Add("z", voxelPosition.z);
            voxelsList.Add(jPos);
        }

        jObject.Add("version", 1);
        jObject.Add("data", voxelsList);
                
        jObject.Add("rows_count", spriteRectData.rowsCount);
        jObject.Add("columns_count", spriteRectData.columnsCount);
        jObject.Add("row_index", spriteRectData.rowIndex);
        jObject.Add("column_index", spriteRectData.columnIndex);

        var spriteIndex = spriteRectData.rowIndex * spriteRectData.columnsCount + spriteRectData.columnIndex;

        using var streamWriter = File.CreateText($"{path}_{spriteIndex}_vox.json");
        using var jsonWriter = new JsonTextWriter(streamWriter);
        jObject.WriteTo(jsonWriter);
    }

    public Texture2D? LoadTexture(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var image = File.ReadAllBytes(path);

        var texture = new Texture2D(2, 2);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(image);

        return texture;
    }
}
}