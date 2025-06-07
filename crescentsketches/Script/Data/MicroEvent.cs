using Godot;
using System;
using System.Collections.Generic;

[System.Serializable]
public class MicroEvent
{
    public string eventId;
    public string title;
    public string dilemma;
    public string[] choices = new string[2]; // 两个选项
    public InterventionOption[] interventions = new InterventionOption[3]; // 三种干预
    public string[] outcomes = new string[6]; // 6种结局组合
    public bool isCruelEvent;
    public string unlockCondition;
}
[System.Serializable]
public class EventContainer {
    public List<MicroEvent> events;
}