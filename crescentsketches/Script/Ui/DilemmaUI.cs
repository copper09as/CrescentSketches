using Godot;
using System;

public partial class DilemmaUI : Control
{
    [Export] private Button option1;
    [Export] private Button option2;
    public override void _Ready()
    {
        base._Ready();
        option1.Pressed += () => HandleOptionSelection(1); // 使用Lambda传递参数
        option2.Pressed += () => HandleOptionSelection(2);
        //DisplayEvent(data.events[0]);
    }
private void HandleOptionSelection(int selection)
{
    GameManager.Instance.SetSelectiont(selection);
}
    public void DisplayEvent(MicroEvent evt)
    {
        option1.Text = evt.choices[0];
        option2.Text = evt.choices[1];
    }
}
