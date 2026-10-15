using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public bool isFading = false;
    private float percent;
    // public void StartFadeOut()
    // {
    //     CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
    //     if (canvasGroup != null)
    //     {
    //         canvasGroup.alpha = 0f;
    //     }
    // }

    public void StartFadeOut()
    {
        isFading = true;
        percent = 0f;
    }

    void Update()
    {
        if (isFading)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            percent = percent + Time.deltaTime * 0.5f;
            if (percent > 1f)
            {
                percent = 1f;
                isFading = false;
            }
            canvasGroup.alpha = Mathf.Lerp(1f , 0f, percent);
        }
    }
}
