using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine;
using UnityEngine.Networking;
using FittsLibrary;
using FittsLibrary.Messages;
using MathNet.Numerics;
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
        var devices = (user.Results.None.ToInt() << 3) +
                      (user.Results.Smaller5.ToInt() << 2) +
                      (user.Results.Smaller11.ToInt() << 1) +
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
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "UserCount");
        var update = Builders<BsonDocument>.Update.Inc("Count", 1);
        resultDb.GetCollection<BsonDocument>("counters").FindOneAndUpdate(filter, update);
        NetworkServer.SendToClient(message.conn.connectionId, MyMsgType.UserCode, new StringMessage(user.Code));
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
                foreach (var item in collection.Find(new BsonDocument{{"TestGroup", _config.TestGroup}}).Project(Builders<BsonDocument>.Projection.Exclude("_id").Include("Name").Include("Code")).ToList())
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
            {"UserCode", user},
            {"TargetDatas", arr}
        };
        collection.InsertOne(document);
    }

    private Tuple<double, double> LinReg(double[] xData, double[] yData)
    {
        if (xData.Length == yData.Length)
            
            return Fit.Line(xData, yData);
        else
        {
            throw new DataMisalignedException();
        }
    }

    public void GotTargetInfos(NetworkMessage message)
    {
        TargetInfosMessage msg = message.ReadMessage<TargetInfosMessage>();
        List<List<TargetInfo>> targetInfos = msg.Content.TargetInfos;
        string user = msg.Content.User;
        var db = MongoDBConnector.GetInstance().GetDatabase();
        var collection = db.GetCollection<BsonDocument>("TargetInfos");
        BsonArray arr = new BsonArray();
        UserResultSet userResultSet = new UserResultSet();
        userResultSet.UID = user;
        userResultSet.DID = msg.Content.DevId;
        double prevColorDiff;
        int i = 0;
        foreach (var list in targetInfos)
        {
            prevColorDiff = list.First().ColorDistance;
            BsonArray innerArr = new BsonArray();
            TargetSet targetSet = new TargetSet();
            List<double> xData = new List<double>();
            List<double> yData = new List<double>();
            Tuple<double, double> result;
            int pos;
            foreach (var targetInfo in list)
            {
                Debug.Log($"Colour differences {prevColorDiff} to {targetInfo.ColorDistance}");
                if (Math.Abs(prevColorDiff - targetInfo.ColorDistance) > 1.0)
                {
                    // Reset line set:
                    targetSet.TestCase = _config.TestCases[i]; 
                    targetSet.ColourDifference = prevColorDiff;
                    pos = IsImportantError(xData);
                    while (pos != -1)
                    {
                        xData.RemoveAt(pos);
                        yData.RemoveAt(pos);
                        pos = IsImportantError(xData);
                    }
                    result = LinReg(xData.ToArray(), yData.ToArray());
                    targetSet.ParameterA = result.Item2;
                    targetSet.ParameterB = result.Item1;
                    userResultSet.TargetSets.Add(targetSet);
                    targetSet = new TargetSet();
                    xData.Clear();
                    yData.Clear();
                }
                prevColorDiff = targetInfo.ColorDistance;
                targetSet.TargetPoints.Add(new TargetPoint(targetInfo.Duration.TotalSeconds, 
                    Math.Log(2*targetInfo.PixelDistance/targetInfo.PixelSize, 2)));
                xData.Add(targetInfo.Duration.TotalSeconds);
                yData.Add(Math.Log(2*targetInfo.PixelDistance/targetInfo.PixelSize, 2));
                innerArr.Add(new BsonDocument(targetInfo.ToBsonDocument()));
            }
            arr.Add(innerArr);
            targetSet.TestCase = _config.TestCases[i]; 
            targetSet.ColourDifference = prevColorDiff;
            pos = IsImportantError(xData);
            while (pos != -1)
            {
                xData.RemoveAt(pos);
                yData.RemoveAt(pos);
                pos = IsImportantError(xData);
            }
            result = LinReg(xData.ToArray(), yData.ToArray());
            targetSet.ParameterA = result.Item2;
            targetSet.ParameterB = result.Item1;
            userResultSet.TargetSets.Add(targetSet);
            i++;
        }
        BsonDocument document = new BsonDocument
        {
            {"UserCode", user},
            {"TargetInfos", arr}
        };
        collection.InsertOne(document);
        var resultDb = MongoDBConnector.GetInstance().GetResultsDatabase();
        resultDb.GetCollection<BsonDocument>("Results").InsertOne(userResultSet.ToBsonDocument());
    }

    private int IsImportantError(List<double> list)
    {
        double offset = 1/(double)list.Count;
        var avg = Avg(list);
        var idMax = list.IndexOf(list.Max());
        var nlist = new List<double>(list);
        nlist.RemoveAt(idMax);
        var navg = Avg(nlist);
        if ((1 - (navg / avg)) > offset)
        {
            return idMax;
        }
        return -1;
    }

    private double Avg(List<double> list)
    {
        double value = 0.0;
        foreach (var v in list)
        {
            value += v;
        }
        return value / list.Count;
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log($"Client with id {conn.connectionId}, from {conn.address} has connected.");
        TestCasesMessage msg = new TestCasesMessage(_config.TestCases);
        NetworkServer.SendToClient(conn.connectionId, MyMsgType.TestCases, msg);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log($"Client with id {conn.connectionId} has disconnected.");
    }
}
