using DynamicScroll;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof (DynamicScrollViewBase), true)]
[CanEditMultipleObjects]
public class DynamicScrollViewEditor : ScrollRectEditor
{
    private DynamicScrollViewBase scrollView;
    protected override void OnEnable()
    {
        base.OnEnable();
        scrollView = target as DynamicScrollViewBase;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Blahblahblahblah from DynamicScrollViewEditor");

        EditorGUILayout.Space();
        scrollView.spacing = EditorGUILayout.FloatField("Spacing", scrollView.spacing);
        EditorGUILayout.Space();

        base.OnInspectorGUI();
    }
}