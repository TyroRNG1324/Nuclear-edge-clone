using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerationTrigger : MonoBehaviour
{
    public bool shouldGenerate = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (shouldGenerate)
            {
                FindObjectOfType<LevelGenerator>().SpawnNewChunks(transform.parent.gameObject);
                shouldGenerate = false;
            }
        }
    }
}
