using System;
using Godot;

public class Interactable : Node {

    public string GetInteractionText() {
        return "interact";
    }

    public void Interact() {
        GD.Print($"Interacted with {Name}");
    }
}