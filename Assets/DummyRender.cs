using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DummyRender : MonoBehaviour {

    public static float grabIntervalSeconds = 1;
    public bool saveImgToFile = false;
    public Image redBlinker;
    public RenderTexture renderTex;
    Texture2D bufTex;

    // Use this for initialization
    void Start ()
    {
        //renderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
        //GetComponent<Camera>().targetTexture = renderTex;
        bufTex = new Texture2D(renderTex.width, renderTex.height, WorldManager.Inst.textureFormat, false);
        
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
