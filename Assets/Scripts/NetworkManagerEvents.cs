﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SharedTypes;
using SharedMessages;

public class NetworkManagerEvents : NetworkManager
{
    private ConfigSingleton _config;
    //private List<NetworkConnection> _connections;
    private void Start()
    {
        _config = ConfigSingleton.GetInstance();
        //_connections = new List<NetworkConnection>();
    }

    public override void OnStartServer()
    {
        Debug.Log($"Server started on port: {singleton.networkPort}");
        transform.GetComponent<MainLoop>().TestCaseSetup();
        NetworkServer.RegisterHandler(MyMsgType.TargetDatas, GetTargetDatas);
        NetworkServer.RegisterHandler(MyMsgType.TargetInfos, GetTargetInfos);
        NetworkServer.RegisterHandler(MyMsgType.DataRequest, DataBroker);
    }

    public void DataBroker(NetworkMessage message)
    {
        RequestMessage msg = message.ReadMessage<RequestMessage>();
        switch (msg.Type)
        {
            case RequestType.UserList:
                List<User> userList = new List<User>();
                NetworkServer.SendToClient(message.conn.connectionId, MyMsgType.UserList, new UserListMessage(userList));
                break;
        }
    }

    public void GetTargetDatas(NetworkMessage message)
    {

    }

    public void GetTargetInfos(NetworkMessage message)
    {

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
