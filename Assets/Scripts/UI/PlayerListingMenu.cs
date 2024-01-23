using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListingMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private PlayerListing playerProperty;
    [SerializeField]
    private MenuManager menuManager;

    private List<PlayerListing> listOfPlayers = new List<PlayerListing>();

    // Gets current players when lobby gets joined
    public override void OnEnable()
    {
        base.OnEnable();
        GetCurrentRoomPlayers();
    }

    // Destroys list of players when leaving.
    public override void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            Destroy(listOfPlayers[i].gameObject);
        }

        listOfPlayers.Clear();
    }

    // Adds every current player in the lobby to the list. 
    private void GetCurrentRoomPlayers()
    {
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Player player)
    {
        // Checks if player already exixts and updates player.
        int index = listOfPlayers.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            listOfPlayers[index].SetPlayerInfo(player);
        }
        // Adds new player player to list.
        else
        {
            PlayerListing listing = Instantiate(playerProperty, content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                listOfPlayers.Add(listing);
            }
        }
    }

    // When the player joins after the room is created, it gets added to the list.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);
    }

    // When the joined player leaves the room it gets removed from list.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = listOfPlayers.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(listOfPlayers[index].gameObject);
            listOfPlayers.RemoveAt(index);
        }
    }
}
