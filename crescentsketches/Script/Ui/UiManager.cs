using Godot;
using System;

public partial class UiManager : Node
{
    public static UiManager Instance;
    [Export] public EventSelectionUi eventSelectionPanel;
    [Export] public DilemmaUI dilemmaPanel;//两难抉择
    [Export] public InterventionUi interventionPanel;//阻碍抉择
    [Export] public Control outcomePanel;//奖励ui
    [Export] public Control rewardPanel;//不知道什么ui
    [Export] public int inex;
    [Export] Button RestartBtn;
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        RestartBtn.Pressed += ReStart;
    }
    public void ResetSessionUI()
    {

        //eventSelectionPanel.Hide();
        dilemmaPanel.Hide();
        interventionPanel.Hide();
        //outcomePanel.Hide();
        //rewardPanel.Hide();
        // 生成3个随机事件点
        eventSelectionPanel.GenerateEventPoints();
    }
    public void ShowDilemma(MicroEvent evt)
    {
        eventSelectionPanel.Hide();
        dilemmaPanel.Show();
        dilemmaPanel.DisplayEvent(evt);
    }
    public void ShowIntervention(MicroEvent evt)
    {
        dilemmaPanel.Hide();
        interventionPanel.Show();
        interventionPanel.DisplayEvent(evt);
    }
    public void FinishSelect()
    {
        interventionPanel.Hide();
        RestartBtn.Show();
    }
    private void ReStart()
    {
        GetTree().ReloadCurrentScene();
    }

    [Export] Label timeRemainingLabel;
    
public void UpdateTearRecoveryDisplay(int currentTears, int maxTears, TimeSpan timeToRecovery)
{
    // 更新泪滴数量显示
    //moonTearsLabel.Text = $"泪滴: {currentTears}/{maxTears}";
    
    // 更新恢复倒计时
    if (timeToRecovery.TotalSeconds > 0)
    {
        timeRemainingLabel.Text = $"下次恢复: {timeToRecovery:mm\\:ss}";
    }
    else
    {
        timeRemainingLabel.Text = "可恢复泪滴!";
        
        // 如果泪滴未达到最大值，强制刷新显示
        if (currentTears < maxTears)
        {
            //moonTearsLabel.Text = $"泪滴: {currentTears}/{maxTears}";
        }
    }
}
}
