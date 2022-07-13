namespace SimpleRecyclerCollection.Core
{
    using UnityEngine;

    using UnityEditor;

    [CustomEditor(typeof(Collection), true)]
    [CanEditMultipleObjects]
    public class CollectionEditor : Editor
    {
        private Collection _collection;

        // Methods

        private void OnEnable() => _collection = target as Collection;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), new GUILayoutOption[0]);

            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Content"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Direction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Movement"));

            if (_collection.Movement == MovementType.Elastic)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Elasticity"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Inertia"));

            if (_collection.Inertia)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DecelerationRate"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Viewport"));

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnValueChanged"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}