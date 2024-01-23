using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public int chunkCPAmount;
    public int previousTotalCP;
    public LevelCheckpoint[] checkpoints;

    private LevelCheckpoint firstCP;
    private LevelCheckpoint lastCP;

    public void StartController()
    {
        //Get the total number of checkpoints
        chunkCPAmount = transform.childCount;
        //Get the levelCheckpoint script of all checkpoints in the children of this gameobject
        checkpoints = transform.GetComponentsInChildren<LevelCheckpoint>();

        //Go through all checkpoints to find the first and last checkpoint
        for (int i = 0; i < checkpoints.Length; i++)
        {
            //Keep track of the first checkpoint
            if (checkpoints[i].isFirstCP)
            {
                firstCP = checkpoints[i];
            }
            //Keep track of the last checkpoint
            if (checkpoints[i].isLastCP)
            {
                lastCP = checkpoints[i];
            }
        }
    }

    //Update the numbering of the checkpoints in the children of this gameobject
    public void UpdateCheckpoints(int previousTotal)
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].UpdateCheckpoint(previousTotal);
        }
    }

    //Give the next checkpoint to the lastCP of this checkpoint controller
    public void UpdateNextCheckpoint(GameObject nextCP)
    {
        lastCP.nextCheckpoint = nextCP;
    }

    //Return the first checkpoint of this checkpoint controller
    public GameObject GetFirstCP()
    {
        return firstCP.transform.parent.gameObject;
    }
}
