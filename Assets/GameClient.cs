using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class GameClient : MonoBehaviour, INetEventListener
{
    public RenderTexture renderTex;

    private NetManager _netClient;

    [SerializeField] private GameObject _clientBall;
    [SerializeField] private GameObject _clientBallInterpolated;

    private float _newBallPosX;
    private float _oldBallPosX;
    private float _lerpTime;
    bool imageInitialized;

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
            //Fixed delta set to 0.05
            var pos = _clientBallInterpolated.transform.position;
            pos.x = Mathf.Lerp(_oldBallPosX, _newBallPosX, _lerpTime);
            _clientBallInterpolated.transform.position = pos;

            //Basic lerp
            _lerpTime += Time.deltaTime/Time.fixedDeltaTime;
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
                        int d;
                        if (int.TryParse(Utils.EatString(ref s), out d))
                        {
                            renderTex = new RenderTexture(w, h, d);
                            renderTex.Create();
                        }
                    }    
                }
                imageInitialized = true;                
            }
            return;
        }
        _newBallPosX = reader.GetFloat();

        var pos = _clientBall.transform.position;

        _oldBallPosX = pos.x;
        pos.x = _newBallPosX;

        _clientBall.transform.position = pos;

        _lerpTime = 0f;
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
