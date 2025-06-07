using Godot;
using System;

public partial class EventPoint : TextureButton
{
    private string eventId;
    [Export] private Label label; 
    public string EventId
    {
        get
        {
            if (eventId == null || eventId.Equals(string.Empty))
                return "Ev001";
            return eventId;
        }
        set
        {
            eventId = value;
        }
    }
    public override void _Ready()
    {
        base._Ready();
        Pressed += OnThisPress;
    }
    public void Init(string eventId)
    {
        this.EventId = eventId;
        label.Text = GameManager.Instance.GetMicroEvent(EventId).title;
    }
    private void OnThisPress()
    {
        GameManager.Instance.SetEvent(EventId);
    }

}
