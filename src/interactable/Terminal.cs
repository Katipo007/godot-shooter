using System;
using Godot;

[Tool]
public class Terminal : Interactable
{
    public Phios.Display Display { get; private set; }

    public Phios.Mouse Mouse { get; private set; }

    private CollisionShape _collisionShape;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        // get references
        Display = GetNode<Phios.Display>("Display");
        Mouse = GetNode<Phios.Mouse>("Display/Mouse");
        _collisionShape = GetNode<CollisionShape>("CollisionShape");

        if (Display == null)
            throw new Exception("No Display is attached!");

        if (Mouse == null)
            throw new Exception("No Mouse is attached!");

        if (_collisionShape == null)
            throw new Exception("No CollisionShape is attached!");

        // set our shape to match the display
        _collisionShape.Shape = (Display.GetNode("StaticBody/CollisionShape") as CollisionShape).Shape;
        _collisionShape.Translation = (Display.GetNode("StaticBody/CollisionShape") as CollisionShape).Translation;
    }

    public override string _GetConfigurationWarning()
    {
        if (GetNodeOrNull<Phios.Display>("Display") == null)
            return "Missing Display";

        if (GetNodeOrNull<CollisionShape>("CollisionShape") == null)
            return "Missing CollisionShape";

        if (GetNodeOrNull<Phios.Mouse>("Display/Mouse") == null)
            return "Display is missing Mouse";

        return "";
    }

    public override string GetInteractionText()
    {
        return "Use Terminal";
    }

    public override void Interact(Node user)
    {
        if (user != null)
        {
            GD.Print("User is not null");
            var player = user as Player;
            if (player != null)
            {
                player.SetActiveMouse(this.Mouse);
            }
        }
    }

    public override void _Process(float delta)
    {
        if (Engine.EditorHint)
            return;

        string options = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var cell = Display.GetCell(0, (int) GD.Randi() % Display.DisplayWidth, (int) GD.Randi() % Display.DisplayHeight, false);

        if (cell != null)
            cell.SetContent(options.Substr((int) (GD.Randi() % options.Length).Clamp(0, options.Length - 1), 1), Colors.Black, Colors.White, 0.5f, Colors.White);
    }
}
