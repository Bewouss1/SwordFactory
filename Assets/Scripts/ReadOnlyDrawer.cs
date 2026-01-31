using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// PropertyDrawer pour afficher les champs ReadOnly en grisé et non modifiables
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Sauvegarde l'état actuel
        bool previousGUIState = GUI.enabled;
        
        // Désactive l'édition
        GUI.enabled = false;
        
        // Affiche le champ
        EditorGUI.PropertyField(position, property, label, true);
        
        // Restaure l'état
        GUI.enabled = previousGUIState;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
