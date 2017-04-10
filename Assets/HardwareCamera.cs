using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HardwareCamera : MonoBehaviour {

    public RawImage display;
    WebCamTexture WebCamTex;
    
    void Start ()
    {
        //try to initialize hardware camera
        WebCamDevice[] devices = WebCamTexture.devices;
        string camName = "";
        if (devices.Length > 0) camName = devices[0].name;
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
            //GetComponent<Camera>().enabled = false;// .targetTexture = null;
            display.texture = WebCamTex;
        }
        else
        {
            Debug.Log("Camera not found!");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
