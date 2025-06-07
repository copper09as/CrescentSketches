using Godot;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int moonTears;
    public int painCrystals;
    public int regretDust;
    public int coldLightShards;
    public int observerLevel;
    public int observerExp;
    public List<string> unlockedPowers = new List<string>();
    public List<string> collectedMasks = new List<string>();
    public Dictionary<string, int> regionCorruption = new Dictionary<string, int>();
     public string lastRecoveryTime; // 新增字段

    
}