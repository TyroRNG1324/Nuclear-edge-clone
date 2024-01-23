using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class RoomListingMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private RoomListing roomProperty;

    private List<RoomListing> listOfRooms = new List<RoomListing>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Removed room from room list.
            if (info.RemovedFromList)
            {
                int index = listOfRooms.FindIndex(room => room.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(listOfRooms[index].gameObject);
                    listOfRooms.RemoveAt(index);
                }
            }
            // Added room to room list.
            else
            {
                int index = listOfRooms.FindIndex(room => room.RoomInfo.Name == info.Name);
                if (index == -1)
                {
                    // Not yet added.
                    RoomListing listing = Instantiate(roomProperty, content);
                    if (listing != null)
                    {
                        listing.SetRoomInfo(info);
                        listOfRooms.Add(listing);
                    }
                    // Already added.
                    else
                    {
                        // Modify listing here.
                    }
                }
            }
        }
    }
}
