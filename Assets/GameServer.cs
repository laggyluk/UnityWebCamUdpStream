using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.IO;
using System.Collections;

public class GameServer : MonoBehaviour, INetEventListener
{
    public static int chunksEachFrame = 16;
    public static int pixelsPerChunk = 16;
    public RenderTexture renderTex;

    private NetManager _netServer;
    private NetPeer _ourPeer;
    private NetDataWriter _dataWriter;

    [SerializeField] private GameObject _serverBall;

    public void Init()
    {
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this, 5, "lego_eXplorer");
        _netServer.Start(5000);
        _netServer.DiscoveryEnabled = true;
        _netServer.UpdateTime = 15;
        
        print("server initialized");
        //renderTex = new RenderTexture(renderTex.width, renderTex.height, 0, RenderTextureFormat.Default);
        
        
        //StartCoroutine(leUpdate());
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
    void FixedUpdate ()
    {
        //while (true)
        {
            //yield return new WaitForEndOfFrame();
            if (_ourPeer != null && buffer!=null)
            {
                //send chunks of image
                int bytesInChunk = pixelsPerChunk * Utils.SomeTextureFormatsToBytes(WorldManager.textureFormat);//rgb*8bits
                for (int j = 0; j < chunksEachFrame; ++j)
                {
                    //pixIndex = bytesInChunk *  Random.Range(0, (buffer.Length / bytesInChunk)-1);
                    _dataWriter.Reset();                    
                    _dataWriter.Put(pixIndex);                    
                    _dataWriter.Put(buffer, pixIndex, bytesInChunk);                    
                    _ourPeer.Send(_dataWriter, SendOptions.Unreliable);
                    pixIndex += bytesInChunk;
                    if (pixIndex >= buffer.Length) pixIndex = 0;
                }
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
