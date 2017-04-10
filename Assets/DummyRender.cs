using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DummyRender : MonoBehaviour {

    public RawImage display;
    public static float grabIntervalSeconds = 1;
    public bool saveImgToFile = false;
    public Image redBlinker;
    public RenderTexture renderTex;
    Texture2D bufTex;
    public WebCamTexture WebCamTex;

    // Use this for initialization
    void Start ()
    {
        bufTex = new Texture2D(renderTex.width, renderTex.height, WorldManager.Inst.textureFormat, false);
        //try to initialize hardware camera
        WebCamDevice[] devices = WebCamTexture.devices;
        string camName = "";
        if(devices.Length>0) camName = devices[0].name;
        //use back facing camera if it exists
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log("Device:" + devices[i].name + "IS FRONT FACING:" + devices[i].isFrontFacing);            
            if (!devices[i].isFrontFacing)
            {
                camName = devices[i].name;
            }
        }
        if (camName != "")
        {
            WebCamTex = new WebCamTexture(camName, 1024, 768);
            WebCamTex.Play();
            GetComponent<Camera>().enabled = false;// .targetTexture = null;
            display.texture = WebCamTex;            
        }
        else
        {
            Debug.Log("Camera not found!");
        }
    }

    float t = 0;
	void OnPostRender ()
    {
        t += Time.deltaTime;
        //some devices can easily run out of memory when grabbing frames too often
        if (t >= grabIntervalSeconds)
        {
            t = 0;
            //read pixels from screen/render texutre                
            bufTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            bufTex.Apply();
            GameServer.buffer = bufTex.GetRawTextureData();
            if (saveImgToFile)
            {
                File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bufTex.EncodeToPNG());
                saveImgToFile = false;
            }
            if(!blinking) StartCoroutine(Blink());
        }
    }

    bool blinking;
    //blinks the small red rectangle indicator
    IEnumerator Blink()
    {
        blinking = true;
        redBlinker.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        redBlinker.gameObject.SetActive(false);
        blinking = false;
    }
    
}
