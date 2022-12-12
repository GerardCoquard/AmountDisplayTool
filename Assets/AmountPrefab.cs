using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmountPrefab : MonoBehaviour
{
    [SerializeField] Image fill;
    [SerializeField] Image fillFollow;
    [SerializeField] Image background;
    public Image GetFill()
    {
        return fill;
    }
    public Image GetFillFollow()
    {
        return fillFollow;
    }
    public Image GetBackground()
    {
        return background;
    }
    
}
