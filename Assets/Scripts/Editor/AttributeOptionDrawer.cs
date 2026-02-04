using UnityEngine;
using UnityEditor;

/// <summary>
/// PropertyDrawer personnalisé pour afficher AttributeOption avec tous les champs en read-only
/// </summary>
[CustomPropertyDrawer(typeof(SwordAttributesConfig.AttributeOption))]
public class AttributeOptionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Désactive l'édition pour tous les champs
        bool previousGUIState = GUI.enabled;
        GUI.enabled = false;
        
        EditorGUI.BeginProperty(position, label, property);
        
        // Affiche le foldout pour la structure
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true
        );
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // Affiche chaque champ
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty weightProp = property.FindPropertyRelative("weight");
            SerializedProperty multiplierProp = property.FindPropertyRelative("multiplier");
            SerializedProperty colorProp = property.FindPropertyRelative("color");
            
            // Name
            EditorGUI.PropertyField(
                new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
                nameProp
            );
            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // Weight
            EditorGUI.PropertyField(
                new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
                weightProp
            );
            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // Multiplier
            EditorGUI.PropertyField(
                new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
                multiplierProp
            );
            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // Color
            EditorGUI.PropertyField(
                new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
                colorProp
            );
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
        
        // Restaure l'état
        GUI.enabled = previousGUIState;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        // Hauteur pour le foldout + 4 champs (name, weight, multiplier, color)
        return EditorGUIUtility.singleLineHeight + 
               (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
    }
}
