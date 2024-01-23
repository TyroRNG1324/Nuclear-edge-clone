using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public MenuManager menuManager;

    [SerializeField] private const int MENU_INDEX = 0;
    [SerializeField] private const int RECONNECT_DELAY = 5;
    [SerializeField] private const int MAX_PLAYERS = 4;

    private bool sceneLoaded = false;

    void Start()
    {
        DontDestroyOnLoad(this);

        // Server settings
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.AutomaticallySyncScene = true;
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        // Making sure that player is not in a room when connecting for the first time or when reconnecting
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // Show lobby when returning from room
            menuManager.BackToLobbyScreen();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected from server. Reason: " + cause.ToString());

        // Trying to reconnect player
        StartCoroutine(ReconnectPlayer());
    }

    private IEnumerator ReconnectPlayer()
    {
        yield return new WaitForSeconds(RECONNECT_DELAY);

        ConnectToServer();

        yield break;
    }

    #region Create Room
    public void CreateRoomMultiplayer()
    {
        if (!PhotonNetwork.IsConnected) { return; }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MAX_PLAYERS
        };

        if (menuManager.IsInputCorrect(menuManager.mp_createRoomNameInput))
        {
            PhotonNetwork.CreateRoom(menuManager.mp_createRoomNameInput.text, options, TypedLobby.Default);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("Failed creating room. Reason: " + message);
    }
    #endregion

    #region Join Room
    public void JoinRoomMultiplayer()
    {
        if (menuManager.IsInputCorrect(menuManager.mp_joinNameInput))
        {
            PhotonNetwork.JoinRoom(menuManager.mp_joinNameInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        menuManager.CreateLobby(PhotonNetwork.CurrentRoom.Name);
        menuManager.startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("Failed joining room. Reason: " + message);
    }
    #endregion

    #region Game Logic
    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StartGame", true } });
        PhotonNetwork.CurrentRoom.IsOpen = PhotonNetwork.CurrentRoom.IsVisible = false;
        int i = Random.Range(1, SceneManager.sceneCountInBuildSettings);
        PhotonNetwork.LoadLevel(i);
        SceneManager.sceneLoaded += OnSceneLoaded; // Checks if scene is loaded for host
    }

    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(MENU_INDEX))
        {
            PhotonNetwork.LoadLevel(0);
            Destroy(gameObject);
        }
        else
        {
            menuManager.BackToLobbyScreen();
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom) { PhotonNetwork.LeaveRoom(); }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged["StartGame"] != null)
        {
            if ((bool)propertiesThatChanged["StartGame"] && !sceneLoaded)
            {
                SceneManager.sceneLoaded += OnSceneLoaded; // Checks if scene is loaded for client
                sceneLoaded = true;
            }
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool playerFound = false;

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerFound = true;
            }
        }

        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(MENU_INDEX) && !playerFound)
        {
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (menuManager != null)
        {
            menuManager.startButton.SetActive(PhotonNetwork.IsMasterClient);
        }
    }
    #endregion
}
