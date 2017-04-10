﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public enum CaptureMode { render, camera }

public class DummyRender : MonoBehaviour {

    public RawImage display;
    public static float grabIntervalSeconds = 1;
    public static CaptureMode captureMode;
    public bool saveImgToFile = false;
    public Image redBlinker;
    public RenderTexture renderTex;
    Texture2D bufTex;
    public static WebCamTexture webCamTex;
    static DummyRender Inst;

    void Awake()
    {
        Inst = this;
    }

    // Use this for initialization
    void Start ()
    {        
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
            webCamTex = new WebCamTexture(camName, 512,256);
            webCamTex.Play();
            GetComponent<Camera>().enabled = false;// .targetTexture = null;
            display.texture = webCamTex;            
            bufTex = new Texture2D(webCamTex.width, webCamTex.height, WorldManager.Inst.textureFormat, false);
            captureMode = CaptureMode.camera;
            GetComponent<Camera>().targetTexture = null;
            StartCoroutine(GrabCamera());
        }
        else
        {
            Debug.Log("Camera not found!");
            captureMode = CaptureMode.render;
            bufTex = new Texture2D(renderTex.width, renderTex.height, WorldManager.Inst.textureFormat, false);
        }
    }

    public static Vector2 GetImageSize()
    {
        if (captureMode == CaptureMode.camera) return new Vector2(webCamTex.width, webCamTex.height);
        else return new Vector2(Inst.renderTex.width,Inst.renderTex.height);
    }

    //grab image from camera to transmission buffer
    IEnumerator GrabCamera()
    {
        float t = 0;
        while (true)
        {            
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            //some devices can easily run out of memory when grabbing frames too often
            if (t >= grabIntervalSeconds)
            {
                t = 0;
                //read pixels from screen/render texutre                
                bufTex.ReadPixels(new Rect(0, 0, webCamTex.width, webCamTex.height), 0, 0);
                bufTex.Apply();
                GameServer.buffer = bufTex.GetRawTextureData();
                if (saveImgToFile)
                {
                    File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bufTex.EncodeToPNG());
                    saveImgToFile = false;
                }
                if (!blinking) StartCoroutine(Blink());
            }
        }
    }

    float t = 0;
    //render image and dump it to transmission buffer
	void OnPostRender ()
    {
        if (webCamTex != null) return;
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
