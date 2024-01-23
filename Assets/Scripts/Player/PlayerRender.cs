using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRender : MonoBehaviour
{
    private Tween tween = null;
    public ParticleSystem dust;

    public void Jump()
    {
        if (transform != null)
        {
            if (tween != null) tween.Complete();
            tween = transform.DOScaleY(1.3f, 0.2f);
            PlayDust(0.075f, 200);
        }
        else
        {
            tween.Kill(true);
        }
    }

    public void Land()
    {
        if (transform != null)
        {
            if (tween != null) tween.Complete();
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            tween = transform.DOPunchScale(new Vector3(0.0f, -0.5f, 0.0f), 1.0f, 4, 0.5f);
            PlayDust(0.25f, 100);
        }
        else
        {
            tween.Kill(true);
        }
    }

    private void PlayDust(float duration, int count)
    {
        {
            if (!dust.isStopped) return;
            var m = dust.main;
            m.duration = duration;
            var e = dust.emission;
            e.rateOverTime = count;
            dust.Play();
        }
    }

}
