using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 💾 Handles saving and loading game data using JSON
/// Works on all platforms including Android
/// </summary>
public class SaveSystem : Singleton<SaveSystem>
{
    private string saveFilePath;
    private SaveData currentSaveData;

    protected override void Awake()
    {
        base.Awake();
        
        // Create Save folder in persistent data path
        string saveFolder = Path.Combine(Application.persistentDataPath, "Save");
        
        // Create folder if it doesn't exist
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        // Save file inside Save folder
        saveFilePath = Path.Combine(saveFolder, "savedata.json");
        MizuLog.Save($"💾 Save file path: {saveFilePath}");
    }

    /// <summary>
    /// 📥 Load save data from file
    /// </summary>
    public SaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                MizuLog.Save($"✅ Game loaded successfully! {currentSaveData.upgradeLevels.Count} upgrades found.");
                return currentSaveData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load game: {e.Message}");
                return CreateNewSave();
            }
        }
        else
        {
            MizuLog.Save("📁 No save file found, creating new save.");
            return CreateNewSave();
        }
    }

    /// <summary>
    /// 💾 Save current game data to file
    /// </summary>
    public void SaveGame(SaveData data)
    {
        try
        {
            currentSaveData = data;
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            MizuLog.Save("💾 Game saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// 🔄 Update a specific upgrade level in save data
    /// </summary>
    public void UpdateUpgradeLevel(string upgradeName, int level)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        // Update or add the upgrade level
        bool found = false;
        for (int i = 0; i < currentSaveData.upgradeLevels.Count; i++)
        {
            if (currentSaveData.upgradeLevels[i].upgradeName == upgradeName)
            {
                currentSaveData.upgradeLevels[i].level = level;
                found = true;
                break;
            }
        }

        if (!found)
        {
            currentSaveData.upgradeLevels.Add(new UpgradeLevelData
            {
                upgradeName = upgradeName,
                level = level
            });
        }

        SaveGame(currentSaveData);
    }

    /// <summary>
    /// 🔍 Get upgrade level from save data
    /// </summary>
    public int GetUpgradeLevel(string upgradeName)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        foreach (var upgrade in currentSaveData.upgradeLevels)
        {
            if (upgrade.upgradeName == upgradeName)
            {
                return upgrade.level;
            }
        }

        return 0; // Default level if not found
    }

    /// <summary>
    /// 🆕 Create new save data
    /// </summary>
    private SaveData CreateNewSave()
    {
        currentSaveData = new SaveData
        {
            upgradeLevels = new List<UpgradeLevelData>(),
            currentStageSelected = 1,
            maxUnlockedStage = 1,
            playerLevel = 1
        };
        SaveGame(currentSaveData);
        return currentSaveData;
    }

    /// <summary>
    /// 🎮 Save stage data
    /// </summary>
    public void SaveStageData(int currentStage, int maxUnlocked)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.currentStageSelected = currentStage;
        currentSaveData.maxUnlockedStage = maxUnlocked;
        SaveGame(currentSaveData);
        MizuLog.Save($"💾 Stage data saved: Current={currentStage}, Max={maxUnlocked}");
    }

    /// <summary>
    /// 📥 Load stage data
    /// </summary>
    public void LoadStageData(out int currentStage, out int maxUnlocked)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentStage = currentSaveData.currentStageSelected;
        maxUnlocked = currentSaveData.maxUnlockedStage;
        MizuLog.Save($"📥 Stage data loaded: Current={currentStage}, Max={maxUnlocked}");
    }

    /// <summary>
    /// 👤 Save player level
    /// </summary>
    public void SavePlayerLevel(int level)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.playerLevel = level;
        SaveGame(currentSaveData);
        MizuLog.Save($"💾 Player level saved: {level}");
    }
    
    /// <summary>
    /// 🌟 Save player exp
    /// </summary>
    public void SavePlayerExp(int exp)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.playerExp = exp;
        SaveGame(currentSaveData);
        MizuLog.Save($"💾 Player exp saved: {exp}");
    }
    
    /// <summary>
    /// 🌟 Save player level and exp together
    /// </summary>
    public void SavePlayerLevelAndExp(int level, int exp)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.playerLevel = level;
        currentSaveData.playerExp = exp;
        SaveGame(currentSaveData);
        MizuLog.Save($"💾 Player level & exp saved: Lv{level}, Exp{exp}");
    }

    /// <summary>
    /// 📥 Get player level
    /// </summary>
    public int GetPlayerLevel()
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        return currentSaveData.playerLevel;
    }
    
    /// <summary>
    /// 🌟 Get player exp
    /// </summary>
    public int GetPlayerExp()
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        return currentSaveData.playerExp;
    }

    /// <summary>
    /// � Save currency by type
    /// </summary>
    public void SaveCurrency(EnumCurrency currencyType, int amount)
    {
        if (currentSaveData == null)
            currentSaveData = LoadGame();

        switch (currencyType)
        {
            case EnumCurrency.yellowBits: currentSaveData.yellowBits = amount; break;
            case EnumCurrency.blueBits: currentSaveData.blueBits = amount; break;
            case EnumCurrency.pinkBits: currentSaveData.pinkBits = amount; break;
            case EnumCurrency.greenBits: currentSaveData.greenBits = amount; break;
            case EnumCurrency.xpBits: currentSaveData.xpBits = amount; break;
        }
        
        SaveGame(currentSaveData);
        MizuLog.Save($"💰 {currencyType} saved: {amount}");
    }

    /// <summary>
    /// 💰 Save all currencies from PlayerStats
    /// </summary>
    public void SaveAllCurrenciesFromPlayerStats(PlayerStats playerStats)
    {
        if (playerStats == null) return;
        if (currentSaveData == null)
            currentSaveData = LoadGame();
        
        currentSaveData.yellowBits = playerStats.GetCurrency(EnumCurrency.yellowBits);
        currentSaveData.blueBits = playerStats.GetCurrency(EnumCurrency.blueBits);
        currentSaveData.pinkBits = playerStats.GetCurrency(EnumCurrency.pinkBits);
        currentSaveData.greenBits = playerStats.GetCurrency(EnumCurrency.greenBits);
        currentSaveData.xpBits = playerStats.GetCurrency(EnumCurrency.xpBits);
        
        SaveGame(currentSaveData);
        MizuLog.Save($"💰 All currencies saved: Y={currentSaveData.yellowBits}, B={currentSaveData.blueBits}, P={currentSaveData.pinkBits}, G={currentSaveData.greenBits}, XP={currentSaveData.xpBits}");
    }

    /// <summary>
    /// 💰 Get currency by type
    /// </summary>
    public int GetCurrency(EnumCurrency currencyType)
    {
        if (currentSaveData == null)
            currentSaveData = LoadGame();

        switch (currencyType)
        {
            case EnumCurrency.yellowBits: return currentSaveData.yellowBits;
            case EnumCurrency.blueBits: return currentSaveData.blueBits;
            case EnumCurrency.pinkBits: return currentSaveData.pinkBits;
            case EnumCurrency.greenBits: return currentSaveData.greenBits;
            case EnumCurrency.xpBits: return currentSaveData.xpBits;
            default: return 0;
        }
    }

    /// <summary>
    /// �🗑️ Delete save file (for testing)
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentSaveData = null;
            MizuLog.Save("🗑️ Save file deleted!");
        }
    }    
    /// <summary>
    /// 📚 Save tutorial shown state by key
    /// </summary>
    public void SaveTutorialShown(string key, bool shown)
    {
        if (currentSaveData == null)
            currentSaveData = LoadGame();
        
        if (key == "tutorialCombatShown")
            currentSaveData.tutorialCombatShown = shown;
        else if (key == "tutorialUpgradeShown")
            currentSaveData.tutorialUpgradeShown = shown;
        
        SaveGame(currentSaveData);
        MizuLog.Save($"📚 Tutorial [{key}] saved: {shown}");
    }
    
    /// <summary>
    /// 📚 Get tutorial shown state by key
    /// </summary>
    public bool GetTutorialShown(string key)
    {
        if (currentSaveData == null)
            currentSaveData = LoadGame();
        
        if (key == "tutorialCombatShown")
            return currentSaveData.tutorialCombatShown;
        else if (key == "tutorialUpgradeShown")
            return currentSaveData.tutorialUpgradeShown;
        
        return false;
    }
}

/// <summary>
/// 📦 Main save data structure
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UpgradeLevelData> upgradeLevels = new List<UpgradeLevelData>();
    
    // 🎮 Level/Stage data
    public int currentStageSelected = 1;
    public int maxUnlockedStage = 1;
    
    // 👤 Player level and exp
    public int playerLevel = 1;
    public int playerExp = 0;
    
    // 💰 Currencies
    public int yellowBits = 0;
    public int blueBits = 0;
    public int pinkBits = 0;
    public int greenBits = 0;
    public int xpBits = 0;
    
    // 📚 Tutorials
    public bool tutorialCombatShown = false;
    public bool tutorialUpgradeShown = false;
    
    // 📺 Ads
    public int interstitialPlayCount = 0;
    
    // 🎁 Daily Reward
    public int dailyRewardClaimedCount = 0;
    public string dailyRewardLastClaimDate = "";
}

/// <summary>
/// 📊 Individual upgrade level data
/// </summary>
[System.Serializable]
public class UpgradeLevelData
{
    public string upgradeName;
    public int level;
}
