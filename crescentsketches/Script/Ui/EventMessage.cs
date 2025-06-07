using Godot;
using Microsoft.VisualBasic;
using System;

public partial class EventMessage : Control
{
    [Export] public Label moonTearTxt;
    [Export] public Label EventTxt;
    public override void _Ready()
    {
        base._Ready();
    }

    public void SetMoonTxt(int number)
    {
        moonTearTxt.Text = number.ToString();
    }
    public void SetEventTxt(string msg)
    {
    if(EventTxt == null || !IsInstanceValid(EventTxt))
    {
        GD.PrintErr("警告：尝试修改已释放的Label！");
        return;
    }
    EventTxt.Text = msg;
    }
}
