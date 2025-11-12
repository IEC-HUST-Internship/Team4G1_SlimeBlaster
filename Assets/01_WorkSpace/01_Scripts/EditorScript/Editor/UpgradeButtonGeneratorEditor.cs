using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(UpgradeButtonGenerator))]
public class UpgradeButtonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpgradeButtonGenerator generator = (UpgradeButtonGenerator)target;

        if (GUILayout.Button("Generate Upgrade Buttons"))
        {
            generator.GenerateButtons();
        }
    }
}
