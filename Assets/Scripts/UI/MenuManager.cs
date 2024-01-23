using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [Header("Menu")]
    public GameObject menuPanel;
    public int characterLimit = 12;
    public TMP_InputField playerNameInput;

    private TMP_Text inputError;

    [Header("Multiplayer")]
    public GameObject multiplayerPanel;
    public TMP_InputField mp_createRoomNameInput;
    public TMP_InputField mp_joinNameInput;
    public TMP_Text mp_playerCount;

    [Header("Lobby")]
    public GameObject lobbyPanel;
    public TMP_Text lobbyName;
    public GameObject startButton;

    [Header("Network Manager")]
    public NetworkManager networkManager;

    public void Multiplayer()
    {
        if (!PlayerNameCorrect(playerNameInput))
        {
            return;
        }
        else if (!PhotonNetwork.IsConnected)
        {
            inputError.text = "Not connected to server. Try again.";
            networkManager.ConnectToServer();
            return;
        }
        else
        {
            menuPanel.SetActive(false);
            multiplayerPanel.SetActive(true);
            inputError.text = "";
        }
    }

    public void BackToMenuScreen()
    {
        multiplayerPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void BackToLobbyScreen()
    {
        lobbyPanel.SetActive(false);
        menuPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    public void CreateLobby(string roomName)
    {
        multiplayerPanel.SetActive(false);
        lobbyName.text = "Room [" + roomName + "]";
        lobbyPanel.SetActive(true);
    }

    public bool IsInputCorrect(TMP_InputField input)
    {
        inputError = input.GetComponentInChildren<TMP_Text>();

        // Check if it is not empty
        if (string.IsNullOrEmpty(input.text))
        {
            inputError.text = "Can not be empty!";
            return false;
        }
        // Check for name length
        else if (input.text.Length > characterLimit)
        {
            inputError.text = "Name too long! (" + input.text.Length + "/" + characterLimit + ")";
            return false;
        }
        // Check for special characters
        else
        {
            foreach (char letter in input.text)
            {
                if (!char.IsLetterOrDigit(letter))
                {
                    inputError.text = "No special characters!";
                    return false;
                }
            }
        }

        // Set error message  to empty
        inputError.text = "";

        return true;
    }

    public bool PlayerNameCorrect(TMP_InputField input)
    {
        if (IsInputCorrect(input))
        {
            PhotonNetwork.NickName = input.text; // Set nickname of player
            
            return true;
        }

        return IsInputCorrect(input);
    }

    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        mp_playerCount.text = "Players online: " + PhotonNetwork.CountOfPlayers.ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayMenuSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/MenuNavigation");
    }
}
