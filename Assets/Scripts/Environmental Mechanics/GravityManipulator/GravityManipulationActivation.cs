using UnityEngine;

public class GravityManipulationActivation : BaseActivator
{
    [Header("Size of activated gravity zone")]
    public Vector2 activatedScale;

    public float timeActivatedOriginal = 5f;
    public bool isStatic = false;
    
    private float timeActivatedCurrent;
    private bool isStarted = false;

    private Vector2 deactivatedScale;
    
    private float lerpTime = 1f;
    private bool isLerping = false;
    float lerpValue = 0;

    public float gravInGM = 0.05f, gravOutGM = 1f, gravMultDown = 5f;

    private void Start()
    {
        deactivatedScale = new Vector2(0, 0);

        transform.localScale = deactivatedScale;

        timeActivatedCurrent = timeActivatedOriginal;   
    }

    private void Update()
    {
        if (isStarted)
        {
            if (timeActivatedCurrent <= 0)
            {
                Deactivate();
            }
            else if(!isStatic)
            {

                timeActivatedCurrent -= Time.deltaTime;
            }
        }
    }

    // Fixed update handles lerping between startScale and desiredScale
    private void FixedUpdate()
    {
        lerpValue += lerpTime * Time.fixedDeltaTime;

        if (isLerping)
        {
            if (isStarted)
            {
                transform.localScale = Vector2.Lerp(transform.localScale, activatedScale, lerpValue);
                if (Vector2.Distance(transform.localScale, activatedScale) < 0.01f)
                {
                    transform.localScale = activatedScale;
                    isLerping = false;
                }
            }
            else
            {
                transform.localScale = Vector2.Lerp(transform.localScale, deactivatedScale, lerpValue);
                if (Vector2.Distance(transform.localScale, deactivatedScale) < 0.01f)
                {
                    transform.localScale = deactivatedScale;
                    isLerping = false;
                }
            }
        }
    }

    // Activates the GM activation
    public override void Activate()
    {
        lerpValue = 0;
        isLerping = true;
        isStarted = true;

        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Traps/GravityManipulator", transform.position);
    }

    // Deactivates the GM activation
    private void Deactivate()
    {
        lerpValue = 0;
        isLerping = true;
        isStarted = false;

        timeActivatedCurrent = timeActivatedOriginal;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Reset gravity back to normal
            GameObject player = collision.gameObject;
            player.GetComponent<PlayerMovement2D>().gravZoneMult = gravOutGM;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject player = collision.gameObject;
            if (player.GetComponent<Rigidbody2D>().velocity.y < 0)
            {
                player.GetComponent<PlayerMovement2D>().gravZoneMult = gravInGM * gravMultDown;
            }
            else
            {
                player.GetComponent<PlayerMovement2D>().gravZoneMult = gravInGM;
            }
        }
    }

}
