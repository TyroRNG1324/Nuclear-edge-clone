using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;

public class InGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject finishedMenu;
    private bool pauseMenuEnabled;
    private bool playedOnce = false;
    private bool playerWon;

    private void Start()
    {
        pauseMenuEnabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // If player reaches finish line or there is only one player remaining
        if (propertiesThatChanged["playerWon"] != null)
        {
            finishedMenu.SetActive(true);

            if (propertiesThatChanged["playerWon"].Equals(PhotonNetwork.LocalPlayer.UserId))
            {
                playerWon = true;
                resultText.text = "You Win!";
                resultText.color = Color.green;
                Invoke("Sound", 1f);
            }
            else
            {
                playerWon = false;
                resultText.text = "You Lose!";
                resultText.color = Color.red;
                Invoke("Sound", 1f);
            }
        }
    }

    private void Sound()
    {
        if (!playedOnce)
        {
            if (playerWon)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Win");
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Lose");
            }
            playedOnce = !playedOnce;
        }
    }

    public void GoToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void TogglePauseMenu()
    {
        pauseMenuEnabled = !pauseMenuEnabled;
        if (pauseMenuEnabled)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }
    }
}
