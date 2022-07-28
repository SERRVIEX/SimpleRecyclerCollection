namespace SimpleRecyclerCollection
{
    using UnityEditor;

    using Core;

    [CustomEditor(typeof(RecyclerCollection<,>), true)]
    [CanEditMultipleObjects]
    public class RecyclerCollectionEditor : CollectionEditor
    {
        private string[] _excludeProperties = new string[] 
        { 
            "m_Script",
            "m_Content",
            "m_Direction",
            "m_Movement",
            "m_Elasticity",
            "m_Inertia",
            "m_DecelerationRate",
            "m_Viewport",
            "m_OnValueChanged",
            "_cellPrefab",
            "_cachedCellCount"
        };

        // Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SimpleEditor.Initialize();

            SimpleEditor.Header("Cells");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cellPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cachedCellCount"));

            SimpleEditor.Header("Other Properties");

            DrawPropertiesExcluding(serializedObject, _excludeProperties);

            serializedObject.ApplyModifiedProperties();
        }
    }
}