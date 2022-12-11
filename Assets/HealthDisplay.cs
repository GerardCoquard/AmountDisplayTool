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
    [SerializeField] float segmentValue;
    [SerializeField] bool singlePrefab;
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] GameObject firstSegmentPrefab;
    [SerializeField] GameObject middleSegmentPrefab;
    [SerializeField] GameObject lastSegmentPrefab;
    HorizontalLayoutGroup layoutGroup;
    List<Image> fills;
    private void OnEnable() {
        HealthSystem.OnSetHealth += InitializeAll;
        HealthSystem.OnHealthChanged += UpdateHealthBar;
    }
    private void OnDisable() {
        HealthSystem.OnSetHealth -= InitializeAll;
        HealthSystem.OnHealthChanged -= UpdateHealthBar;
    }
    private void Awake() {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = _spacing;
        fills = new List<Image>();
    }
    void InitializeAll(float health,float maxHP)
    {
        StopAllCoroutines();
        SetSegments(maxHP);
    }
    void UpdateHealthBar(float actualHealth,float previousHealth,bool damage)
    {
        
    }
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
        SetFills();
    }
    void AddSegments(int amount)
    {
        if(transform.childCount != 0)
        {
            DeleteSegment();
            CreateSegment(singlePrefab ? segmentPrefab : middleSegmentPrefab);
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
        CreateSegment(singlePrefab ? segmentPrefab : lastSegmentPrefab);
    }
    void CreateSegment(GameObject prefab)
    {
        GameObject segment = Instantiate(prefab,transform.position,Quaternion.identity,transform);
        fills.Add(segment.GetComponent<HealthFill>().GetFill());
    }
    void DeleteSegment()
    {
        GameObject segment = transform.GetChild(transform.childCount-1).gameObject;
        fills.Remove(segment.GetComponent<HealthFill>().GetFill());
        DestroyImmediate(segment);
    }
    void SetFills()
    {

    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(HealthDisplay)), CanEditMultipleObjects]
    public class HealthDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            HealthDisplay healthDisplay = (HealthDisplay)target;

            EditorGUILayout.BeginHorizontal();
            healthDisplay.segmentValue = (float)EditorGUILayout.FloatField("Segment Value",healthDisplay.segmentValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay._spacing = (float)EditorGUILayout.FloatField("Segment Spacing",healthDisplay._spacing);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            healthDisplay.singlePrefab = (bool)EditorGUILayout.Toggle("Single Prefab",healthDisplay.singlePrefab);
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

            if(GUI.changed)
            {
                Undo.RecordObject(this,"q");
                Undo.RecordObject(target,"b");
                Undo.RecordObject(healthDisplay,"a");

                SaveChanges();
                
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(healthDisplay);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
#endif
    #endregion
}
