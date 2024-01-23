using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallChange : BaseActivator
{
    public SpriteRenderer spriteHolder;
    public new BoxCollider2D collider;

    private Vector2 startPosition;
    private Vector2 destinationPosition;
    private bool wallInStartPosition = true;
    private float lerpTime = 1f;
    private bool isLerping = false;
    float lerpValue = 0;
    public bool hasMoved;

    [Header("platform movement variables")]
    [Range(-10, 10)] public int moveX;
    [Range(-10, 10)] public int moveY;

    private void Start()
    {
        startPosition = transform.position;   

        // Reverse movement if the platform has moved, only called when instantiated in the next set of chunks
        if(hasMoved)
        {
            moveX *= -1;
            moveY *= -1;
        }
    }
    private void FixedUpdate()
    {
        lerpValue += lerpTime * Time.fixedDeltaTime;
        if (isLerping)
        {
            destinationPosition = new Vector2(startPosition.x + moveX, startPosition.y + moveY);
            if (!wallInStartPosition)
            {
                //lerp towards new destination pos

                hasMoved = true;
                transform.position = Vector2.Lerp(transform.position, destinationPosition, lerpValue);
                if (Vector2.Distance(transform.position, destinationPosition) < 0.01f)
                {
                    transform.position = destinationPosition;
                    isLerping = false;
                }
            }
            else
            {
                //lerp back to startPos

                hasMoved = false;
                transform.position = Vector2.Lerp(transform.position, startPosition, lerpValue);
                if (Vector2.Distance(transform.position, startPosition) < 0.01f)
                {
                    transform.position = startPosition;
                    isLerping = false;
                }
            }
        }
    }

    public override void Activate()
    {
        Trigger();
    }

    public void Trigger()
    {
        lerpValue = 0;
        isLerping = true;
        if (wallInStartPosition)
        {
            wallInStartPosition = false;
        }
        else
        {
            wallInStartPosition = true;
        }
    }
}
