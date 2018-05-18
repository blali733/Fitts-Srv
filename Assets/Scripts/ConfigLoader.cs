using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using SharedTypes;
using UnityEngine.Networking;

[Serializable]
public class ConfigContent
{
    public string ServerIp;
    public string ServerPort;

    public ConfigContent() { }

    public ConfigContent(string serverIp, string serverPort)
    {
        ServerIp = serverIp;
        ServerPort = serverPort;
    }
}

public class ConfigLoader : MonoBehaviour
{

    // Use this for initialization
    private void Start()
    {
        string config = LoadFile();
        ConfigContent configuration = JsonUtility.FromJson<ConfigContent>(config);
        ConfigSingleton configInstance = ConfigSingleton.GetInstance();
        configInstance.SetMyNetworkConfig(new MyNetworkConfig(configuration.ServerIp, configuration.ServerPort));
        NetworkManager.singleton.networkPort = int.Parse(configuration.ServerPort);
        NetworkManager.singleton.StartServer();
        Debug.Log(string.Format("Server started on port: {0}",NetworkManager.singleton.networkPort));
    }

    private string LoadFile()
    {
        return File.ReadAllText("./server_config.json");
    }
}
