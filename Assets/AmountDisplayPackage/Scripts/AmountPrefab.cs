using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmountPrefab : MonoBehaviour
{
    //This component goes in the parent prefab of the segment, and stores the fill,
    //followFill, and background of the segment so that the AmountDisplay can acces it later
    
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
