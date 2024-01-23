using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCheckpoint : MonoBehaviour
{
    SpriteRenderer sr;
    public int baseCPNumber;
    public int checkpointNumber;
    public GameObject nextCheckpoint;
    PlayerCPTracker pcpt;

    public bool isFirstCP;
    public bool isLastCP;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    //On trigger exit activates when a rigidbody moves outside of the collider of this gameobject
    private void OnTriggerExit2D(Collider2D other)
    {
        //If a player Exits
        if (other.CompareTag("Player"))
        {
            //Get that player its checkpoint tracker
            pcpt = other.GetComponent<PlayerCPTracker>();
            //Set this checkpoint number as current checkpoint
            pcpt.SetCurrentCheckpoint(checkpointNumber);
            //Give the player which gameobject will be the next checkpoint for distance tracking
            pcpt.SetNextCheckpoint(nextCheckpoint);
        }
    }

    //Update the numbering of this checkpoint
    public void UpdateCheckpoint(int previousTotal)
    {
        checkpointNumber = baseCPNumber + previousTotal;
    }
}
