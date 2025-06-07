using Godot;
using System;

public partial class StartTscn : Control
{
    [Export] private Button StartGameBtn;
    [Export] private Button QuitBtn;
    public override void _Ready()
    {
        base._Ready();
        StartGameBtn.Pressed += OnStartBtnPress;
        QuitBtn.Pressed += OnExit;
    }
    private void OnStartBtnPress()
    {
        SceneChangeManager.Instance.ChangeScene(StringResource.MainGamePath);
    }
    private void OnExit()
    {
        GetTree().Quit();
    }

}
