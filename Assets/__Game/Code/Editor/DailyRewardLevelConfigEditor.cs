using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DailyRewardLevelConfig))]
public class DailyRewardLevelConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DailyRewardLevelConfig config = (DailyRewardLevelConfig)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            $"Preview: Stage 1 base = {config.baseValue}\n" +
            $"Within stage: btn0 = base, btn1 = base×2, ... btn4 = base×5\n" +
            $"Stage 2 base = {Mathf.RoundToInt(config.baseValue * config.baseMultiplyPerStage)}, " +
            $"Stage 3 base = {Mathf.RoundToInt(config.baseValue * config.baseMultiplyPerStage * config.baseMultiplyPerStage)}, ...",
            MessageType.Info);

        EditorGUILayout.Space(5);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎁 Auto Generate Rewards", GUILayout.Height(35)))
        {
            GenerateRewards(config);
        }
        GUI.backgroundColor = Color.white;
    }

    private void GenerateRewards(DailyRewardLevelConfig config)
    {
        Undo.RecordObject(config, "Auto Generate Daily Rewards");

        config.levelRewards.Clear();

        float currentBase = config.baseValue;

        for (int stage = 0; stage < config.stageCount; stage++)
        {
            var stageLevel = new DailyRewardLevel();
            stageLevel.rewards = new List<DailyRewardEntry>();

            for (int btn = 0; btn < config.buttonsPerStage; btn++)
            {
                var entry = new DailyRewardEntry();
                entry.currencyType = EnumCurrency.blueBits;
                entry.amount = Mathf.RoundToInt(currentBase * (btn + 1));
                stageLevel.rewards.Add(entry);
            }

            config.levelRewards.Add(stageLevel);

            // Next stage base = current base × multiplier
            currentBase *= config.baseMultiplyPerStage;
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();

        Debug.Log($"[DailyRewardLevelConfig] Generated {config.stageCount} stages × {config.buttonsPerStage} buttons. Base={config.baseValue}, Multiply={config.baseMultiplyPerStage}");
    }
}
