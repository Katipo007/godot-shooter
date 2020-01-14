using System;
using Godot;

public class InteractRayCast : RayCast {
    private Godot.Object _currentCollider;

    private Label _interactionLabel;

    public override void _Ready() {
        _interactionLabel = GetNode<Label>("/root/World/HUD/InteractionLabel");
        SetInteractionText("");
    }

    public override void _Process(float delta) {
        var collider = GetCollider();

        // check for interactable instances
        if (IsColliding() && (collider is Interactable)) {
            if (_currentCollider != collider) {
                _currentCollider = collider;
                SetInteractionText((collider as Interactable).GetInteractionText());
            }

            if (Input.IsActionJustPressed("interact")) {
                (collider as Interactable).Interact();
                // text could have changed
                SetInteractionText((collider as Interactable).GetInteractionText());
            }
        } else if (_currentCollider != null) {
            _currentCollider = null;
            SetInteractionText("");
        }
    }

    /// <summary>
    /// Update the HUD's interaction text
    /// </summary>
    /// <param name="text"></param>
    public void SetInteractionText(string text) {

        if (text.Empty()) {
            _interactionLabel.Text = "";
            _interactionLabel.Hide();
        } else {
            var interact_key = OS.GetScancodeString((InputMap.GetActionList("interact") [0] as InputEventKey).Scancode);

            _interactionLabel.Text = $"Press {interact_key} to {text}";
            _interactionLabel.Show();
        }
    }
}