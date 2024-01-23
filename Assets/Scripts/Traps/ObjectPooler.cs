using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public Vector2 startPosition = new Vector2(0, -30);

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictioray;

    [HideInInspector] public bool createdObjectPool = false;

    private void Start()
    {
        StartCoroutine(CreatingPools());
    }

    // Use this for initialization
    IEnumerator CreatingPools()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            yield return new WaitUntil(() => createdObjectPool == true);
        }

        poolDictioray = new Dictionary<string, Queue<GameObject>>();
        int viewId = 1;

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    GameObject obj = PhotonNetwork.InstantiateRoomObject(pool.tag, startPosition, Quaternion.identity);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                else if (!PhotonNetwork.IsMasterClient)
                {
                    GameObject obj = PhotonView.Find(viewId).gameObject;
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                    viewId++;
                }
            }
            poolDictioray.Add(pool.tag, objectPool);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            createdObjectPool = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "MasterCreatedPool", createdObjectPool } });
        }

        yield break;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictioray == null) { return null; }

        if (!poolDictioray.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with " + tag + "tag doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictioray[tag].Dequeue();

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        poolDictioray[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged["MasterCreatedPool"] != null)
        {
            createdObjectPool = (bool)propertiesThatChanged["MasterCreatedPool"];
        }
    }
}
