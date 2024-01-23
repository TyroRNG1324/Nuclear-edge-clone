using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

//[ExecuteInEditMode]
public class FallingPlatformStatic : MonoBehaviourPun
{
    [Header("Falling variables")]
    public float TURNING_OFF_TIME = 0.5f;
    public float FALLING_DOWN_DUR = 1;
    public float FALLING_SPEED = 5;
    public float MAX_DELAY_TIME = 5;

    private float startTime;
    private float timer;

    private bool steppedOn = false;
    private bool canFall = false;
    private bool isActivated = false;

    private SpriteRenderer spriteHolder;
    private BoxCollider2D platformCollider;
    private GameObject newPlatform;

    Vector3 spriteScale;
    Vector2 spriteSize;

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == EventCodes.PLATFORM_STATIC﻿)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 position = (Vector3)data[0];
            float serverTime = (float)data[1];

            if (position == transform.position)
            {
                startTime = serverTime;
                steppedOn = true;
            }
        }
    }

    private void ActivateFallingPlatform()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { transform.position, (float)PhotonNetwork.Time }; ;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EventCodes.PLATFORM_STATIC, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void Start()
    {
        spriteHolder = GetComponentInChildren<SpriteRenderer>();
        platformCollider = GetComponent<BoxCollider2D>();
        spriteSize = spriteHolder.size;
        spriteScale = transform.GetChild(0).localScale;
    }

    private void Update()
    {
        // Check if the player is on top of the platform
        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.up, 0.02f) && !isActivated)
        {
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.up, 0.02f);
            PhotonView pv = hit.transform.gameObject.GetComponent<PhotonView>();

            if (hit.transform.tag == "Player" && !canFall && pv.IsMine)
            {
                ActivateFallingPlatform();
                isActivated = true;
            }
        }

        // Start timer if the platform is stepped on
        if (steppedOn)
        {
            timer = (float)(PhotonNetwork.Time - startTime);

            // If receiving event took longer than delay time, skip falling
            if (timer >= MAX_DELAY_TIME)
            {
                steppedOn = isActivated = false;
                timer = 0;
            }
            else
            {
                canFall = true;
            }
        }

        // If timer is maxed out start falling
        if (timer >= TURNING_OFF_TIME && timer < MAX_DELAY_TIME)
        {
            newPlatform = ObjectPooler.Instance.SpawnFromPool("FallingPlatformMoving", transform.position, Quaternion.identity);
            newPlatform.GetComponent<FallingPlatformMoving>().SetValues(canFall, FALLING_SPEED, FALLING_DOWN_DUR, transform.localScale, spriteScale, spriteSize);
            SwitchStaticPlatform(false);

            // Reset trigger of moving platform
            steppedOn = false;
            timer = 0;

            // Reset static platform when moving platform is done
            Invoke("ResetStaticPlatform", FALLING_DOWN_DUR);
        }
    }

    //reset the platform after a while
    private void ResetStaticPlatform()
    {
        SwitchStaticPlatform(true);
        canFall = false;
        isActivated = false;
    }

    public void SwitchStaticPlatform(bool isActive)
    {
        if (isActive)
        {
            spriteHolder.enabled = true;
            platformCollider.enabled = true;
        }
        else
        {
            spriteHolder.enabled = false;
            platformCollider.enabled = false;
        }
    }
}
