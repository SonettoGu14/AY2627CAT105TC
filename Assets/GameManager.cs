using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UnityEngine.UI.Slider hpSlider;
    public UnityEngine.UI.Image hpImage;
    // Start is called before the first frame update
    void Start()
    {
        hpSlider.value = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            hpSlider.value = hpSlider.value - 0.2f;
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            hpSlider.value = hpSlider.value + 0.2f;
        }
        if(hpSlider.value <= 0.2)
        {
            hpImage.color = Color.red;
        }
        else if(hpSlider.value <= 0.4)
        {
            hpImage.color = Color.yellow;
        }else
        {
            hpImage.color = Color.green;
        }
    }
}
