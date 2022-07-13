namespace SimpleRecyclerCollection
{
    using UnityEngine;

    using UnityEditor;

    [CustomEditor(typeof(CollectionLayoutGroup))]
    [CanEditMultipleObjects]
    public class CollectionLayoutGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true, new GUILayoutOption[0]);

            GUI.enabled = true;

            CollectionLayoutGroup layoutGroup = target as CollectionLayoutGroup;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoTuples"));

            if (!layoutGroup.AutoTuples)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_tupleCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_expand"));
            }

            if (!layoutGroup.Expand)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_align"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_padding"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spacing"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}