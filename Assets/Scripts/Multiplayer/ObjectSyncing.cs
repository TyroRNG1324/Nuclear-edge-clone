using Photon.Pun;
using UnityEngine;

public class ObjectSyncing : MonoBehaviourPun, IPunObservable
{
    [Header("What needs to be sycned of object")]
    public bool syncPosition = true;
    public bool syncRotation = true;

    private Rigidbody2D objectRB;

    public PlayerRender render;
    public PlayerMovement2D playerMovement;

    private Vector2 networkPosition;
    private float networkRotation;
    private float lag;

    [SerializeField]
    float ROTATION_SMOOTHNESS = 100f;
    float DELAY_DISTANCE = 5;

    int rotation = 0;
    bool justLanded = false;

    private void OnEnable()
    {
        if (GetComponent<Rigidbody2D>() != null)
        {
            objectRB = GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogWarning("No rigidbody of " + gameObject.name + " found. You sure you need syncing?");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Sends information of object to others
        if (stream.IsWriting)
        {
            if (syncPosition)
            {
                stream.SendNext(objectRB.position);
                stream.SendNext(objectRB.velocity);
                if (gameObject.CompareTag("Player"))
                {
                    stream.SendNext(playerMovement.justLandedLocally);
                    playerMovement.justLandedLocally = false;
                }
            }

            if (syncRotation) { stream.SendNext(objectRB.rotation); }
        }
        // Receives information of object to others
        else
        {
            lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

            if (syncPosition)
            {
                networkPosition = (Vector2)stream.ReceiveNext();
                objectRB.velocity = (Vector2)stream.ReceiveNext();
                if (gameObject.CompareTag("Player"))
                {
                    justLanded = (bool)stream.ReceiveNext();
                }
                networkPosition += objectRB.velocity * lag;
            }

            if (syncRotation) { networkRotation = (float)stream.ReceiveNext(); }
        }
    }

    private void FixedUpdate()
    {
        // Updates object with new information of others (clients)
        if (!photonView.IsMine)
        {
            if (syncPosition)
            {
                float distance = Vector2.Distance(objectRB.position, networkPosition);

                if (distance >= DELAY_DISTANCE)
                {
                    objectRB.position = networkPosition;
                }
                else
                {
                    objectRB.position = Vector2.MoveTowards(objectRB.position, networkPosition, Time.fixedDeltaTime);
                }
            }
        }

        if (syncRotation) { objectRB.MoveRotation(networkRotation + Time.fixedDeltaTime * ROTATION_SMOOTHNESS); }

        if (gameObject.CompareTag("Player"))
        {
            if (objectRB.velocity.x > 0.1f) rotation = 0;
            else if (objectRB.velocity.x < -0.1f) rotation = 180;
            render.transform.rotation = Quaternion.Euler(0, rotation, 0);

            if (justLanded)
            {
                render.Land();
                justLanded = false;
            }
        }
    }
}
