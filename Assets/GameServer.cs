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
    public Image blueBlinker;
    public RenderTexture renderTex;
    public static float sendInterval = 1;
    public static GameServer Inst;
    private NetManager _netServer;
    private NetPeer _ourPeer;
    private NetDataWriter _dataWriter;
    CompressionMode _compMode;
    public CompressionMode compression
    {
        get { return _compMode; }
        set {
            _compMode = value;
            _netServer.MergeEnabled = value != CompressionMode.none;
        }
    }

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
                int headerSize = sizeof(int)+1;//pix index, compression mode
                int bytesInChunk = (_ourPeer.Mtu / Utils.SomeTextureFormatsToBytes(WorldManager.Inst.textureFormat)) - headerSize;                                
                for (int j = 0; j < chunksEachFrame; ++j)
                {
                    if(streamMode==StreamMode.randomChunks)
                        pixIndex = bytesInChunk *  Random.Range(0, (buffer.Length / bytesInChunk)-1);                    
                    _dataWriter.Reset();                    
                    _dataWriter.Put(pixIndex);

                    if (compression != CompressionMode.none)
                    {
                        byte[] packed = new byte[bytesInChunk];
                        System.Array.Copy(buffer, pixIndex, packed, 0, bytesInChunk);
                        packed = Compressor.Pack(packed, compression);
                        if (packed.Length < bytesInChunk)
                        {
                            _dataWriter.Put((byte)compression);
                            _dataWriter.Put(packed, 0, packed.Length);
                        }
                        else //if compressed packet is larger than not compressed
                        {
                            print("compression ratio fail");
                            _dataWriter.Put((byte)CompressionMode.none);
                            _dataWriter.Put(buffer, pixIndex, Mathf.Min(bytesInChunk, buffer.Length - pixIndex));
                        }
                    }
                    else
                    {
                        _dataWriter.Put((byte)CompressionMode.none);
                        _dataWriter.Put(buffer, pixIndex, Mathf.Min(bytesInChunk, buffer.Length - pixIndex));
                    }
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
        Vector2 size = DummyRender.GetImageSize();
        writer.Put(string.Format("{0};{1}",size.x,size.y));
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
