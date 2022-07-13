namespace SimpleRecyclerCollection
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    using UnityEngine;

    using UnityEditor;

    [ExecuteInEditMode]
    [CustomPropertyDrawer(typeof(CellReference<,>), true)]
    public class CellReferenceEditor : PropertyDrawer
    {
        private float _singleLineHeight => EditorGUIUtility.singleLineHeight;
        private float _singleLineSpaceHeight => _singleLineHeight + 2;

        // Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProperty = property.FindPropertyRelative("_references");
            float lineHeight = itemsProperty.arraySize * _singleLineSpaceHeight + _singleLineHeight;
            return lineHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            Vector2 position = rect.position;
            rect.position = position;
            rect.height = _singleLineHeight;

            SerializedProperty baseTypeProperty = property.FindPropertyRelative("_baseType");
            if (!string.IsNullOrEmpty(baseTypeProperty.stringValue))
            {
                Type baseType = Type.GetType(baseTypeProperty.stringValue);
                if (baseType != null)
                {
                    OnMainGUI(ref rect, property, baseTypeProperty, baseType);
                }
                else
                {
                    GUI.color = Color.red;
                    RawLabelField("Create at least one cell data class.", ref rect);
                    GUI.color = Color.white;
                }
            }
        }

        private void OnMainGUI(ref Rect rect, SerializedProperty property, SerializedProperty baseType, Type type)
        {
            var subclassTypes = Assembly.GetAssembly(type).GetTypes().Where(t => t.IsSubclassOf(type));
            List<string> options = new List<string>();
            options.Add(baseType.stringValue);
            foreach (var item in subclassTypes)
                options.Add(item.ToString());

            List<string> options2 = new List<string>();

            SerializedProperty itemsProperty = property.FindPropertyRelative("_references");

            // Remove elements that have lost their link.
            for (int i = itemsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty item = itemsProperty.GetArrayElementAtIndex(i);
                string classType = item.FindPropertyRelative("_type").stringValue;

                if (options.Contains(classType))
                    options.Remove(classType);
                else
                    itemsProperty.DeleteArrayElementAtIndex(i);
            }

            // Insert new items.
            for (int i = 0; i < options.Count; i++)
            {
                itemsProperty.InsertArrayElementAtIndex(0);
                SerializedProperty item = itemsProperty.GetArrayElementAtIndex(0);
                SerializedProperty itemTypeProperty = item.FindPropertyRelative("_type");
                itemTypeProperty.stringValue = options[i];
            }

            var boldtext = new GUIStyle(GUI.skin.label);
            boldtext.fontStyle = FontStyle.Bold;

            GUI.color = new Color32(145, 210, 163, 255);
            RawLabelField("TCellData", "TCellView", ref rect, boldtext);
            GUI.color = Color.white;

            // Draw items.
            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty itemTypeProperty = itemProperty.FindPropertyRelative("_type");
                RawPropertyField(itemProperty, "_view", $"{itemTypeProperty.stringValue}", ref rect);
            }
        }

        private void RawLabelField(string value, ref Rect screenRect, GUIStyle style = null)
        {
            if (style == null)
                EditorGUI.LabelField(screenRect, value);
            else
                EditorGUI.LabelField(screenRect, value, style);

            screenRect.position = new Vector2(screenRect.position.x, screenRect.position.y + _singleLineSpaceHeight);
        }

        private void RawLabelField(string value,  string value2, ref Rect screenRect, GUIStyle style = null)
        {
            if (style == null)
                EditorGUI.LabelField(screenRect, value, value2);
            else
                EditorGUI.LabelField(screenRect, value, value2, style);
            screenRect.position = new Vector2(screenRect.position.x, screenRect.position.y + _singleLineSpaceHeight);
        }

        private SerializedProperty RawPropertyField(SerializedProperty property, string propertyName, string displayName, ref Rect screenRect)
        {
            SerializedProperty serializedProperty = property.FindPropertyRelative(propertyName);
            EditorGUI.PropertyField(screenRect, serializedProperty, new GUIContent(displayName));
            screenRect.position = new Vector2(screenRect.position.x, screenRect.position.y + _singleLineSpaceHeight);
            return serializedProperty;
        }
    }
}