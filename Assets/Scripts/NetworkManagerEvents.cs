using System.Collections;
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
        Debug.Log(string.Format("Server started on port: {0}", singleton.networkPort));
        transform.GetComponent<MainLoop>().TestCaseSetup();
        NetworkServer.RegisterHandler(MyMsgType.TargetDatas, GetTargetDatas);
        NetworkServer.RegisterHandler(MyMsgType.TargetInfos, GetTargetInfos);
    }

    public void GetTargetDatas(NetworkMessage message)
    {

    }

    public void GetTargetInfos(NetworkMessage message)
    {

    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log(string.Format("Client with id {0}, from {1} has connected.", conn.connectionId, conn.address));
        //_connections.Add(conn);
        TestCasesMessage msg = new TestCasesMessage(_config.TestCases);
        NetworkServer.SendToClient(conn.connectionId, MyMsgType.TestCases, msg);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log(string.Format("Client with id {0} has disconnected.", conn.connectionId));
    }
}
