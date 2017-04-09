using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRender : MonoBehaviour {

    Texture2D bufTex;

    // Use this for initialization
    void Start ()
    {
        bufTex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);
    }
	
	// Update is called once per frame
	void OnPostRender () {
        //read pixels from screen/render texutre                
        bufTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        bufTex.Apply();
        buffer = bufTex.GetRawTextureData();
        File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bufTex.EncodeToPNG());
    }
}
