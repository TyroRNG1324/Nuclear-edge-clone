using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    private Vector2 moveDirection;
    private float moveSpeed;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == EventCodes.BULLET_DESTROY)
        {
            object[] data = (object[])photonEvent.CustomData;
            int objectViewID = (int)data[0];

            if (objectViewID == photonView.ViewID)
            {
                Destroy();
            }
        }
    }

    private void DestoyBulletEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { photonView.ViewID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EventCodes.BULLET_DESTROY, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void Update()
    {
        if (MultiTargetCamera.createdPlayerList) { transform.Translate(moveDirection * moveSpeed * Time.deltaTime); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
            {
                collision.gameObject.GetComponent<PlayerStatusEffects>().isStunned = true;
            }
        }

        if (PhotonNetwork.IsMasterClient) { DestoyBulletEvent(); }
        Destroy();
    }

    // Variables that are set in the shooting method in the Cannon.
    public void SetBulletProperties(Vector2 dir, float speed, float lifeSpan)
    {
        moveDirection = dir;
        moveSpeed = speed;

        if (PhotonNetwork.IsMasterClient) { Invoke("DestoyBulletEvent", lifeSpan); }
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
    }
    public void PlaySound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Traps/CannonFire", transform.position);
    }
}
