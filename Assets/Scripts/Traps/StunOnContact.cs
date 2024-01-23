using Photon.Pun;
using UnityEngine;

[ExecuteInEditMode]
public class StunOnContact : MonoBehaviour
{
    PlayerStatusEffects pse;
    public SpriteRenderer spriteHolder;
    public new BoxCollider2D collider;

    //When something enters the trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If it is the player
        if (other.CompareTag("Player"))
        {
            if(other.GetComponent<PhotonView>().IsMine)
            {
                //Get the PlayerStatusEffects script from the player
                pse = other.GetComponent<PlayerStatusEffects>();
                //The player is dead
                pse.isStunned = true;
            }   
        }
    }
}
