namespace SimpleRecyclerCollection
{
    using UnityEngine;

    using UnityEditor;

    using Core;

    [CustomEditor(typeof(RecyclerCollection<,>), true)]
    [CanEditMultipleObjects]
    public class RecyclerCollectionEditor : CollectionEditor
    {
        private GUIStyle _headerStyle;

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

        private void OnGUI() => InitializeStyles();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InitializeStyles();

            TitleBar("Cells");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cellPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cachedCellCount"));

            TitleBar("Other Properties");

            DrawPropertiesExcluding(serializedObject, _excludeProperties);

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.box);
                _headerStyle.normal.textColor = Color.white;
                _headerStyle.normal.background = CreateTexture(2, 2, new Color32(62, 62, 62, 255));
            }
        }

        private Texture2D CreateTexture(int width, int height, Color col)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private void TitleBar(string title)
        {
            EditorGUILayout.Space(5);

            BeginHorizontalLine();

            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            rect.position = new Vector2(0, rect.position.y);
            GUIStyle titleStyle = new GUIStyle(_headerStyle);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fixedWidth = EditorGUIUtility.currentViewWidth;
            titleStyle.stretchWidth = true;
            EditorGUI.LabelField(rect, title, titleStyle);

            EndHorizontalLine();
        }

        private void BeginHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, -1);
            rect.position = new Vector2(0, rect.position.y);
            rect.width = EditorGUIUtility.currentViewWidth;
            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color32(26, 26, 26, 255));
        }

        private void EndHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.position = new Vector2(0, rect.position.y -2);
            rect.width = EditorGUIUtility.currentViewWidth;
            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color32(48, 48, 48, 255));
        }
    }
}