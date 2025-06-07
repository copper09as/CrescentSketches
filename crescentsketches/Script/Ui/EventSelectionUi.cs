using Godot;
using System;

public partial class EventSelectionUi : Control
{
    [Export] private Godot.Collections.Array<EventPoint> points;

    public void GenerateEventPoints()
    {
        this.Show();
        foreach (var i in points)
        {
            var microEvent = GameManager.Instance.GetRandomEvent();
            i.Init(microEvent.eventId);
        }
    }

}
