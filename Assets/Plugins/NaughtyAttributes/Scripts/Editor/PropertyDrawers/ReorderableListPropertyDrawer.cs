using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace NaughtyAttributes.Editor
{
    [PropertyDrawer(typeof(ReorderableListAttribute))]
    public class ReorderableListPropertyDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> reorderableListsByPropertyName = new Dictionary<string, ReorderableList>();

        public override void DrawProperty(SerializedProperty property)
        {
            EditorDrawUtility.DrawHeader(property);

            if (property.isArray)
            {
                if (!this.reorderableListsByPropertyName.ContainsKey(property.name))
                {
                    ReorderableList reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, string.Format("{0}: {1}", property.displayName, property.arraySize), EditorStyles.label); },

                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                        {
                            var element = property.GetArrayElementAtIndex(index);
                            rect.y += 2f;
                            rect.x += 10f;
                            rect.width -= 10f;
                            
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, 0), element, true);
                        },
                        elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)) + 5
                    };

                    this.reorderableListsByPropertyName[property.name] = reorderableList;
                }

                this.reorderableListsByPropertyName[property.name].DoLayoutList();
            }
            else
            {
                string warning = typeof(ReorderableListAttribute).Name + " can be used only on arrays or lists";
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
                Debug.LogWarning(warning, PropertyUtility.GetTargetObject(property));

                EditorDrawUtility.DrawPropertyField(property);
            }
        }

        public override void ClearCache()
        {
            this.reorderableListsByPropertyName.Clear();
        }
    }
}