using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public class AmountDisplay : MonoBehaviour
{
    [SerializeField] float _spacing;
    [SerializeField] Color fillColor;
    [SerializeField] Color backgroundColor;
    [SerializeField] bool singlePrefab;
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] GameObject firstSegmentPrefab;
    [SerializeField] GameObject middleSegmentPrefab;
    [SerializeField] GameObject lastSegmentPrefab;
    [SerializeField] bool follow;
    [SerializeField] float followTime;
    [SerializeField] Color followGainColor;
    [SerializeField] Color followLoseColor;
    [SerializeField] bool unscaledTime;
    float segmentValue;
    HorizontalLayoutGroup layoutGroup;
    Image emptyImage;
    List<Image> fills;
    List<Image> fillsFollow;
    List<Image> background;
    
    //IMPORTANT WORDS:
    //Amount(current amount)
    //MaxAmount(max amount)
    //Fills(filled part of the segment)
    //Follow(follow effect of fills)
    //Segments(the visual part that represents the quantity of segmentValue of the Amount)
    
    private void OnEnable() {
        //CHANGE THE EVENTS TO THE MANAGER SYSTEM THAT YOU WANT!
        HealthSystemExample.OnSetHealth += InitializeAll;
        HealthSystemExample.OnHealthChanged += SetFillsFollow;
    }
    private void OnDisable() {
        //CHANGE THE EVENTS TO THE MANAGER SYSTEM THAT YOU WANT!
        HealthSystemExample.OnSetHealth -= InitializeAll;
        HealthSystemExample.OnHealthChanged -= SetFillsFollow;
    }
    private void Awake() {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = _spacing;
        emptyImage = GetComponent<Image>();
        emptyImage.enabled = false;
        fills = new List<Image>();
        fillsFollow = new List<Image>();
        background = new List<Image>();
    }
    void InitializeAll(float _amount,float _maxAmount, float _segmentValue)
    {
        StopAllCoroutines();
        segmentValue = _segmentValue;
        SetSegments(_maxAmount);
        SetFills(fills,_amount);
        SetFills(fillsFollow,0);
    }

    ///////////////////////SEGMENTS///////////////////////
    void SetSegments(float _maxAmount)
    {
        //Check how many segments you need to add or remove
        int newSegments = Mathf.CeilToInt(_maxAmount/segmentValue);
        int neededSegments = newSegments - transform.childCount;
        if(neededSegments == 0) return;
        if(neededSegments > 0)
        {
            AddSegments(Mathf.Abs(neededSegments));
        }
        else
        {
            RemoveSegments(Mathf.Abs(neededSegments));
        }
    }
    void AddSegments(int segmentsToAdd)
    {
        //Removes last segment(if there's any), Create again that segment but with the correct type
        if(transform.childCount != 0)
        {
            DeleteSegment();
            if(transform.childCount == 0) CreateSegment(singlePrefab ? segmentPrefab : firstSegmentPrefab);
            else CreateSegment(singlePrefab ? segmentPrefab : middleSegmentPrefab);
        }
        //Adds the segments you need
        for (int i = 0; i < segmentsToAdd; i++)
        {
            if(singlePrefab) CreateSegment(segmentPrefab); //single segment
            else
            {
                if(transform.childCount == 0) CreateSegment(firstSegmentPrefab); //first segment
                else CreateSegment(i == segmentsToAdd-1 ? lastSegmentPrefab : middleSegmentPrefab);// middle or last segment
            }
        }
    }
    void RemoveSegments(int segmentsToRemove)
    {
        //Removes the needed segments and one more. Then, Creates the last one of the correct type
        for (int i = 0; i <= segmentsToRemove; i++)
        {
            DeleteSegment();
        }
        if(transform.childCount != 0) CreateSegment(singlePrefab ? segmentPrefab : lastSegmentPrefab);
        else CreateSegment(singlePrefab ? segmentPrefab : firstSegmentPrefab); //or add a extra segment prefab for this case
        
    }
    void CreateSegment(GameObject prefab)
    {
        //Instantiate the given segment prefab at the last position, Adds the Images of it to the Lists, and change the Color
        GameObject segment = Instantiate(prefab,transform.position,Quaternion.identity,transform);
        AmountPrefab amountPrefab = segment.GetComponent<AmountPrefab>();
        AddFills(amountPrefab);
        SetFillColor(amountPrefab.GetFill(),fillColor);
        SetFillColor(amountPrefab.GetBackground(),backgroundColor);
    }
    void DeleteSegment()
    {
        //Deletes the last segment, and Removes the Images of the Lists.
        GameObject segment = transform.GetChild(transform.childCount-1).gameObject;
        RemoveFills(segment.GetComponent<AmountPrefab>());
        DestroyImmediate(segment);
    }

    ///////////////////////FILLS///////////////////////
    void SetFills(List<Image> fillList, float _amount)
    {
        //Sets each fillamount of the given List of Images to what it should be based on the current Amount 
        //and the Max Amount(max amount is the same as segmentAmount * segmentValue)

        for (int i = 0; i < transform.childCount; i++)
        {
            fillList[i].fillAmount = Mathf.Clamp(((_amount - i * segmentValue)/segmentValue),0,1);
        }
    }
    void SetFillsFollow(float actualAmount, float previousAmount)
    {
        //If bool follow is false, then just sets the fill to its value.
        //If not, sets the follow colors, and in case of gaining Amount it sets the current fill to the previous Amount,
        //and make it follow the follow fill, which is set at the actual Amount.
        //If losing Amount, the fills switch sides.
        if(!follow)
        {
            SetFills(fills,actualAmount);
            return;
        }

        bool amountGained = previousAmount < actualAmount;
        StopAllCoroutines();

        SetFillsColor(fillsFollow,amountGained ? followGainColor : followLoseColor);
        SetFills(amountGained ? fillsFollow : fills,actualAmount);
        StartCoroutine(GoFromTo(amountGained ? fills : fillsFollow,previousAmount,actualAmount));
    }
    IEnumerator GoFromTo(List<Image> fillList, float from, float to)
    {
        //Make a value go From to To in a perdiod of time, and setts each frame the fill of the given Images List at the value

        SetFills(fillList,from);
        float timer = 0;
        while (timer < followTime)
        {
            SetFills(fillList, Mathf.Lerp(from,to,timer/followTime));
            timer += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        SetFills(fillList, to);
    }
    void SetFillsColor(List<Image> fillList,Color color)
    {
        //Changes the Color of all the Images of the given List to the given Color

        for (int i = 0; i < transform.childCount; i++)
        {
            fillList[i].color = color;
        }
    }
    void SetFillColor(Image _image,Color _color)
    {
        //Changes the Color of the Image to the given Color

        if(_image != null) _image.color = _color;
    }
    void AddFills(AmountPrefab amountPrefab)
    {
        AddToList(fills,amountPrefab.GetFill());
        AddToList(fillsFollow,amountPrefab.GetFillFollow());
        AddToList(background,amountPrefab.GetBackground());
    }
    void RemoveFills(AmountPrefab amountPrefab)
    {
        RemoveFromList(fills,amountPrefab.GetFill());
        RemoveFromList(fillsFollow,amountPrefab.GetFillFollow());
        RemoveFromList(background,amountPrefab.GetBackground());
    }
    void AddToList(List<Image> _list, Image _image)
    {
        _list.Add(_image != null? _image : emptyImage);
    }
    void RemoveFromList(List<Image> _list, Image _image)
    {
        _list.Remove(_image != null? _image : emptyImage);
    }

    ///////////////////////EDITOR///////////////////////

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(AmountDisplay)), CanEditMultipleObjects]
    public class HealthDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AmountDisplay amountDisplay = (AmountDisplay)target;

            EditorGUILayout.BeginHorizontal();
            amountDisplay._spacing = (float)EditorGUILayout.FloatField("Segment Spacing",amountDisplay._spacing);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            amountDisplay.fillColor = (Color)EditorGUILayout.ColorField("Fill Color",amountDisplay.fillColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            amountDisplay.backgroundColor = (Color)EditorGUILayout.ColorField("Background Color",amountDisplay.backgroundColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            amountDisplay.singlePrefab = (bool)EditorGUILayout.Toggle("Single Segment",amountDisplay.singlePrefab);
            EditorGUILayout.EndHorizontal();

            if(amountDisplay.singlePrefab)
            {
                EditorGUILayout.BeginHorizontal();
                amountDisplay.segmentPrefab = (GameObject)EditorGUILayout.ObjectField("Segment Prefab",amountDisplay.segmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                amountDisplay.firstSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("First Segment Prefab",amountDisplay.firstSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                amountDisplay.middleSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("Middle Segment Prefab",amountDisplay.middleSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                amountDisplay.lastSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("Last Segment Prefab",amountDisplay.lastSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            amountDisplay.follow = (bool)EditorGUILayout.Toggle("Follow",amountDisplay.follow);
            EditorGUILayout.EndHorizontal();

            if(amountDisplay.follow)
            {
                EditorGUILayout.BeginHorizontal();
                amountDisplay.followTime = (float)EditorGUILayout.FloatField("Follow Time",amountDisplay.followTime);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                amountDisplay.followGainColor = (Color)EditorGUILayout.ColorField("Follow Gain Color",amountDisplay.followGainColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                amountDisplay.followLoseColor = (Color)EditorGUILayout.ColorField("Follow Lose Color",amountDisplay.followLoseColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                amountDisplay.unscaledTime = (bool)EditorGUILayout.Toggle("Unscaled Time",amountDisplay.unscaledTime);
                EditorGUILayout.EndHorizontal();
            }

            if(GUI.changed)
            {
                //it all works withouth this, but prefer to keep it just in case

                Undo.RecordObject(this,"");
                Undo.RecordObject(target,"");
                Undo.RecordObject(amountDisplay,"");

                SaveChanges();
                
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(amountDisplay);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
#endif
    #endregion
}
