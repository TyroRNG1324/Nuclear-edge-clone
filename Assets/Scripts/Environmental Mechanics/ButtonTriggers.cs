using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Experimental.Rendering.Universal;

public class ButtonTriggers : MonoBehaviourPun
{
    public float activationCooldown = 1f;

    [Header("Place gameobject with activator here")]
    public BaseActivator[] activators;
    public Light2D light;

    [SerializeField] private float rotationSpeed;
    private float timestamp;
    private Color originalLightColor;

    private void Start()
    {
        originalLightColor = gameObject.GetComponent<SpriteRenderer>().color;
    }

    private void Update()
    {
        if (timestamp <= Time.time)
        {
            gameObject.GetComponent<SpriteRenderer>().color = originalLightColor;
            light.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine && timestamp <= Time.time)
        {
            TriggerTrapsEvent();
            gameObject.GetComponent<SpriteRenderer>().flipX = !gameObject.GetComponent<SpriteRenderer>().flipX;
            gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
            light.enabled = false;
            timestamp = Time.time + activationCooldown;
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }

    private void ActivateTraps()
    {
        for (int i = 0; i < activators.Length; i++)
        {
            /* 
             * Some triggers should only be triggered by the master (like activating cannon),
             * this is a work around for that. Add MasterControlled tag to any trap that needs
             * to be controlled ONLY by the master.
             */

            if (PhotonNetwork.IsMasterClient && activators[i].gameObject.CompareTag("MasterControlled"))
            {
                activators[i].Activate();
            }
            else
            {
                activators[i].Activate();
            }
        }
    }

    private void TriggerTrapsEvent()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { transform.position };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EventCodes.TRIGGER_TRAPS, content, raiseEventOptions, SendOptions.SendReliable);
    }

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
        if (eventCode == EventCodes.TRIGGER_TRAPS)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 position = (Vector3)data[0];

            if (position == transform.position)
            {
                ActivateTraps();
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Trigger");
            }
        }
    }
}
