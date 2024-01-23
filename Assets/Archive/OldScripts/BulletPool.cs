using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool bulletPoolInstance;
    private bool notEnoughBulletsInPool = true;
    [SerializeField] private GameObject pooledBullet;

    private List<GameObject> bullets;

    private void Awake()
    {
        bulletPoolInstance = this;
    }

    private void Start()
    {
      //initlialize list
        bullets = new List<GameObject>();
    }
    public GameObject GetBullet()
    {
      //loop through bullets in pool
        if(bullets.Count > 0)
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                if(!bullets[i].activeInHierarchy)
                {
                    return bullets[i];
                }
            }
        }

        if(notEnoughBulletsInPool)
        {
          // if there are no bullet in the bool make a new one
            GameObject bullet = Instantiate(pooledBullet);
            bullet.SetActive(false);
            bullets.Add(bullet);
            return bullet;
        }
        return null;
    }
}
