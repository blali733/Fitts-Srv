using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

public class DBConfig
{
    public string Domain;
    public string DB;
    public string User;
    public string Password;
    public string Port;
}

public class MongoDBConnector
{
    private MongoClient _client;
    private MongoClient _resultClient;
    private static MongoDBConnector _instance;

    public static MongoDBConnector GetInstance()
    {
        return _instance ?? (_instance = new MongoDBConnector());
    }

    private MongoDBConnector()
    {
        var fileText = File.ReadAllText("./DB_config.json");
        var dbConfig = JsonUtility.FromJson<DBConfig>(fileText);
        _client = new MongoClient($"mongodb://{dbConfig.User}:{dbConfig.Password}@{dbConfig.Domain}:{dbConfig.Port}/{dbConfig.DB}");
    }

    public IMongoDatabase GetDatabase()
    {
        return _client.GetDatabase("fitts");
    }

    public IMongoDatabase GetResultsDatabase()
    {
        return _client.GetDatabase("fitts_results");
    }
}
