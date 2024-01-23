using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class TrapHighlighter : MonoBehaviourPunCallbacks
{
    [Header("Line values")]
    [ColorUsageAttribute(true, true)]
    public Color lineColour;
    public Material lineMat;
    public float startAlpha = 0;
    public float endAlpha = .25f;
    public float lineStartWidth = .25f;
    public float lineEndWidth = .1f;

    private ButtonTriggers bt;
    private GameObject player;
    private List<GameObject> lines = new List<GameObject>();
    private bool updateLines = false;

    private void Start()
    {
        bt = GetComponentInParent<ButtonTriggers>();
        for (int i = 0; i < bt.activators.Length; i++)
        {
            CreateLine();
        }
    }

    private void Update()
    {
        if (updateLines)
        {
            for (int i = 0; i < bt.activators.Length; i++)
            {
                lines[i].GetComponent<LineRenderer>().SetPosition(1, bt.activators[i].transform.position);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            if (player.GetComponent<PhotonView>().IsMine)
            {
                updateLines = true;
                foreach (GameObject lineObject in lines)
                {
                    LineRenderer line = lineObject.GetComponent<LineRenderer>();
                    line.enabled = true;
                    line.startColor = new Color(lineColour.r, lineColour.g, lineColour.b, startAlpha);
                    line.endColor = new Color(lineColour.r, lineColour.g, lineColour.b, endAlpha);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player = collision.gameObject;
        if (collision.CompareTag("Player") && player.GetComponent<PhotonView>().IsMine)
        {
            foreach (GameObject lineObject in lines)
            {
                LineRenderer line = lineObject.GetComponent<LineRenderer>();
                line.enabled = false;
            }
            updateLines = false;
        }
    }

    private void CreateLine()
    {
        GameObject highlightLine = new GameObject();
        highlightLine.transform.parent = transform;
        highlightLine.AddComponent<LineRenderer>();
        LineRenderer line = highlightLine.GetComponent<LineRenderer>();
        line.material = lineMat;
        line.numCapVertices = 5;
        line.startWidth = lineStartWidth;
        line.endWidth = lineEndWidth;
        line.SetPosition(0, transform.position);
        line.enabled = false;
        lines.Add(highlightLine);
    }
}
