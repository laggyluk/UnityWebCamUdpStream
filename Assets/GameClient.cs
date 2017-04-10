using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.UI;

public class GameClient : MonoBehaviour, INetEventListener
{
    public RawImage clientRenderTex;
    Texture2D clientTex;

    private NetManager _netClient;
    bool imageInitialized;
    //image chunks are written to this while we go
    public static byte[] buffer;

    public void Init ()
    {
        _netClient = new NetManager(this, "lego_eXplorer");
	    _netClient.Start();
	    _netClient.UpdateTime = 15;
        print("client initialized");
    }

    public void Shutdown()
    {
        if (_netClient != null)
            _netClient.Stop();
    }

    public void Update ()
    {
        if (_netClient == null) return;

	    _netClient.PollEvents();

        var peer = _netClient.GetFirstPeer();
        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
            clientTex.LoadRawTextureData(buffer);
            clientTex.Apply();
        }
        else
        {
            _netClient.SendDiscoveryRequest(new byte[] { 1 }, 5000);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    
    public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
    {
        if (!imageInitialized)
        {
            string s = reader.GetString(32);
            if (s != string.Empty)
            {
                print("received: " + s);
                int w = 0;
                if (int.TryParse(Utils.EatString(ref s), out w))
                {
                    int h;
                    if (int.TryParse(Utils.EatString(ref s), out h))
                    {
                        clientTex = new Texture2D(w, h, WorldManager.Inst.textureFormat, false);
                        buffer = new byte[w * h * Utils.SomeTextureFormatsToBytes(WorldManager.Inst.textureFormat)];//where 16 is fixed render texture bit depth
                        clientRenderTex.texture = clientTex;
                    }    
                }
                imageInitialized = true;                
            }
            return;
        }
        //partially fill buffer at given index 
        int start = reader.GetInt();        
        CompressionMode compMode = (CompressionMode)reader.GetByte();
        byte[] payload = reader.GetRemainingBytes();
        if(compMode!=CompressionMode.none) payload = Compressor.UnPack(payload, compMode);
        System.Array.Copy(payload, 0, buffer, start, payload.Length);        
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.DiscoveryResponse && _netClient.PeersCount == 0)
        {
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            _netClient.Connect(remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }
}
