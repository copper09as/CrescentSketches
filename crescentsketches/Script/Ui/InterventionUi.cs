using Godot;
using System;

public partial class InterventionUi : Control
{
    [Export] private Button option1;
    [Export] private Button option2;
    [Export] private Button option3;
    public override void _Ready()
    {
        base._Ready();
        option1.Pressed += () => HandleOptionSelection(1); // 使用Lambda传递参数
        option2.Pressed += () => HandleOptionSelection(2);
        option3.Pressed += () => HandleOptionSelection(3);
    }
    private void HandleOptionSelection(int intervention)
    {
        GameManager.Instance.ProcessIntervention(intervention);
    }
    public void DisplayEvent(MicroEvent evt)
    {
        option1.Text = "消耗泪滴" + evt.interventions[0].cost.ToString() + "/" + evt.interventions[0].desc;
        option2.Text = "消耗泪滴" + evt.interventions[1].cost.ToString() + "/" + evt.interventions[1].desc;
        option3.Text = "消耗泪滴" + evt.interventions[2].cost.ToString() + "/" + evt.interventions[2].desc;
    }
}