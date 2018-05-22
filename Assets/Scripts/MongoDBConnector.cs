using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

public class MongoDBConnector
{
    private MongoClient _client;
    private static MongoDBConnector _instance;

    public static MongoDBConnector GetInstance()
    {
        return _instance ?? (_instance = new MongoDBConnector());
    }

    private MongoDBConnector()
    {
        var address = JsonUtility.FromJson<ConfigContent>(File.ReadAllText("./server_config.json")).DBDomain;
        _client = new MongoClient($"mongodb://{address}:27017");
    }

    public IMongoDatabase GetDatabase()
    {
        return _client.GetDatabase("fitts");
    }
}
