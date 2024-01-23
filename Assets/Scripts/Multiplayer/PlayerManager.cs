using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public SpriteRenderer playerSprite;

    private MultiTargetCamera multiTargetCamera;
    private List<int> playersLoaded;
    private Dictionary<int, string> playerColors;

    private List<int> deadPlayers;

    private float mercyTimer;
    [Header("How many seconds can a player be off screen before it dies")]
    public float mercyTime;

    void Awake()
    {
        if (!photonView.IsMine) { enabled = false; return; }

        multiTargetCamera = FindObjectOfType<MultiTargetCamera>();

        deadPlayers = new List<int>();
        playersLoaded = new List<int>();

        if (PhotonNetwork.IsMasterClient)
        {
            if (!playersLoaded.Contains(photonView.OwnerActorNr)) { playersLoaded.Add(photonView.OwnerActorNr); }

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                MultiTargetCamera.allPlayersCreated = true;
            }
            else
            {
                MasterLoadedEvent();
            }
        }

        StartCoroutine(SetPlayersProperty());
    }

    #region Event Related
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == EventCodes.MASTER_LOADED)
        {
            ClientLoadedEvent();
        }

        if (eventCode == EventCodes.CLIENT_LOADED)
        {
            object[] data = (object[])photonEvent.CustomData;
            int actorId = (int)data[0];
            if (!playersLoaded.Contains(actorId)) { playersLoaded.Add(actorId); }

            if (playersLoaded.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                MultiTargetCamera.allPlayersCreated = true;
                AllLoadedEvent();
            }
        }

        if (eventCode == EventCodes.ALL_LOADED)
        {
            object[] data = (object[])photonEvent.CustomData;
            bool allReady = (bool)data[0];

            MultiTargetCamera.allPlayersCreated = allReady;
        }

        if (eventCode == EventCodes.PLAYER_PROPERTY)
        {
            object[] data = (object[])photonEvent.CustomData;
            playerColors = (Dictionary<int, string>)data[0];
            playerColors.ToStringFull();
        }
    }

    private void MasterLoadedEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { true };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.MASTER_LOADED, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void ClientLoadedEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { photonView.OwnerActorNr };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(EventCodes.CLIENT_LOADED, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void AllLoadedEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { MultiTargetCamera.allPlayersCreated };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.ALL_LOADED, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void SetPropertyEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { playerColors };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.PLAYER_PROPERTY, content, raiseEventOptions, SendOptions.SendReliable);
    }
    #endregion

    private void Update()
    {
        // Kill the player off screen
        if (MultiTargetCamera.createdPlayerList && !playerSprite.isVisible)
        {
            // Keep track of time amount of time the player is off screen
            mercyTimer += Time.deltaTime;
            // If the player is longer off screen than the given mercy time kill the player
            if (mercyTimer > mercyTime)
            {
                KillPlayer();
            }
        }
        else
        {
            // If the player isn't off screen reset the timer
            mercyTimer = 0;
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // Disable player if found in list of players
        if (propertiesThatChanged["DeadPlayers"] != null)
        {
            deadPlayers = (propertiesThatChanged["DeadPlayers"] as int[]).ToList();
        };
    }

    public IEnumerator SetPlayersProperty()
    {
        yield return new WaitUntil(() => MultiTargetCamera.createdPlayerList == true);

        if (PhotonNetwork.IsMasterClient)
        {
            playerColors = new Dictionary<int, string>();
            Color[] playerColor = { new Color(0.44f, 0.66f, 0.215f, 1f), new Color(0.9f, 0.29f, 0.196f, 1f), new Color(0.17f, 0.8f, 0.87f, 1f), new Color(0.96f, 0.7f, 0.13f, 1f) };

            // Master sets colors
            int i = 0;
            foreach (PhotonView player in multiTargetCamera.pvPlayers)
            {
                string color = $"#{ColorUtility.ToHtmlStringRGBA(playerColor[i])}";
                playerColors.Add(player.ViewID, color);
                i++;
            }

            // When done sends list of players and colors to others
            SetPropertyEvent();
        }

        // Sets name and color of players
        foreach (PhotonView player in multiTargetCamera.pvPlayers)
        {
            ColorUtility.TryParseHtmlString(playerColors[player.ViewID], out Color colorTemp);

            var renderObject = player.transform.Find("PivotOffset");
            renderObject.GetComponentInChildren<SpriteRenderer>().color = colorTemp;
            player.GetComponentInChildren<TextMeshPro>().text = player.Owner.NickName;
            player.GetComponent<PlayerStatusEffects>().GetColours();
        }
    }

    public void KillPlayer()
    {
        // Add player name to list of dead player for server
        if (!deadPlayers.Contains(photonView.OwnerActorNr))
        {
            deadPlayers.Add(photonView.OwnerActorNr);
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "DeadPlayers", deadPlayers.ToArray() } });
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!deadPlayers.Contains(otherPlayer.ActorNumber)) { deadPlayers.Add(otherPlayer.ActorNumber); }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "DeadPlayers", deadPlayers.ToArray() } });
    }
}

