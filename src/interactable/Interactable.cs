using System;
using Godot;

public class Interactable : Node {

    public virtual string GetInteractionText() {
        return "interact";
    }

    public virtual void Interact() {
        GD.Print($"Interacted with {Name}");
    }
}