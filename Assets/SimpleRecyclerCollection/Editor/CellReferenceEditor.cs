namespace SimpleRecyclerCollection.Core
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
            float lineHeight = itemsProperty.arraySize * 4 * _singleLineSpaceHeight + _singleLineHeight;
            return lineHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            Vector2 position = rect.position;
            rect.position = position;
            rect.height = _singleLineHeight;

            SerializedProperty baseTypeProperty = property.FindPropertyRelative("_baseType");

            GUI.enabled = false;
            RawPropertyField(property, "_baseType", "Base Type", ref rect);
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(baseTypeProperty.stringValue))
            {
                Type baseType = Type.GetType(baseTypeProperty.stringValue);
               
                if (baseType != null)
                    OnMainGUI(ref rect, property, baseType);
                else
                {
                    GUI.color = Color.red;
                    RawLabelField("Create at least one cell data class.", ref rect, new GUIStyle("helpBox"));
                    GUI.color = Color.white;
                }
            }
        }

        private void OnMainGUI(ref Rect rect, SerializedProperty property, Type baseType)
        {
            List<Type> options = Assembly.GetAssembly(baseType).GetTypes().Where(t => t.IsSubclassOf(baseType)).ToList();
            options.Insert(0, baseType);

            SerializedProperty itemsProperty = property.FindPropertyRelative("_references");

            // Remove elements that have lost their link.
            for (int i = itemsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty item = itemsProperty.GetArrayElementAtIndex(i);
                string assemblyQualifiedName = item.FindPropertyRelative("_assemblyQualifiedName").stringValue;

                Type t = Type.GetType(assemblyQualifiedName);

                if (options.Contains(t))
                    options.Remove(t);
                else
                    itemsProperty.DeleteArrayElementAtIndex(i);
            }

            // Insert new items.
            for (int i = 0; i < options.Count; i++)
            {
                itemsProperty.InsertArrayElementAtIndex(0);
                SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(0);
                itemProperty.FindPropertyRelative("_assemblyQualifiedName").stringValue = options[i].AssemblyQualifiedName;
                itemProperty.FindPropertyRelative("_type").stringValue = options[i].Name;
            }

            // Draw items.
            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                EditorGUI.indentLevel++;
                RawLabelField($"Item ({i})", ref rect);
                EditorGUI.indentLevel++;
                SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(i);
                GUI.enabled = false;
                RawPropertyField(itemProperty, "_assemblyQualifiedName", "Assembly Qualified Name", ref rect);
                RawPropertyField(itemProperty, "_type", "Data", ref rect);
                GUI.enabled = true;
                RawPropertyField(itemProperty, "_view", $"View", ref rect);
                EditorGUI.indentLevel-= 2;
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

        private SerializedProperty RawPropertyField(SerializedProperty property, string propertyName, string displayName, ref Rect screenRect)
        {
            SerializedProperty serializedProperty = property.FindPropertyRelative(propertyName);
            EditorGUI.PropertyField(screenRect, serializedProperty, new GUIContent(displayName));
            screenRect.position = new Vector2(screenRect.position.x, screenRect.position.y + _singleLineSpaceHeight);
            return serializedProperty;
        }
    }
}