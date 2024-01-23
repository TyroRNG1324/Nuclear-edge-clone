using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class PlayerCPTracker : MonoBehaviourPunCallbacks
{
    //Keep track of the current checkpoint
    public int currentCP = 0;
    public GameObject nextCheckpoint;
    public float distanceToNextCP;

    private const float checkPointUpdate = 0.2f;

    private void Start()
    {
        InvokeRepeating("UpdateCheckpointsToCamera", 3, checkPointUpdate);
    }

    //The main update loop
    private void Update()
    {
        //Only check if the next checkpoint is assigned with checkpoint one minimum being a failsafe
        if (currentCP > 0 && nextCheckpoint != null)
        {
            //Keep track of the current distance to the next checkpoint
            distanceToNextCP = Vector3.Distance(transform.position, nextCheckpoint.transform.position);
        }
    }

    //Read the current checkpoint number
    public int CurrentCheckpoint()
    {
        return currentCP;
    }

    //Read the distance to the next checkpoint
    public float DistanceToNextCP()
    {
        return distanceToNextCP;
    }

    //Set the current checkpoint number
    public void SetCurrentCheckpoint(int checkpoint)
    {
        currentCP = checkpoint;
    }

    //Set the next checkpoint gameobject
    public void SetNextCheckpoint(GameObject next)
    {
        nextCheckpoint = next;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        currentCP = 0;
        UpdateCheckpointsToCamera();
        CancelInvoke("UpdateCheckpointsToCamera");
    }

    public void UpdateCheckpointsToCamera()
    {
        if (!PhotonNetwork.InRoom) { return; }
        int currentCheckpoint = CurrentCheckpoint();
        float currentDistance = DistanceToNextCP();

        object[] content = new object[] { photonView.OwnerActorNr.ToString(), currentCheckpoint, currentDistance };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(EventCodes.CHECKPOINT, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
