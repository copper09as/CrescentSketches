using Godot;

public partial class RewardSystem : Node 
{
    public static RewardSystem Instance;
    void Awake() => Instance = this;

    public void CalculateRewards(string outcome, bool isCruelEvent)
    {
        int stars = CalculateStars(outcome);

        // 基础奖励
        //GameManager.Instance.playerData.moonTears += stars;
        GameManager.Instance.playerData.observerExp += 30 + (stars * 20);

        // 特殊奖励
        if (isCruelEvent)
        {
            GameManager.Instance.playerData.painCrystals++;
        }
        if (outcome.Contains("连锁"))
        {
            GameManager.Instance.playerData.regretDust += 2;
        }
        if (outcome.Contains("真相"))
        {
            GameManager.Instance.playerData.coldLightShards++;
        }

        // 检查升级
        CheckLevelUp();

    }
    
    private int CalculateStars(string outcome) {
        // 根据结局文本中的★数量计算
        int count = 0;
        foreach (char c in outcome) {
            if (c == '★') count++;
        }
        return Mathf.Clamp(count, 1, 5);
    }
    
    private void CheckLevelUp() {
        int requiredExp = GameManager.Instance.playerData.observerLevel * 100;
        if (GameManager.Instance.playerData.observerExp >= requiredExp) {
            GameManager.Instance.playerData.observerLevel++;
            GameManager.Instance.playerData.observerExp = 0;
            UnlockLevelReward();
        }
    }
    
    private void UnlockLevelReward() {
        int level = GameManager.Instance.playerData.observerLevel;
        if (level == 5) {
            GameManager.Instance.playerData.unlockedPowers.Add("memory_rewind");
        } else if (level == 10) {
            GameManager.Instance.playerData.unlockedPowers.Add("omen_eye");
        } else if (level == 15) {
            GameManager.Instance.playerData.unlockedPowers.Add("dark_shadow");
        }
    }
}