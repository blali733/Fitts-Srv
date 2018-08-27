using System.Collections;
using System.Collections.Generic;
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine;
using UnityEngine.Networking;
using FittsLibrary;
using FittsLibrary.Messages;
using UnityEngine.Networking.NetworkSystem;

public class NetworkManagerEvents : NetworkManager
{
    private ConfigSingleton _config;

    private System.Random _rng;
    private void Start()
    {
        _config = ConfigSingleton.GetInstance();
        _rng = new System.Random();
    }

    public override void OnStartServer()
    {
        Debug.Log($"Server started on port: {singleton.networkPort}");
        transform.GetComponent<MainLoop>().TestCaseSetup();
        NetworkServer.RegisterHandler(MyMsgType.TargetDatas, GotTargetDatas);
        NetworkServer.RegisterHandler(MyMsgType.TargetInfos, GotTargetInfos);
        NetworkServer.RegisterHandler(MyMsgType.DataRequest, DataBroker);
        NetworkServer.RegisterHandler(MyMsgType.NewUserData, AddUser);
        NetworkServer.RegisterHandler(MyMsgType.DeviceData, CheckDevice);
    }

    public void CheckDevice(NetworkMessage message)
    {
        DeviceDataMessage msg = message.ReadMessage<DeviceDataMessage>();
        DeviceIdentification deviceIdentification = msg.DeviceIdentification;
        var db = MongoDBConnector.GetInstance().GetDatabase();
        var collection = db.GetCollection<BsonDocument>("Devices");
        var results = collection.Find(new BsonDocument {{"DevId", deviceIdentification.DevId}}).ToList();
        int addr = -1;
        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                if (result["ScreenWidth"].AsInt32 == deviceIdentification.ScreenWidth &&
                    result["ScreenHeight"].AsInt32 == deviceIdentification.ScreenHeight &&
                    result["DeviceClass"].AsInt32 == (int)deviceIdentification.DeviceClass)
                {
                    addr = result["id"].AsInt32;
                }
            }
        }
        if (addr == -1)
        {
            var value = collection.Find(new BsonDocument()).Sort(new BsonDocument {{"id", -1}}).Limit(1).Project(Builders<BsonDocument>.Projection.Exclude("_id").Include("id"));
            int maxid;
            if (value.Count() > 0)
            {
                maxid = JsonUtility.FromJson<int>(value.ToJson());
            }
            else
            {
                maxid = 0;
            }
            var document = new BsonDocument
            {
                {"id", maxid + 1},
                {"DevId", deviceIdentification.DevId},
                {"ScreenWidth", deviceIdentification.ScreenWidth},
                {"ScreenHeight", deviceIdentification.ScreenHeight},
                {"DeviceClass", deviceIdentification.DeviceClass}
            };
            collection.InsertOne(document);
            addr = maxid;
        }
        NetworkServer.SendToClient(message.conn.connectionId, MyMsgType.DeviceId, new IntegerMessage(addr));
    }

    public void AddUser(NetworkMessage message)
    {
        StoredUserMessage msg = message.ReadMessage<StoredUserMessage>();
        StoredUser user = msg.User;
        user.TestGroup = _config.TestGroup;
        //Generate random code:
        string code;
        var resultDb = MongoDBConnector.GetInstance().GetResultsDatabase();
        var registeredUsersCollection = resultDb.GetCollection<BsonDocument>("Users");
        int count;
        do
        {
            code = GenRandomCode();
            count = registeredUsersCollection.Find(new BsonDocument {{"_id", code}}).Limit(1).ToList().Count;
        } while (count != 0);
        user.Code = code;
        var db = MongoDBConnector.GetInstance().GetDatabase();
        var collection = db.GetCollection<BsonDocument>("Users");
        var document = new BsonDocument
        {
            {"Name", user.Name},
            {"TestGroup", user.TestGroup},
            {"Code", user.Code },
            {"Questionarie", new BsonDocument{
                    {"AgeGroup", user.Results.AgeGroup},
                    {"TouchFrequency", user.Results.TouchFrequency},
                    {"None", user.Results.None},
                    {"Smaller5", user.Results.Smaller5},
                    {"Smaller11", user.Results.Smaller11},
                    {"Greater11", user.Results.Greater11},
                    {"Activities", user.Results.Activities},
                    {"ColorPerception", user.Results.ColorPerception}
                }
            }
        };
        collection.InsertOne(document);
        var devices = user.Results.None.ToInt() << 3 +
                      user.Results.Smaller5.ToInt() << 2 +
                      user.Results.Smaller11.ToInt() << 1 +
                      user.Results.Greater11.ToInt();
        document = new BsonDocument
        {
            {"_id", code},
            {"AgeGroup", user.Results.AgeGroup},
            {"ColorPerception", user.Results.ColorPerception},
            {"TouchFrequency", user.Results.TouchFrequency},
            {"Activities", user.Results.Activities},
            {"Devices", devices}
        };
        registeredUsersCollection.InsertOne(document);
    }

    public string GenRandomCode()
    {
        var chars = "";
        chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        chars += "abcdefghijklmnopqrstuvwxyz";
        chars += "0123456789";
        var stringChars = new char[8];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[_rng.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    public void DataBroker(NetworkMessage message)
    {
        RequestMessage msg = message.ReadMessage<RequestMessage>();
        switch (msg.Type)
        {
            case RequestType.UserList:
                List<User> userList = new List<User>();
                var db = MongoDBConnector.GetInstance().GetDatabase();
                var collection = db.GetCollection<BsonDocument>("Users");
                foreach (var item in collection.Find(new BsonDocument{{"TestGroup", _config.TestGroup}}).Project(Builders<BsonDocument>.Projection.Exclude("_id").Include("Name")).ToList())
                {
                    var jsonString = item.ToJson();
                    userList.Add(JsonUtility.FromJson<User>(jsonString));
                }
                NetworkServer.SendToClient(message.conn.connectionId, MyMsgType.UserList, new UserListMessage(userList));
                break;
            case RequestType.ColorRanges:
                NetworkServer.SendToClient(message.conn.connectionId, MyMsgType.ColorRanges, new ColorRangesMessage(_config.ColorRanges));
                break;
        }
    }

    public void GotTargetDatas(NetworkMessage message)
    {
        RawTargetDatasMessage msg = message.ReadMessage<RawTargetDatasMessage>();
        List<List<TargetData>> targetDatas = msg.Content.TargetDatas;
        string user = msg.Content.User;
        var db = MongoDBConnector.GetInstance().GetDatabase();
        var collection = db.GetCollection<BsonDocument>("TargetDatas");
        var userCollection = db.GetCollection<BsonDocument>("Users");
        BsonArray arr = new BsonArray();
        foreach (var list in targetDatas)
        {
            BsonArray innerArr = new BsonArray();
            foreach (var targetData in list)
            {
                innerArr.Add(new BsonDocument(targetData.ToBsonDocument()));
            }
            arr.Add(innerArr);
        }
        BsonDocument document = new BsonDocument
        {
            {"UserName", user},
            {"TargetDatas", arr}
        };
        collection.InsertOne(document);
    }

    public void GotTargetInfos(NetworkMessage message)
    {
        TargetInfosMessage msg = message.ReadMessage<TargetInfosMessage>();
        List<List<TargetInfo>> targetInfos = msg.Content.TargetInfos;
        string user = msg.Content.User;
        var db = MongoDBConnector.GetInstance().GetDatabase();
        var collection = db.GetCollection<BsonDocument>("TargetInfos");
        var userCollection = db.GetCollection<BsonDocument>("Users");
        BsonArray arr = new BsonArray();
        foreach (var list in targetInfos)
        {
            BsonArray innerArr = new BsonArray();
            foreach (var targetInfo in list)
            {
                innerArr.Add(new BsonDocument(targetInfo.ToBsonDocument()));
            }
            arr.Add(innerArr);
        }
        BsonDocument document = new BsonDocument
        {
            {"UserName", user},
            {"TargetInfos", arr}
        };
        collection.InsertOne(document);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log($"Client with id {conn.connectionId}, from {conn.address} has connected.");
        //_connections.Add(conn);
        TestCasesMessage msg = new TestCasesMessage(_config.TestCases);
        NetworkServer.SendToClient(conn.connectionId, MyMsgType.TestCases, msg);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log($"Client with id {conn.connectionId} has disconnected.");
    }
}
