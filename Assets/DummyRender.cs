using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DummyRender : MonoBehaviour {

    public bool saveImgToFile = false;

    public RenderTexture renderTex;
    Texture2D bufTex;

    // Use this for initialization
    void Awake ()
    {
        //renderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
        //GetComponent<Camera>().targetTexture = renderTex;
        bufTex = new Texture2D(renderTex.width, renderTex.height, WorldManager.textureFormat, false);
        
    }
	
	void OnPostRender ()
    {
        //read pixels from screen/render texutre                
        bufTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        bufTex.Apply();
        GameServer.buffer = bufTex.GetRawTextureData();
        if (saveImgToFile)
        {
            File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bufTex.EncodeToPNG());
            saveImgToFile = false;
        }
    }
}
