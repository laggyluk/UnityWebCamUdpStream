using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.IO;
using System.Collections;
using UnityEngine.UI;

public enum StreamMode { continous, randomChunks}

public class GameServer : MonoBehaviour, INetEventListener
{
    public StreamMode streamMode;    
    public int chunksEachFrame = 512;
    public int pixelsPerChunk = 8;
    public Image blueBlinker;
    public RenderTexture renderTex;
    public static float sendInterval = 1;
    public static GameServer Inst;
    private NetManager _netServer;
    private NetPeer _ourPeer;
    private NetDataWriter _dataWriter;    

    void Awake()
    {
        Inst = this;
    }

    public void Init()
    {
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this, 5, "lego_eXplorer");
        _netServer.Start(5000);
        _netServer.DiscoveryEnabled = true;
        _netServer.UpdateTime = 15;
        
        print("server initialized");
        //renderTex = new RenderTexture(renderTex.width, renderTex.height, 0, RenderTextureFormat.Default);
        
        
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

    public static byte[] buffer;

    ///dump image and load it again
    int pixIndex = 0;
    IEnumerator leUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);
            //send chunks of image
            if (_ourPeer != null && buffer!=null)
            {
                //how much bytes we can fit in packet?
                int bytesInChunk = pixelsPerChunk * Utils.SomeTextureFormatsToBytes(WorldManager.Inst.textureFormat);//rgb*8bits
                
                for (int j = 0; j < chunksEachFrame; ++j)
                {
                    if(streamMode==StreamMode.randomChunks)
                        pixIndex = bytesInChunk *  Random.Range(0, (buffer.Length / bytesInChunk)-1);
                    _dataWriter.Reset();                    
                    _dataWriter.Put(pixIndex);                    
                    _dataWriter.Put(buffer, pixIndex, bytesInChunk);                    
                    _ourPeer.Send(_dataWriter, SendOptions.Unreliable);
                    if (streamMode == StreamMode.continous)
                    {
                        pixIndex += bytesInChunk;
                        if (pixIndex >= buffer.Length) pixIndex = 0;
                    }
                }
                if (!blinking) StartCoroutine(Blink());
            }
        }
    }

    bool blinking;
    //blinks the small red rectangle indicator
    IEnumerator Blink()
    {
        blinking = true;
        blueBlinker.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        blueBlinker.gameObject.SetActive(false);
        blinking = false;
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
