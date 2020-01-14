using System;
using Godot;

public class InteractableLightswitch : Interactable {
    [Export]
    private bool _onByDefault = true;
    [Export]
    private float _energyWhenOn = 1;
    [Export]
    private float _energyWhenOff = 0;

    private Light _light;
    private bool _on;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _light = GetNode<Light>("Light");
        _on = _onByDefault;
        SetLightEnergy();
    }

    public override void Interact() {
        _on = !_on;
        SetLightEnergy();
    }

    public override string GetInteractionText() {
        var turn_text = _on ? "Off" : "On";
        return $"Turn light {turn_text}";
    }

    private void SetLightEnergy() {
        _light.SetParam(Light.Param.Energy, (_on ? _energyWhenOn : _energyWhenOff));
    }
}