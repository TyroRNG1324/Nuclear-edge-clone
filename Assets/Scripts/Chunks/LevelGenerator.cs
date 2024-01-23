using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject checkPointChunk;
    [SerializeField] public List<GameObject> chunks = new List<GameObject>();
    [SerializeField] private GameObject finishChunk;
    [SerializeField] private Transform chunkHolder;
    [SerializeField] private GameObject checkpoints;

    public int totalCheckpoints;
    private bool init = true;

    private CheckpointController tempCPController;
    private CheckpointController tempCPController2;
    private GameObject tempChunk;

    private float inbetweenChunkWidth = 24;
    private float chunkWidth = 110;

    private void Start()
    {
        if (init)
        {
            // Error message when there is something wrong
            if (chunkHolder.childCount != checkpoints.transform.childCount)
            {
                Debug.Log("ERROR: Existing chunks and in between chunks aren't equal!");
                Debug.Log("If the amount isn't equal, this will break the checkpoint system.");
                Debug.Log("Make sure all chunks are in the chunkholder gameobject and all in between chunks are in the checkpoints gameobject");
            }

            // Go through the existing chunks (Must make sure the existing chunks and checkpoints are in order in the gameobjects) 
            for (int i = 0; i < chunkHolder.transform.childCount; i++)
            {
                // Add exisitng chunks to a list
                chunks.Add(chunkHolder.transform.GetChild(i).gameObject);

                // Get the checkpoint controller for a chunk
                tempCPController = chunkHolder.transform.GetChild(i).transform.GetComponentInChildren<CheckpointController>();
                // Start the controller via a function to keep things in order
                tempCPController.StartController();
                // Update the checkpoint numbering
                tempCPController.UpdateCheckpoints(totalCheckpoints);
                // Keep track of the total number of checkpoints
                totalCheckpoints += tempCPController.chunkCPAmount;

                // Get the checkpoint controller for a in betweenchunk
                tempCPController2 = checkpoints.transform.GetChild(i).transform.GetComponentInChildren<CheckpointController>();
                // Start the controller via a function to keep things in order
                tempCPController2.StartController();
                // Update the checkpoint numbering
                tempCPController2.UpdateCheckpoints(totalCheckpoints);
                // Keep track of the total number of checkpoints
                totalCheckpoints += tempCPController2.chunkCPAmount;

                // Update the next checkpoint for the chunk
                tempCPController.UpdateNextCheckpoint(tempCPController2.GetFirstCP());

                // For all in between chunks except the last one
                if (i < chunkHolder.transform.childCount - 1)
                {
                    // Get the next chunk checkpoint controller
                    tempCPController = chunkHolder.transform.GetChild(i + 1).transform.GetComponentInChildren<CheckpointController>();
                    // Start the controller via a function to keep things in order
                    tempCPController.StartController();


                    // Update the next checkpoint for the in between chunk
                    tempCPController2.UpdateNextCheckpoint(tempCPController.GetFirstCP());
                }
            }

            // Only do this once
            init = false;
        }
    }

    public void SpawnNewChunks(GameObject originInBetweenChunk)
    {
        if(chunks != null)
        {
            // Get the checkpoint manager of the in between chunk that was triggered 
            tempCPController2 = originInBetweenChunk.GetComponentInChildren<CheckpointController>();
            // Start the controller via a function to keep things in order
            tempCPController2.StartController();

            // Get the position of the inbetween chunk
            Vector2 position = originInBetweenChunk.transform.position;
            // Get a random chunk gameobject from the list
            GameObject chunk = chunks[Random.Range(0, chunks.Count)];
            // Spawn that chunk after the inbetween chunk
            tempChunk = Instantiate(chunk, position + new Vector2(inbetweenChunkWidth, 0), Quaternion.identity, chunkHolder);

            // Get the checkpoint manager of the new chunk
            tempCPController = tempChunk.transform.GetComponentInChildren<CheckpointController>();
            // Start the controller via a function to keep things in order
            tempCPController.StartController();
            // Update the checkpoints in the new chunk
            tempCPController.UpdateCheckpoints(totalCheckpoints);
            // Keep track of the total number of checkpoints
            totalCheckpoints += tempCPController.chunkCPAmount;

            // Update the next checkpoint for the in between chunk to be the first of the newly spawned chunk
            tempCPController2.UpdateNextCheckpoint(tempCPController.GetFirstCP());

            // Remove the spawned chunk from the list of chunks that can still spawn again
            chunks.Remove(chunk);

            if (chunks.Count == 0)
            {
                // Instantiate the finish if no more chunks left
                GameObject finish = Instantiate(finishChunk, position + new Vector2(chunkWidth + inbetweenChunkWidth, 0), Quaternion.identity, chunkHolder);
                tempCPController.UpdateNextCheckpoint(finish.GetComponentInChildren<PlayerFinish>().gameObject);
            }
            else
            {
                // Spawn in a checkpoint
                GameObject newInbetweenChunk = Instantiate(checkPointChunk, position + new Vector2(chunkWidth + inbetweenChunkWidth, 0), Quaternion.identity, checkpoints.transform);
                // This new in between chunk should spawn the next chunk with trigger
                newInbetweenChunk.GetComponentInChildren<LevelGenerationTrigger>().shouldGenerate = true;

                // Get the checkpoint manager of the newly spawned inbetween chunk
                tempCPController2 = newInbetweenChunk.GetComponentInChildren<CheckpointController>();
                // Start the controller via a function to keep things in order
                tempCPController2.StartController();
                // Update the next checkpoint for the chunk to be the inbetween chunk
                tempCPController.UpdateNextCheckpoint(tempCPController2.GetFirstCP());
            }
            
        }
    }
}
