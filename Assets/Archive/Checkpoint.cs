using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameObject player;
    PlayerStatusEffects playerStatusEffects;
    public bool shouldGenerate = false;

    SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sprite.color = Color.green;
            player = collision.gameObject;
            playerStatusEffects = player.GetComponent<PlayerStatusEffects>();

            if (playerStatusEffects.respawnPosition.x < transform.position.x)
            {
                playerStatusEffects.respawnPosition = player.transform.position;
            }

            if(shouldGenerate)
            {
                FindObjectOfType<LevelGenerator>().SpawnNewChunks(transform.parent.gameObject);
                shouldGenerate = false;
            }

        }
    }
}
