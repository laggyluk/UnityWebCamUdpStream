using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.IO;
using System.Collections;

public class GameServer : MonoBehaviour, INetEventListener
{
    public int chunksEachFrame = 10;
    public int pixelsPerChunk = 10;
    public RenderTexture renderTex;

    private NetManager _netServer;
    private NetPeer _ourPeer;
    private NetDataWriter _dataWriter;
    Texture2D bufTex;

    [SerializeField] private GameObject _serverBall;

    public void Init()
    {
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this, 100, "lego_eXplorer");
        _netServer.Start(5000);
        _netServer.DiscoveryEnabled = true;
        _netServer.UpdateTime = 15;
        print("server initialized");
        renderTex = new RenderTexture(renderTex.width, renderTex.height, 0, RenderTextureFormat.Default);
        bufTex = new Texture2D(renderTex.width, renderTex.height,TextureFormat.RGB24,false);
        
        StartCoroutine(leUpdate());
    }

    public void Shutdown()
    {
        if (_netServer != null)
            _netServer.Stop();
    }

    void Update()
    {
        if(_netServer!=null) _netServer.PollEvents();
    }

    byte[] buffer;

    ///dump image and load it again

    IEnumerator leUpdate()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (_ourPeer != null)
            {                
                //read pixels from screen/render texutre                
                bufTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                bufTex.Apply();
                buffer = bufTex.GetRawTextureData();                
                File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bufTex.EncodeToPNG());
                //send chunks of image
                for (int j = 0; j < chunksEachFrame; ++j)
                {
                    int start = Random.Range(0, buffer.Length / pixelsPerChunk);
                    _dataWriter.Reset();
                    _dataWriter.Put(start);
                    _dataWriter.PutBytesWithLength(buffer, start, pixelsPerChunk * 24);
                    _ourPeer.Send(_dataWriter, SendOptions.Unreliable);
                }
                /*_serverBall.transform.Translate(1f * Time.fixedDeltaTime, 0f, 0f);
                _dataWriter.Reset();
                _dataWriter.Put(_serverBall.transform.position.x);
                _ourPeer.Send(_dataWriter, SendOptions.Sequenced);
                */
            }
        }
    }


    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[SERVER] We have new peer " + peer.EndPoint);
        _ourPeer = peer;
        NetDataWriter writer = new NetDataWriter();                 // Create writer class        
        //send video resolution
        writer.Put(string.Format("{0};{1};{2}",renderTex.width,renderTex.height,renderTex.depth));
        peer.Send(writer, SendOptions.ReliableOrdered);             // Send with reliability
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectReason reason, int socketErrorCode)
    {
 
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.DiscoveryRequest)
        {
            Debug.Log("[SERVER] Received discovery request. Send discovery response");
            _netServer.SendDiscoveryResponse(new byte[] {1}, remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        if (peer == _ourPeer)
            _ourPeer = null;
    }

    public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
    {
        
    }
}
