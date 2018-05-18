using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTypes;

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton _instance;

    //Variables:
    private MyNetworkConfig _networkConfig;
    public string DBDomain { get; set; }
    
    //Instance getter
    public static ConfigSingleton GetInstance()
    {
        return _instance ?? (_instance = new ConfigSingleton());
    }

    public MyNetworkConfig GetMyNetworkConfig()
    {
        return this._networkConfig;
    }

    public void SetMyNetworkConfig(MyNetworkConfig networkConfig)
    {
        _networkConfig = networkConfig;
    }
}
