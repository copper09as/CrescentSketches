using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class PlayerUi : FlowUi
{
    [Export]private Label _displayLabel;

    public override void _Ready()
    {
        base._Ready();
    }
    public override void UpdateUi()
    {
        base.UpdateUi();

        var gameData = SaveSystem.Load<GameData>(StringResource.playerData);
        // 构建各模块内容
        string resources = BuildResourcesSection(gameData);
        string observer = BuildObserverSection(gameData);
        string powers = BuildListSection(gameData.unlockedPowers, "特殊能力");
        string masks = BuildListSection(gameData.collectedMasks, "收集进度");
        string regions = BuildDictionarySection(gameData.regionCorruption, "区域状态");

        // 组合完整内容
        string fullText = $"{resources}{observer}{powers}{masks}{regions}";
        
        // 应用样式并更新
        _displayLabel.Text = fullText;
    }  
    
    

    // 资源模块构建
    private string BuildResourcesSection(GameData gameData)
    {
        return $"[b]资源储备[/b]\n" +
            $"[Moon Tears]         ×{gameData.moonTears}\n" +
            $"[Pain Crystals]      ×{gameData.painCrystals}\n" +
            $"[Regret Dust]        ×{gameData.regretDust}\n" +
            $"[Cold Light Shards]  ×{gameData.coldLightShards}\n\n";
    }

    // 观察者模块构建
    private string BuildObserverSection(GameData gameData)
    {
        return $"[b]观察者状态[/b]\n" +
            $"等级: Lv.{gameData.observerLevel}\n" +
            $"经验值: {gameData.observerExp}/"+GameManager.Instance.playerData.observerLevel * 100+"\n\n";
    }

    // 列表型数据模块构建（通用）
    private string BuildListSection(IEnumerable<string> items, string title)
    {
        if(items == null || !items.Any())
            return $"[b]{title}[/b]\n▷ 无\n\n";

        return $"[b]{title}[/b]\n" +
            string.Join("\n", items.Select(i => $"▷ {i.Capitalize()}")) + "\n\n";
    }

    // 字典型数据模块构建（通用）
    private string BuildDictionarySection(Dictionary<string, int> data, string title)
    {
        if(data == null || !data.Any())
            return $"[b]{title}[/b]\n▷ 未检测到异常数据\n\n";

        return $"[b]{title}[/b]\n" +
            string.Join("\n", data.Select(kv => $"▷ {kv.Key} [{kv.Value}%]")) + "\n";
    }
}