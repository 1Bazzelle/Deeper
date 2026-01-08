using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(UnlockCondition))]
public class UnlockConditionEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProperty = property.FindPropertyRelative("type");
        SerializedProperty upgradeNecessary = property.FindPropertyRelative("upgradeNecessary");
        SerializedProperty researcher = property.FindPropertyRelative("researcher");
        SerializedProperty researcherProgressNecessary = property.FindPropertyRelative("researcherProgressNecessary");
        SerializedProperty creature = property.FindPropertyRelative("creature");
        SerializedProperty creatureProgressNecessary = property.FindPropertyRelative("creatureProgressNecessary");
        SerializedProperty otherMission = property.FindPropertyRelative("otherMission");

        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, typeProperty, new GUIContent("Condition Type"));
        position.y += EditorGUIUtility.singleLineHeight * 2;

        switch ((ConditionType)typeProperty.enumValueIndex)
        {
            case ConditionType.Upgrade:
                EditorGUI.PropertyField(position, upgradeNecessary, new GUIContent("Upgrade"));
                break;
            case ConditionType.Researcher:
                EditorGUI.PropertyField(position, researcher, new GUIContent("Name"));
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, researcherProgressNecessary, new GUIContent("Progress"));
                break;
            case ConditionType.CreatureProgress:
                EditorGUI.PropertyField(position, creature, new GUIContent("Name"));
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, creatureProgressNecessary, new GUIContent("Progress"));
                break;
            case ConditionType.SpecificMission:
                EditorGUI.PropertyField(position, otherMission, new GUIContent("Other Mission"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProperty = property.FindPropertyRelative("type");
        switch ((ConditionType)typeProperty.enumValueIndex)
        {
            case ConditionType.Upgrade:
                return EditorGUIUtility.singleLineHeight * 3;
            case ConditionType.Researcher:
                return EditorGUIUtility.singleLineHeight * 4;
            case ConditionType.CreatureProgress:
                return EditorGUIUtility.singleLineHeight * 4;
            case ConditionType.SpecificMission:
                return EditorGUIUtility.singleLineHeight * 3;
            default:
                return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif