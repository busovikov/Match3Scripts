using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckButton : MonoBehaviour
{
    public Sprite onSprite;
    public Sprite offSprite;

    public bool on = false;

    Image image;

    private void Awake()
    {
        
    }

    public void SetOn(bool value)
    {
        if(image == null)
            image = GetComponent<Button>().GetComponent<Image>();

        on = value;
        if (on)
        {
            image.sprite = onSprite;
        }
        else 
        {
            image.sprite = offSprite;
        }
    }

    public bool CheckIfOnAndSwitch()
    {
        SetOn(!on);
        return !on;
    }

}
