using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphDescriptor))]
public class FancyGraphDesciptorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var descriptor = target as GraphDescriptor;

        GUILayout.Label("Features", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableAggregateTextStructure)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan))); 
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableConsistsOfSentencesIntegratingRelationships)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableGenerateReferringExpressionsEntities)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableGenerateReferringExpressionsSentences)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(descriptor.EnableCentralElementInversions)));

        if (serializedObject.ApplyModifiedProperties())
        {
            descriptor.RegenerateTexualDescription();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Description Text", EditorStyles.boldLabel);

        GUILayout.TextArea(descriptor.TextualDescription);
    }
}
