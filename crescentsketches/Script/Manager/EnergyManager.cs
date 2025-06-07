using Godot;
using System;
using System.Collections.Generic;

public partial class EnergyManager : Node
{
    // 单例模式
    public static EnergyManager Instance { get; private set; }

    [Export] public int MaxEnergy = 100;
    [Export] public int DailyRecovery = 5;

    private int _currentEnergy;
    private DateTime _lastRecoveryTime;


    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("EnergyManager already exists!");
            QueueFree();
            return;
        }
        Instance = this;

        LoadData();
        UpdateEnergy();
    }

    public override void _Process(double delta)
    {
        // 可选：实时显示能量（仅演示用）
        GD.Print($"Current Energy: {_currentEnergy}/{MaxEnergy}");

    }

    private void LoadData()
    {
        var energy = SaveSystem.Load<Energy>(StringResource.energy_data);
        if (energy == null)
        {
            _currentEnergy = 0;
            _lastRecoveryTime = DateTime.UtcNow;
            SaveData();
        }
        else
        {

            _currentEnergy = energy.moonTears;
            _lastRecoveryTime = DateTime.Parse(energy.last_time);
        }
    }

    private void SaveData()
    {

        var energy = new Energy
        {
            moonTears = _currentEnergy,
            last_time = DateTime.UtcNow.ToString("o")
        };
        SaveSystem.Save<Energy>(energy, StringResource.energy_data);
    }

    private void UpdateEnergy()
    {
        TimeSpan elapsed = DateTime.UtcNow - _lastRecoveryTime;
        int hoursPassed = (int)elapsed.TotalHours;
        int energyToAdd = hoursPassed * DailyRecovery;
        _currentEnergy = Mathf.Min(_currentEnergy + energyToAdd, MaxEnergy);
        _lastRecoveryTime = DateTime.UtcNow;
        SaveData();
    }

    public bool ConsumeEnergy(int amount)
    {
        if (_currentEnergy >= amount)
        {
            _currentEnergy -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    // 外部调用方法（示例）
    public void OnGameStarted()
    {
        UpdateEnergy(); // 每次游戏启动时检查能量恢复
    }
}
[System.Serializable]
public class Energy()
{
    public int moonTears;
    public string last_time;
}