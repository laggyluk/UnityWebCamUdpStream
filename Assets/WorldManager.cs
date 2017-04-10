using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour {

    
    public Text roleText;
    public Slider grabIntervalSlider,sendIntervalSlider;
    public Dropdown chunksDropdown, modeDropdown, compressionDropdown;
    public GameObject configPanel;
    public RawImage serverTex, clientTex;
    //camera used for image source when no hardware camera exists
    public Camera dummyCamera;
    public static WorldManager Inst;
    public TextureFormat textureFormat = TextureFormat.RGB24;
    //app can be either client receiving video image or server sending it
    bool roleServer;
    GameServer server;
    GameClient client;
    
    void Awake()
    {
        Inst = this;
    }

	// Use this for initialization
	void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        server = GetComponent<GameServer>();
        client = GetComponent<GameClient>();
        //load old settings
        grabIntervalSlider.value = PlayerPrefs.GetFloat("grabInterval", 0.5f);
        modeDropdown.value = PlayerPrefs.GetInt("fragmentationMode", 0);
        chunksDropdown.value = PlayerPrefs.GetInt("chunksCount", 2);
        compressionDropdown.value = PlayerPrefs.GetInt("compressionMode", 0);
        sendIntervalSlider.value = PlayerPrefs.GetFloat("sendInterval", 0.1f);
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
            dummyCamera.gameObject.SetActive(false);
            serverTex.enabled = false;
            clientTex.enabled = true;
        }
        else
        {
            client.Shutdown();
            server.Init();
            roleText.text = "role: server";
            dummyCamera.gameObject.SetActive(true);
            serverTex.enabled = true;
            clientTex.enabled = false;
        }
        roleServer = !roleServer;
        //grabIntervalSlider.gameObject.SetActive(roleServer);
    }

    public void OnIntervalSliderChanged()
    {
        float val = grabIntervalSlider.value;
        DummyRender.grabIntervalSeconds = val;
        grabIntervalSlider.GetComponentInChildren<Text>().text = string.Format("frame grab interval: {0:0.00}(s)", val);
    }

    public void OnSendIntervalChange()
    {
        float val = sendIntervalSlider.value;
        GameServer.sendInterval = val;
        sendIntervalSlider.GetComponentInChildren<Text>().text = string.Format("send packets interval: {0:0.00}(s)", val);
    }    

    public void OnChunksPerFrameChanged()
    {
        GameServer.Inst.chunksEachFrame = int.Parse(chunksDropdown.captionText.text);
    }

    public void OnSendModeChanged()
    {
        GameServer.Inst.streamMode = (StreamMode)modeDropdown.value;
    }

    public void OnCompressionModeChanged()
    {
        GameServer.Inst.compression = (CompressionMode)compressionDropdown.value;
        
    }

    public void OnShowConfigBtnClick()
    {
        configPanel.SetActive(!configPanel.activeSelf);
    }

    private void OnApplicationQuit()
    {
        //save settings
        PlayerPrefs.SetInt("roleServer", (roleServer ? 1 : 0));
        PlayerPrefs.SetFloat("grabInterval", grabIntervalSlider.value);
        PlayerPrefs.SetInt("fragmentationMode", modeDropdown.value);
        PlayerPrefs.SetInt("chunksCount", chunksDropdown.value);
        PlayerPrefs.SetInt("compressionMode", compressionDropdown.value);
        PlayerPrefs.SetFloat("sendInterval", sendIntervalSlider.value);
    }
}
