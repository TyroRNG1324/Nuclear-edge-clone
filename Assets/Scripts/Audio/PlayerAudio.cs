using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Photon.Pun;

public class PlayerAudio : MonoBehaviourPun
{
    [SerializeField]
    StudioListener listener;
    private void Start()
    {
        if (!photonView.IsMine) { listener.enabled = false; }
    }
}
