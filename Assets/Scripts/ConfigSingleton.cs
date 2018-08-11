using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using UnityEngine;
using FittsLibrary;

//Color space data structure
[Serializable]
public class ColorSpaceContainer
{
    public List<float> Labels;
    public List<List<List<float>>> Payload;
}

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton _instance;
    public string TestGroup { get; set; }
    public ColorSpaceContainer ColorSpaceContainer { get; }
    public List<ColorRange> ColorRanges { get; }

    public List<TestCase> TestCases { get; }

    //Variables:
    private MyNetworkConfig _networkConfig;
    public string DBDomain { get; set; }
    
    //Instance getter
    public static ConfigSingleton GetInstance()
    {
        return _instance ?? (_instance = new ConfigSingleton());
    }

    private ConfigSingleton()
    {
        MongoDBConnector conn = MongoDBConnector.GetInstance();
        TestCases = new List<TestCase>();
        var db = conn.GetDatabase();
        var colorSpace = JsonUtility.FromJson<ConfigContent>(File.ReadAllText("./server_config.json")).ColorSpace;
        var collection = db.GetCollection<BsonDocument>("testCases");
        foreach (var item in collection.Find(new BsonDocument()).Project(Builders<BsonDocument>.Projection.Exclude("_id")).ToList())
        {
            var jsonString = item.ToJson();
            TestCases.Add(JsonUtility.FromJson<TestCase>(jsonString));
        }
        collection = db.GetCollection<BsonDocument>("ColorSpaces");
        var colorSpaceRaw = collection.Find(new BsonDocument{{"Name", colorSpace}}).Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude("Name")).First();
        ColorSpaceContainer = JsonConvert.DeserializeObject<ColorSpaceContainer>(colorSpaceRaw.ToJson());
        // Parsing ColorSpaceContainer to list of color ranges:
        ColorRanges = new List<ColorRange>();
        int maax = ColorSpaceContainer.Labels.Count;
        for (int i = 0; i < maax; i++)
        {
            ColorRanges.Add(new ColorRange(ColorSpaceContainer.Labels[i], ColorSpaceContainer.Payload[i]));
        }
    }

    public MyNetworkConfig GetMyNetworkConfig()
    {
        return this._networkConfig;
    }

    public void SetMyNetworkConfig(MyNetworkConfig networkConfig)
    {
        _networkConfig = networkConfig;
    }
}
