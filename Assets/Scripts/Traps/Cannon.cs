using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine;

/*
 * IMPORTED
 * 
 * This script still has outdated code which isn't being used. This can be confusing, but it maybe needed for later on
 * or gets decided to be removed. For now cannon can't be angled.
 */

public class Cannon : BaseActivator
{
    public GameObject pivot;
    public GameObject firePoint;

    public bool activated = true;

    [Header("Shooting variables")]
    [Range(0.2f, 5f)] public float SHOOTING_INTERVAL = 2;
    
    [SerializeField] private float BULLET_SPEED = 2;
    [SerializeField] private float BULLET_LIFESPAN = 2;

    Coroutine coroutine;

    #region Old angle variables
    /*
    [Header("Rotation variables")]
    [Range(0, 20f)] public float lerpSpeed;
    [Range(0f, 1f)] public float waitForLerpTime;
    private int degreeChange;
    private int currentAngle = 0;
    private int minRotation = 0, maxRotation = 180;
    private bool changeDir = true;
    private bool lerping = false;

    private int[] angles = new int[] { 12, 6, 3 };
    private int angleDivision;


    [Header("Angle determination")]
    public MyEnum amountOfAngles = new MyEnum();
    */
    #endregion

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Start()
    {
        // AmountOfAngles();

        if (PhotonNetwork.IsMasterClient && activated) { coroutine = StartCoroutine(Fire()); }
    }

    private void ActivateShootEventToAll()
    {
        if (!PhotonNetwork.InRoom) { return; }

        object[] content = new object[] { transform.position }; ;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.TRIGGER_TRAPS, content, raiseEventOptions, SendOptions.SendReliable);
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
                Bullet();
            }
        }
    }

    public override void Activate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            activated = !activated;

            // Old code is ChangeAngles()
            if (activated && coroutine == null)
            {
                coroutine = StartCoroutine(Fire());
            }
        }
    }

    #region Firing bullet
    private IEnumerator Fire()
    {
        yield return new WaitUntil(() => MultiTargetCamera.createdPlayerList == true);

        while (activated)
        {
            ActivateShootEventToAll();
            Bullet();
            yield return new WaitForSeconds(SHOOTING_INTERVAL);
        }

        // TODO: Insert visual that shows trigger on cooldown
        yield return new WaitForSeconds(SHOOTING_INTERVAL);
        // TODO: Revert visual to normal
        coroutine = null;
        activated = false;
    }

    private void Bullet() // Old void name was Fire
    {
        // Get bullet from the bulletPool, set the position to the fire point. set the firing direction, bulletLifespan and the bullet movepseed.
        Vector2 bulDir = ((Vector2)firePoint.transform.position - (Vector2)pivot.transform.position).normalized;
        GameObject bullet = ObjectPooler.Instance?.SpawnFromPool("Bullet", firePoint.transform.position, Quaternion.identity);
        bullet?.GetComponent<Bullet>().SetBulletProperties(bulDir, BULLET_SPEED, BULLET_LIFESPAN);
        bullet.GetComponent<Bullet>().PlaySound();
    }
    #endregion

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Start();
        }
    }

    #region Old angeling code
    /*
        public enum MyEnum
    {
        High,
        Medium,
        Low
    };
    private void Update()
    {
        // lerp to the next angle in the angles section
        if (lerping)
        {
            pivot.transform.localRotation = Quaternion.Lerp(pivot.transform.localRotation, Quaternion.Euler(0, 0, currentAngle), Time.deltaTime * lerpSpeed);
        }
    }

    private IEnumerator ChangeAngles()
    {
        if (!activated) { yield break; }

        // check at which angle the canon should stop and should fire
        AngleOptions();

        // give lerping time to lerp
        lerping = true;
        yield return new WaitForSeconds(waitForLerpTime);
        lerping = false;

        Fire();

        yield return new WaitForSeconds(shootingInterval);
        StartCoroutine(ChangeAngles());
    }

    #region amount of angles
    private void AmountOfAngles()
    {
        // check what the degree change should be
        if (amountOfAngles == MyEnum.High)
        {
            angleDivision = angles[0];
        }
        else if (amountOfAngles == MyEnum.Medium)
        {
            angleDivision = angles[1];
        }
        else if (amountOfAngles == MyEnum.Low)
        {
            angleDivision = angles[2];
        }
        degreeChange = maxRotation / angleDivision;
    }
    #endregion

    #region angle change

    private void AngleOptions()
    {
        if (currentAngle == maxRotation)
        {
            changeDir = false;
        }
        else if (currentAngle == minRotation)
        {
            changeDir = true;
        }

        if (changeDir)
        {
            currentAngle += degreeChange;
        }
        else
        {
            currentAngle -= degreeChange;
        }
    }
    #endregion
    */
    #endregion
}
