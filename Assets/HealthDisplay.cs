using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
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
    /*[SerializeField]*/ float segmentValue; //change if want segment value to be on display instead of the manager
    HorizontalLayoutGroup layoutGroup;
    List<Image> fills;
    List<Image> fillsFollow;
    List<Image> background;
    private void OnEnable() {
        HealthSystem.OnSetHealth += InitializeAll;
        HealthSystem.OnHealthChanged += SetFillsFollow;
    }
    private void OnDisable() {
        HealthSystem.OnSetHealth -= InitializeAll;
        HealthSystem.OnHealthChanged -= SetFillsFollow;
    }
    private void Awake() {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = _spacing;
        fills = new List<Image>();
        fillsFollow = new List<Image>();
        background = new List<Image>();
    }
    void InitializeAll(float health,float maxHP, float _segmentValue)
    {
        StopAllCoroutines();
        segmentValue = _segmentValue;
        SetSegments(maxHP);
        SetFills(fills,health);
        SetFills(fillsFollow,0);
    }

    ///////////////////////SEGMENTS///////////////////////
    void SetSegments(float maxHealth)
    {
        int newSegments = Mathf.CeilToInt(maxHealth/segmentValue);
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
    void AddSegments(int amount)
    {
        if(transform.childCount != 0)
        {
            DeleteSegment();
            if(transform.childCount == 0) CreateSegment(singlePrefab ? segmentPrefab : firstSegmentPrefab);
            else CreateSegment(singlePrefab ? segmentPrefab : middleSegmentPrefab);
        }
        for (int i = 0; i < amount; i++)
        {
            if(singlePrefab) CreateSegment(segmentPrefab);
            else
            {
                if(transform.childCount == 0) CreateSegment(firstSegmentPrefab); //first segment
                else CreateSegment(i == amount-1 ? lastSegmentPrefab : middleSegmentPrefab);// middle or last segment
            }
        }
    }
    void RemoveSegments(int amount)
    {
        for (int i = 0; i <= amount; i++)
        {
            DeleteSegment();
        }
        if(transform.childCount != 0) CreateSegment(singlePrefab ? segmentPrefab : lastSegmentPrefab);
        else CreateSegment(singlePrefab ? segmentPrefab : firstSegmentPrefab); //or add a extra segment prefab for this case
        
    }
    void CreateSegment(GameObject prefab)
    {
        GameObject segment = Instantiate(prefab,transform.position,Quaternion.identity,transform);
        HealthFill healthFill = segment.GetComponent<HealthFill>();
        fills.Add(healthFill.GetFill());
        fillsFollow.Add(healthFill.GetFillFollow());
        background.Add(healthFill.GetBackground());
        healthFill.GetFill().color = fillColor;
        healthFill.GetBackground().color = backgroundColor;
    }
    void DeleteSegment()
    {
        GameObject segment = transform.GetChild(transform.childCount-1).gameObject;
        HealthFill healthFill = segment.GetComponent<HealthFill>();
        fills.Remove(healthFill.GetFill());
        fillsFollow.Remove(healthFill.GetFillFollow());
        background.Remove(healthFill.GetBackground());
        DestroyImmediate(segment);
    }

    ///////////////////////FILLS///////////////////////
    void SetFills(List<Image> fillList, float currentHealth)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            fillList[i].fillAmount = Mathf.Clamp(((currentHealth - i * segmentValue)/segmentValue),0,1);
        }
    }
    void SetFillsFollow(float actualHealth, float previousHealth, bool heal)
    {
        if(!follow)
        {
            SetFills(fills,actualHealth);
            return;
        }

        StopAllCoroutines();
        if(heal)
        {
            SetFillColor(fillsFollow,followGainColor);
            SetFills(fillsFollow,actualHealth);
            StartCoroutine(GoFromTo(fills,previousHealth,actualHealth));
        }
        else
        {
            SetFillColor(fillsFollow,followLoseColor);
            SetFills(fills,actualHealth);
            StartCoroutine(GoFromTo(fillsFollow,previousHealth,actualHealth));
        }
    }
    IEnumerator GoFromTo(List<Image> fillList, float from, float to)
    {
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
    void SetFillColor(List<Image> fillList,Color color)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            fillList[i].color = color;
        }
    }

    ///////////////////////EDITOR///////////////////////

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(HealthDisplay)), CanEditMultipleObjects]
    public class HealthDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            HealthDisplay healthDisplay = (HealthDisplay)target;
            
            //change if want segment value to be on display instead of the manager
            
            //EditorGUILayout.BeginHorizontal();
            //healthDisplay.segmentValue = (float)EditorGUILayout.FloatField("Segment Value",healthDisplay.segmentValue);
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay._spacing = (float)EditorGUILayout.FloatField("Segment Spacing",healthDisplay._spacing);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay.fillColor = (Color)EditorGUILayout.ColorField("Fill Color",healthDisplay.fillColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay.backgroundColor = (Color)EditorGUILayout.ColorField("Background Color",healthDisplay.backgroundColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay.singlePrefab = (bool)EditorGUILayout.Toggle("Single Segment",healthDisplay.singlePrefab);
            EditorGUILayout.EndHorizontal();

            if(healthDisplay.singlePrefab)
            {
                EditorGUILayout.BeginHorizontal();
                healthDisplay.segmentPrefab = (GameObject)EditorGUILayout.ObjectField("Segment Prefab",healthDisplay.segmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                healthDisplay.firstSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("First Segment Prefab",healthDisplay.firstSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                healthDisplay.middleSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("Middle Segment Prefab",healthDisplay.middleSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                healthDisplay.lastSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("Last Segment Prefab",healthDisplay.lastSegmentPrefab,typeof(GameObject),false);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            healthDisplay.follow = (bool)EditorGUILayout.Toggle("Follow",healthDisplay.follow);
            EditorGUILayout.EndHorizontal();

            if(healthDisplay.follow)
            {
                EditorGUILayout.BeginHorizontal();
                healthDisplay.followTime = (float)EditorGUILayout.FloatField("Follow Time",healthDisplay.followTime);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                healthDisplay.followGainColor = (Color)EditorGUILayout.ColorField("Follow Gain Color",healthDisplay.followGainColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                healthDisplay.followLoseColor = (Color)EditorGUILayout.ColorField("Follow Lose Color",healthDisplay.followLoseColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                healthDisplay.unscaledTime = (bool)EditorGUILayout.Toggle("Unscaled Time",healthDisplay.unscaledTime);
                EditorGUILayout.EndHorizontal();
            }

            if(GUI.changed)
            {
                //it all works withouth this, but prefer to keep it just in case

                Undo.RecordObject(this,"");
                Undo.RecordObject(target,"");
                Undo.RecordObject(healthDisplay,"");

                SaveChanges();
                
                EditorUtility.SetDirty(this);
                //EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(healthDisplay);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
#endif
    #endregion
}
