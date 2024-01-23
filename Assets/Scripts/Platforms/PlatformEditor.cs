using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlatformEditor : MonoBehaviour
{ 
    [Header("enable/disable editting mode")]
    public bool editing = false;

    [Header("Size variables")]
    [Range(1, 25)] public int platformLength;
    [Range(1, 10)] public int platformHeight;
    public SpriteRenderer spriteHolder;
    public new BoxCollider2D collider;

    void Update()
    {
        //if you want to edit the wall: enable editing.
        if (editing)
        {

            //prevent NaN errors
            if (platformHeight == 0 || platformLength == 0)
            {
                return;
            }

            //update sprite and scale
            transform.localScale = new Vector3(platformLength, platformHeight, transform.localScale.z);
            spriteHolder.drawMode = SpriteDrawMode.Tiled;
            spriteHolder.transform.localScale = new Vector3(1, 1, 1);
            float newLength = spriteHolder.transform.localScale.y / platformLength;
            float newHeight = spriteHolder.transform.localScale.y / platformHeight;
            spriteHolder.transform.localScale = new Vector3(newLength, newHeight, 1);
            spriteHolder.size = new Vector2(platformLength, platformHeight);

            //update the collider
            collider.enabled = false;
            collider.enabled = true;
        }
    }
}
