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
    public string ResultDB;
    public string User;
    public string Password;
    public string Port;
}

public class MongoDBConnector
{
    private MongoClient _client;
    private MongoClient _resultClient;
    private static MongoDBConnector _instance;
    private DBConfig _dbConfig;

    public static MongoDBConnector GetInstance()
    {
        return _instance ?? (_instance = new MongoDBConnector());
    }

    private MongoDBConnector()
    {
        var fileText = File.ReadAllText("./DB_config.json");
        _dbConfig = JsonUtility.FromJson<DBConfig>(fileText);
        _client = new MongoClient($"mongodb://{_dbConfig.User}:{_dbConfig.Password}@{_dbConfig.Domain}:{_dbConfig.Port}/{_dbConfig.DB}");
    }

    public IMongoDatabase GetDatabase()
    {
        return _client.GetDatabase(_dbConfig.DB);
    }

    public IMongoDatabase GetResultsDatabase()
    {
        return _client.GetDatabase(_dbConfig.ResultDB);
    }

    public IMongoDatabase GetResultsDatabase()
    {
        return _client.GetDatabase("fitts_results");
    }
}
