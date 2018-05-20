using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManagerEvents : NetworkManager {
    public override void OnStartServer()
    {
        Debug.Log(string.Format("Server started on port: {0}", singleton.networkPort));
        transform.GetComponent<MainLoop>().TestCaseSetup();
    }
}
