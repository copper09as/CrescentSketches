using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance;
    public GameData playerData;
    public List<MicroEvent> allEvents = new List<MicroEvent>();
    private MicroEvent currentEvent;
    private int selectionIndex;
    [Export] EventMessage eventMessage;
    [Export] UiManager uiManager;
    [Export] RewardSystem rewardSystem;

    // 计时器相关变量
    private Timer _recoveryTimer;
    private DateTime _lastRecoveryTime;
    private const int MAX_MOON_TEARS = 40; // 最大泪滴数量
    private const int RECOVERY_INTERVAL_MINUTES = 1; // 检查间隔（分钟）

    public int MoonTears
    {
        get => playerData.moonTears;
        set
        {
            playerData.moonTears = Math.Clamp(value, 0, MAX_MOON_TEARS);
            if (eventMessage != null)
                eventMessage.SetMoonTxt(playerData.moonTears);

            UpdateUI();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        if (Instance == null)
            Instance = this;
        else
            QueueFree();

        // 创建并配置更频繁的检查计时器
        _recoveryTimer = new Timer
        {
            WaitTime = RECOVERY_INTERVAL_MINUTES * 60, // 每1分钟检查一次
            Autostart = true
        };
        AddChild(_recoveryTimer);
        _recoveryTimer.Timeout += OnRecoveryTimerTimeout;

        InitializeGame();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Instance = null;
        if (_recoveryTimer != null)
        {
            _recoveryTimer.Timeout -= OnRecoveryTimerTimeout;
        }

        // 退出时保存游戏
        SaveGame();
    }

    // 计时器回调方法
    private void OnRecoveryTimerTimeout()
    {
        RecoverMoonTears();
    }

    // 泪滴恢复方法（简化版）
    private void RecoverMoonTears(bool isInitialLoad = false)
    {
        try
        {
            // 添加恢复冷却检测
            if ((DateTime.Now - _lastRecoveryTime).TotalHours < 1 && !isInitialLoad)
                return;

            double hoursPassed = (DateTime.Now - _lastRecoveryTime).TotalHours;
            if (hoursPassed < 1)
            {
                GD.Print($"时间不足1小时 ({hoursPassed:F2}h)，跳过恢复");
                return;
            }

            // 修正：严格按整小时恢复（不依赖上限差值）
            int tearsToAdd = (int)Math.Floor(hoursPassed);
            int actualTears = Math.Min(tearsToAdd*3, MAX_MOON_TEARS - MoonTears);

            if (actualTears <= 0) return;

            MoonTears += actualTears;
            _lastRecoveryTime = DateTime.Now.AddHours(-(hoursPassed - tearsToAdd)); // 关键修复

            GD.Print($"恢复了{actualTears}滴泪，当前泪滴={MoonTears}");
            SaveRecoveryTime();
        }
        catch (Exception ex) { /*...*/ }
    }
    // 安全时间解析方法（支持多种格式）
    private DateTime SafeParseDateTime(string dateTimeString)
    {
        if (string.IsNullOrEmpty(dateTimeString))
            return DateTime.Now;

        try
        {
            // 尝试解析ISO格式（带Z的UTC时间）
            if (DateTime.TryParseExact(
                dateTimeString,
                "yyyy-MM-ddTHH:mm:ss.fffffffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out DateTime isoResult))
            {
                return isoResult.ToLocalTime();
            }

            // 尝试解析标准格式
            if (DateTime.TryParse(
                dateTimeString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime stdResult))
            {
                return stdResult;
            }

            // 尝试解析本地时间格式
            if (DateTime.TryParseExact(
                dateTimeString,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime localResult))
            {
                return localResult;
            }

            GD.PrintErr($"无法解析时间字符串: {dateTimeString}");
            return DateTime.Now;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"时间解析错误: {ex.Message}");
            return DateTime.Now;
        }
    }

    void LoadPlayerData()
    {
        try
        {
            // 从存档加载或初始化新数据
            playerData = SaveSystem.Load<GameData>(StringResource.playerData) ?? new GameData
            {
                moonTears = MAX_MOON_TEARS,
                painCrystals = 0,
                observerLevel = 1,
                observerExp = 0,
                unlockedPowers = new List<string> { "shadow_whisper" },
                lastRecoveryTime = DateTime.Now.ToString("o")
            };

            GD.Print($"存档中的恢复时间原始值: '{playerData.lastRecoveryTime}'");

            // 从保存的数据中恢复上次恢复时间
            _lastRecoveryTime = SafeParseDateTime(playerData.lastRecoveryTime);
            GD.Print($"解析后的恢复时间: {_lastRecoveryTime:yyyy-MM-dd HH:mm:ss}");

            // 计算存档时间与当前时间的差距
            TimeSpan timeDifference = DateTime.Now - _lastRecoveryTime;
            GD.Print($"时间差: {timeDifference.TotalHours:F2}小时");

            // 如果存档时间超过1小时，强制检查并恢复泪滴
            if (timeDifference.TotalHours > 1)
            {
                GD.Print("检测到离线时间，尝试恢复泪滴...");
                RecoverMoonTears();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"加载玩家数据失败: {ex.Message}");
            // 创建默认数据
            playerData = new GameData
            {
                moonTears = MAX_MOON_TEARS,
                painCrystals = 0,
                observerLevel = 1,
                observerExp = 0,
                unlockedPowers = new List<string> { "shadow_whisper" },
                lastRecoveryTime = DateTime.Now.ToString("o")
            };
            _lastRecoveryTime = DateTime.Now;
            MoonTears = MAX_MOON_TEARS;
        }
    }

    public void ProcessIntervention(int interventionIndex)
    {
        try
        {
            if (currentEvent == null || interventionIndex < 1 || interventionIndex > currentEvent.interventions.Count())
            {
                GD.PrintErr($"无效的干预索引: {interventionIndex}");
                return;
            }

            InterventionOption selected = currentEvent.interventions[interventionIndex - 1];
            int cost = selected.cost;

            // 检查泪滴是否足够
            if (MoonTears < cost)
            {
                GD.Print($"泪滴不足! 需要 {cost}，当前 {MoonTears}");
                eventMessage.SetEventTxt("月影低语：你的力量还不够...");
                return;
            }

            // 记录消耗前的状态
            int previousTears = MoonTears;
            bool wasFull = (previousTears == MAX_MOON_TEARS);

            // 消耗泪滴
            MoonTears -= cost;
            GD.Print($"消耗了 {cost} 泪滴，从 {previousTears} 到 {MoonTears}");

            // 如果从满状态开始消耗，重置恢复时间
            if (wasFull)
            {
                _lastRecoveryTime = DateTime.Now; // 使用本地时间
                GD.Print("泪滴从满状态被消耗，重置恢复时间为当前本地时间");
                SaveRecoveryTime();
            }

            // 计算结局索引
            int outcomeIndex = (selectionIndex - 1) * 3 + (interventionIndex - 1);

            if (outcomeIndex < 0 || outcomeIndex >= currentEvent.outcomes.Count())
            {
                GD.PrintErr($"无效的结局索引: {outcomeIndex}");
                return;
            }

            string outcome = currentEvent.outcomes[outcomeIndex];
            eventMessage.SetEventTxt(outcome);
            uiManager.FinishSelect();

            // 结算奖励
            rewardSystem.CalculateRewards(outcome, currentEvent.isCruelEvent);

            // 保存游戏状态
            SaveGame();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"处理干预时发生错误: {ex.Message}");
        }
    }

    // 保存恢复时间的辅助方法
    private void SaveRecoveryTime()
    {
        try
        {
            // 使用ISO格式保存时间
            playerData.lastRecoveryTime = _lastRecoveryTime.ToString("o");
            SaveSystem.Save<GameData>(playerData, StringResource.playerData);
            GD.Print($"保存恢复时间: {_lastRecoveryTime:yyyy-MM-dd HH:mm:ss} (ISO格式)");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"保存恢复时间失败: {ex.Message}");
        }
    }

    public void SaveGame()
    {
        try
        {
            // 确保保存前更新恢复时间
            playerData.lastRecoveryTime = _lastRecoveryTime.ToString("o");
            SaveSystem.Save(playerData, StringResource.playerData);
            GD.Print($"保存游戏: 泪滴={MoonTears}, 上次恢复时间={_lastRecoveryTime:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"保存游戏失败: {ex.Message}");
        }
    }

    public void SetEvent(string id)
    {
        currentEvent = GetMicroEvent(id);
        eventMessage.SetEventTxt(currentEvent.dilemma);
        uiManager.ShowDilemma(currentEvent);
    }

    public MicroEvent GetMicroEvent(string id) => allEvents.Find(i => i.eventId == id);
    void InitializeGame()
    {
        LoadPlayerData(); // 内部已调用恢复
        LoadEventData();
        StartNewSession(); // 移除此处的恢复调用

        // 保持以下不变
        eventMessage.SetMoonTxt(MoonTears);
        UpdateUI();
    }

    void LoadEventData()
    {
        try
        {
            EventContainer container = SaveSystem.Load<EventContainer>(StringResource.EventDataPath);
            allEvents = container?.events ?? new List<MicroEvent>();
            allEvents.RemoveAll(e => !string.IsNullOrEmpty(e.unlockCondition) && !playerData.unlockedPowers.Contains(e.unlockCondition));
            GD.Print($"加载了 {allEvents.Count} 个事件");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"加载事件数据失败: {ex.Message}");
            allEvents = new List<MicroEvent>();
        }
    }

    public void StartNewSession()
    {
        // 检查泪滴恢复
        uiManager.ResetSessionUI();
    }

    public MicroEvent GetRandomEvent()
    {
        if (allEvents.Count == 0)
        {
            GD.PrintErr("事件列表为空!");
            return null;
        }

        int randomIndex = (int)(GD.Randi() % allEvents.Count);
        return allEvents[randomIndex];
    }

    public void SetSelectiont(int selectionIndex)
    {
        this.selectionIndex = selectionIndex;
        if (UiManager.Instance != null)
        {
            UiManager.Instance.ShowIntervention(currentEvent);
        }
    }

    // 立即恢复所有泪滴（用于测试）
    public void RecoverAllTears()
    {
        MoonTears = MAX_MOON_TEARS;
        _lastRecoveryTime = DateTime.Now;
        GD.Print("所有泪滴已恢复!");
        SaveSystem.Save<GameData>(playerData, StringResource.playerData);
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 移除内部的 RecoverMoonTears() 调用
        if (uiManager != null && IsInstanceValid(uiManager))
        {
            TimeSpan timeToNextRecovery = TimeSpan.Zero;
            if (MoonTears < MAX_MOON_TEARS)
            {
                TimeSpan timePassed = DateTime.Now - _lastRecoveryTime;
                timeToNextRecovery = TimeSpan.FromHours(1) - timePassed;

                // 仅显示时间，不触发恢复
                if (timeToNextRecovery.TotalSeconds < 0)
                    timeToNextRecovery = TimeSpan.Zero;
            }
            uiManager.UpdateTearRecoveryDisplay(MoonTears, MAX_MOON_TEARS, timeToNextRecovery);
        }
    }

    // 在_Process中定期更新
    public override void _Process(double delta)
    {
        UpdateUI();
    }

    // 添加诊断方法：显示当前时间信息
    public void DisplayTimeInfo()
    {
        GD.Print("===== 时间诊断信息 =====");
        GD.Print($"当前本地时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        GD.Print($"上次恢复时间: {_lastRecoveryTime:yyyy-MM-dd HH:mm:ss}");
        GD.Print($"存档中的恢复时间: {playerData.lastRecoveryTime}");
        GD.Print($"时间差: {(DateTime.Now - _lastRecoveryTime).TotalHours:F2} 小时");
        GD.Print($"泪滴数量: {MoonTears}/{MAX_MOON_TEARS}");
        GD.Print("========================");
    }
}
