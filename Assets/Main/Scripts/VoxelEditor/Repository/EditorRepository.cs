using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Scripts.Helpers;
using Main.Scripts.Helpers.Export;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.State.Vox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Main.Scripts.VoxelEditor.Repository
{
public class EditorRepository
{
    private const string KEY_VERSION = "version";
    private const string KEY_TEXTURE = "texture";
    private const string KEY_COLUMNS_COUNT = "columns_count";
    private const string KEY_ROWS_COUNT = "rows_count";
    private const string KEY_SPRITES = "sprites";
    private const string KEY_ROW_INDEX = "row_index";
    private const string KEY_COLUMN_INDEX = "column_index";
    private const string KEY_PIVOT = "pivot";
    private const string KEY_VOXELS = "voxels";
    private const string KEY_X = "x";
    private const string KEY_Y = "y";
    private const string KEY_Z = "z";
    
    private const int VERSION = 1;
    
    public VoxData? LoadVoxFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var sprites = new Dictionary<SpriteIndex, SpriteData>();
        
        using var streamReader = File.OpenText(path);
        using var jsonReader = new JsonTextReader(streamReader);

        var jObject = (JObject)JToken.ReadFrom(jsonReader);
        
        var jTexture = (JObject)jObject.GetValue(KEY_TEXTURE);
        var textureData = new TextureData(
            rowsCount: (int)jTexture.GetValue(KEY_ROWS_COUNT),
            columnsCount: (int)jTexture.GetValue(KEY_COLUMNS_COUNT)
        );

        var jSprites = (JArray)jObject.GetValue(KEY_SPRITES);

        foreach (var jSprite in jSprites.Cast<JObject>())
        {
            var rowIndex = (int)jSprite.GetValue(KEY_ROW_INDEX);
            var columnIndex = (int)jSprite.GetValue(KEY_COLUMN_INDEX);
            
            var spriteIndex = new SpriteIndex(rowIndex, columnIndex);

            var jPivot = (JObject)jSprite.GetValue(KEY_PIVOT);
            var pivot = new Vector2(
                x: (float)jPivot.GetValue(KEY_X),
                y: (float)jPivot.GetValue(KEY_Y)
            );

            var jVoxels = (JArray)jSprite.GetValue(KEY_VOXELS);
            var voxels = new HashSet<Vector3Int>();

            foreach (var jVoxel in jVoxels.Cast<JObject>())
            {
                var x = (int)jVoxel.GetValue("x");
                var y = (int)jVoxel.GetValue("y");
                var z = (int)jVoxel.GetValue("z");
                voxels.Add(new Vector3Int(x, y, z));
            }
            sprites.Add(spriteIndex, new SpriteData(pivot, voxels));
        }

        return new VoxData(
            textureData: textureData,
            sprites: sprites
        );
    }

    public void SaveVoxFile(string path, VoxData voxData)
    {
        var jObject = new JObject();

        jObject.Add(KEY_VERSION, VERSION);
        
        var jTexture = new JObject();
        jTexture.Add(KEY_ROWS_COUNT, voxData.textureData.rowsCount);
        jTexture.Add(KEY_COLUMNS_COUNT, voxData.textureData.columnsCount);
        jObject.Add(KEY_TEXTURE, jTexture);

        var jSprites = new JArray();
        foreach (var (spriteIndex, spriteData) in voxData.sprites)
        {
            var jSprite = new JObject();
            
            jSprite.Add(KEY_ROW_INDEX, spriteIndex.rowIndex);
            jSprite.Add(KEY_COLUMN_INDEX, spriteIndex.columnIndex);

            var jPivot = new JObject();
            jPivot.Add(KEY_X, spriteData.pivot.x);
            jPivot.Add(KEY_Y, spriteData.pivot.y);
            jSprite.Add(KEY_PIVOT, jPivot);
            
            var voxelsList = new JArray();
            foreach (var voxelPosition in spriteData.voxels)
            {
                var jPos = new JObject();
                jPos.Add("x", voxelPosition.x);
                jPos.Add("y", voxelPosition.y);
                jPos.Add("z", voxelPosition.z);
                voxelsList.Add(jPos);
            }
            jSprite.Add(KEY_VOXELS, voxelsList);
            
            jSprites.Add(jSprite);
        }
        jObject.Add(KEY_SPRITES, jSprites);

        using var streamWriter = File.CreateText($"{path.Split('.')[0]}.json");
        using var jsonWriter = new JsonTextWriter(streamWriter);
        jObject.WriteTo(jsonWriter);
    }

    public void ExportMesh(
        string path,
        HashSet<Vector3Int> voxels,
        int textureWidth,
        int textureHeight,
        TextureData textureData,
        SpriteIndex spriteIndex,
        Vector2 pivotPoint,
        float pixelsPerUnit
    )
    {
        var meshGenerator = new MeshFromVoxelsGenerator(
            voxels,
            textureWidth,
            textureHeight,
            textureData,
            spriteIndex,
            pivotPoint,
            pixelsPerUnit
        );

        var mesh = meshGenerator.GenerateMesh();

        if (mesh != null)
        {
            ExportHelper.ExportMeshAsObj(mesh, $"{path.Split('.')[0]}.obj");
        }
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

    public Mesh GenerateMesh(
        SpriteData spriteData,
        float pixelsPerUnit,
        TextureData textureData,
        SpriteIndex spriteIndex,
        int textureWidth,
        int textureHeight
    )
    {
        return MeshGenerationHelper.GenerateMesh(
            spriteData: spriteData,
            pixelsPerUnit: pixelsPerUnit,
            textureData: textureData,
            spriteIndex: spriteIndex,
            textureWidth: textureWidth,
            textureHeight: textureHeight
        );
    }
}
}