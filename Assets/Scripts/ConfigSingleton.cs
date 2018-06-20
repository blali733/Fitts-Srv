using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine;
using SharedTypes;

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton _instance;
    private List<TestCase> _testCases;
    public string TestGroup { get; set; }

    public List<TestCase> TestCases
    {
        get { return _testCases; }
    }

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
        _testCases = new List<TestCase>();
        var db = conn.GetDatabase();
        var collection = db.GetCollection<BsonDocument>("testCases");
        foreach (var item in collection.Find(new BsonDocument()).Project(Builders<BsonDocument>.Projection.Exclude("_id")).ToList())
        {
            var jsonString = item.ToJson();
            _testCases.Add(JsonUtility.FromJson<TestCase>(jsonString));
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
