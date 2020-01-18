using System;
using Godot;

public abstract class Interactable : Node
{

    public abstract string GetInteractionText();

    public abstract void Interact(Node user);
}
