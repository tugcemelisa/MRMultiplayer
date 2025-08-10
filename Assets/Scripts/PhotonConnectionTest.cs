using UnityEngine;
using Photon.Pun;

public class PhotonConnectionTest : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); 
        Debug.Log("Connecting to Photon...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause);
    }
}
