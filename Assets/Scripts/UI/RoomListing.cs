using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomListing : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roomName, roomCount;

    public RoomInfo RoomInfo { get; private set; }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        roomName.text = roomInfo.Name;
        roomCount.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomInfo.Name);
    }
}

