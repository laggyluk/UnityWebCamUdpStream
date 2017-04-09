using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour {

    public Text roleText;

    //app can be either client receiving video image or server sending it
    bool roleServer;
    GameServer server;
    GameClient client;
    
	// Use this for initialization
	void Start ()
    {
        server = GetComponent<GameServer>();
        client = GetComponent<GameClient>();
        //read default role
        roleServer = !(PlayerPrefs.GetInt("roleServer", 0) > 0);
        SwitchRole();
    }
	
    public void SwitchRole()
    {
        if (roleServer)
        {
            server.Shutdown();
            client.Init();
            roleText.text = "role: client";
        }
        else
        {
            client.Shutdown();
            server.Init();
            roleText.text = "role: server";
        }
        roleServer = !roleServer;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("roleServer", (roleServer ? 1 : 0));
    }
}
