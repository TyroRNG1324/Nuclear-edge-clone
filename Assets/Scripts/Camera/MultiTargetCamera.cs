using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class MultiTargetCamera : MonoBehaviourPunCallbacks
{
    [Header("Targets")]
    public List<Transform> targets = new List<Transform>();
    public List<PhotonView> pvPlayers = new List<PhotonView>();
    public Transform firstPlayer;

    [Header("Camera movement settings")]
    [Range(0f, 1f)]
    public float firstPlayerPriority = 1;
    public Vector3 offset;
    public float smoothTime = 0.5f;

    [Header("Camera zoom settings")]
    public float minZoom = 15f;
    public float maxZoom = 10f;
    public float zoomLimit = 10;
    public float getPlayerBuffer = 0.5f;

    public static bool allPlayersCreated;
    public static bool createdPlayerList;

    private Vector3 velocity;
    private Vector3 middlePoint;
    private new Camera camera;

    object[] player1 = new object[2];
    object[] player2 = new object[2];
    object[] player3 = new object[2];
    object[] player4 = new object[2];

    string[] playerActorIds = new string[4];
    object[][] playerProgressList = new object[4][];

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

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true, 60);
        playerProgressList[0] = player1;
        playerProgressList[1] = player2;
        playerProgressList[2] = player3;
        playerProgressList[3] = player4;

        camera = GetComponent<Camera>();
        camera.orthographic = true;

        allPlayersCreated = createdPlayerList = false;
        StartCoroutine(GetPlayers());
    }

    private void LateUpdate()
    {
        if (targets.Count <= 0 || targets[0] == null || !allPlayersCreated)
        {
            /*
             * TODO: Add visual loading screen
             * Needs to be a seperate if statement with only allPlayerLoaded in it for loading screen
             */

            return;
        }

        CameraMove();
        CameraZoom();
    }

    private void CameraMove()
    {
        // Move camera according to transforms in list
        middlePoint = GetMiddlePoint();

        // Determine new pos
        if (firstPlayer != null)
        {
            Vector3 firstPlayerOffset = firstPlayer.position - middlePoint;
            offset = new Vector3(firstPlayerOffset.x * firstPlayerPriority, firstPlayerOffset.y * firstPlayerPriority, offset.z);
        }
        Vector3 newPos = middlePoint + offset;

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);
    }

    private void CameraZoom()
    {
        // Zoom in based on distance greatest distance between targets
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimit);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, newZoom, Time.deltaTime);
    }

    float GetGreatestDistance()
    {
        // Calculate distance between targets, return biggest
        var bounds = new Bounds(targets[0].position, Vector3.zero);

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }

        return bounds.size.x;
    }

    Vector3 GetMiddlePoint()
    {
        // Gets the middle point between all targets in bounds
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        // Encapsulate all positions in bounds
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }

        return bounds.center;
    }

    private IEnumerator GetPlayers()
    {
        // Finds all players in scene and adds them to the target list
        yield return new WaitUntil(() => allPlayersCreated == true);

        targets.Clear();
        pvPlayers.Clear();

        List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();

        foreach (GameObject player in players)
        {
            targets.Add(player.transform);
            pvPlayers.Add(player.GetComponent<PhotonView>());
        }

        createdPlayerList = true;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged["DeadPlayers"] != null && createdPlayerList)
        {
            List<int> deadPlayers = (propertiesThatChanged["DeadPlayers"] as int[]).ToList();

            // Disable player if found in list of players
            foreach (Transform target in targets.ToList())
            {
                int actorId = target.GetComponent<PhotonView>().OwnerActorNr;
                if (deadPlayers.Contains(actorId))
                {
                    targets.Remove(target);
                    target.gameObject.SetActive(false);
                }
            }

            // If one player is left, it wins   
            if (targets.Count == 1)
            {
                if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("playerWon"))
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "playerWon", targets[0].GetComponent<PhotonView>().Owner.UserId } });
                }
            }
        };
    }

    public void RemovePlayer(int actorID)
    {
        foreach (PhotonView player in pvPlayers.ToList())
        {
            if (player.OwnerActorNr == actorID)
            {
                targets.Remove(player.gameObject.transform);
                pvPlayers.Remove(player);
            }
        }
    }

    // Updates players and camera when a player leaves
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemovePlayer(otherPlayer.ActorNumber);
    }

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == EventCodes.CHECKPOINT)
        {
            CalculatePlacements(photonEvent);
        }

        if (eventCode == EventCodes.FIRST_PLACE)
        {
            SetFirstPlace(photonEvent);
        }
    }

    private void CalculatePlacements(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        string actorId = (string)data[0];
        int cp = (int)data[1];
        float distance = (float)data[2];

        if (!playerActorIds.Contains(actorId))
        {
            for (int i = 0; i < playerActorIds.Length; i++)
            {
                if (playerActorIds[i] != null)
                {
                    continue;
                }
                else
                {
                    playerActorIds[i] = actorId;
                    break;
                }
            }
        }

        for (int i = 0; i < playerActorIds.Length; i++)
        {
            if (playerActorIds[i] == actorId)
            {
                playerProgressList[i][0] = cp;
                playerProgressList[i][1] = distance;
            }
        }

        // Sort on distance from checkpoint
        for (int i = 0; i < 4; i++)
        {
            // Keep track if there was a swap in this loop
            bool swapped = false;

            if (playerActorIds[0] != null && playerActorIds[1] != null)
            {
                // Check if the first entry in the array has a bigger distance to the next checkpoint than the second entry in the array
                if ((float)playerProgressList[0][1] > (float)playerProgressList[1][1])
                {
                    // Swap the two entries
                    SwapOrder(0, 1);
                    // There was a swap
                    swapped = true;
                }
            }

            if (playerActorIds[1] != null && playerActorIds[2] != null)
            {
                if ((float)playerProgressList[1][1] > (float)playerProgressList[2][1])
                {
                    SwapOrder(1, 2);
                    swapped = true;
                }
            }

            if (playerActorIds[2] != null && playerActorIds[3] != null)
            {
                if ((float)playerProgressList[2][1] > (float)playerProgressList[3][1])
                {
                    SwapOrder(2, 3);
                    swapped = true;
                }
            }

            // If no swaps took place this loop end the for loop
            if (swapped == false)
            {
                break;
            }
        }

        // Sort on current checkpoint
        for (int i = 0; i < 4; i++)
        {
            // Keep track if there was a swap in this loop
            bool swapped = false;
            if (playerActorIds[0] != null && playerActorIds[1] != null)
            {
                // Check if the first entry in the array has a smaller checkpoint number than the second entry in the array
                if ((int)playerProgressList[0][0] < (int)playerProgressList[1][0])
                {
                    // Swap the two entries
                    SwapOrder(0, 1);
                    // There was a swap
                    swapped = true;
                }
            }

            if (playerActorIds[1] != null && playerActorIds[2] != null)
            {
                if ((int)playerProgressList[1][0] < (int)playerProgressList[2][0])
                {
                    SwapOrder(1, 2);
                    swapped = true;
                }
            }

            if (playerActorIds[2] != null && playerActorIds[3] != null)
            {
                if ((int)playerProgressList[2][0] < (int)playerProgressList[3][0])
                {
                    SwapOrder(2, 3);
                    swapped = true;
                }
            }
            // If no swaps took place this loop end the for loop
            if (swapped == false)
            {
                break;
            }
        }

        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { playerActorIds[0] };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EventCodes.FIRST_PLACE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void SetFirstPlace(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].GetComponent<PhotonView>().OwnerActorNr.ToString() == (string)data[0])
            {
                firstPlayer = targets[i];
            }
        }
    }

    // This function swaps the two given entries in teh player progress array and the player names array
    private void SwapOrder(int swapFirst, int swapSecond)
    {
        object[] tempObject = playerProgressList[swapFirst];
        playerProgressList[swapFirst] = playerProgressList[swapSecond];
        playerProgressList[swapSecond] = tempObject;

        string tempName = playerActorIds[swapFirst];
        playerActorIds[swapFirst] = playerActorIds[swapSecond];
        playerActorIds[swapSecond] = tempName;
    }
}
