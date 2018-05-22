using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

public class MongoDBConnector : MonoBehaviour
{
    private MongoClient _client;
    private static MongoDBConnector _instance;

    public static MongoDBConnector GetInstance()
    {
        return _instance ?? (_instance = new MongoDBConnector());
    }

    private MongoDBConnector()
    {
        _client = new MongoClient("mongodb://192.168.1.11:27017");
    }

    public IMongoDatabase GetDatabase()
    {
        return _client.GetDatabase("fitts");
    }
}
