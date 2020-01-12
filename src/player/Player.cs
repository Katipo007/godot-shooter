using System;
using Godot;

public class Player : KinematicBody {
    [Export]
    private float _speed = 10.0f;
    [Export]
    private float _acceleration = 5.0f;
    [Export]
    private float _gravity = 0.98f;
    [Export]
    private float _jump_power = 30.0f;

    // TODO: Move these to settings
    [Export]
    private float mouse_sensitivity = 0.3f;
    [Export]
    private bool mouse_invert_x = false;
    [Export]
    private bool mouse_invert_y = false;

    private Vector3 _velocity;

    private Spatial _head;
    private Camera _camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _head = GetNode<Spatial>("Head");
        _camera = GetNode<Camera>("Head/Camera");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("ui_cancel"))
            Input.SetMouseMode(Input.MouseMode.Visible);
    }

    public override void _Input(InputEvent input) {
        InputEventMouseMotion motion = input as InputEventMouseMotion;

        // handle mouse look
        if (motion != null) {
            _head.RotateY(Mathf.Deg2Rad(-motion.Relative.x * mouse_sensitivity * (mouse_invert_x ? -1 : 1)));

            var x_delta = -motion.Relative.y * mouse_sensitivity * (mouse_invert_y ? -1 : 1);
            if (((_camera.RotationDegrees.x + x_delta) > -90.0f) && ((_camera.RotationDegrees.x + x_delta) < 90.0f)) {
                _camera.RotateX(Mathf.Deg2Rad(x_delta));
            }
        }
    }

    public override void _PhysicsProcess(float delta) {
        var head_basis = _head.GetGlobalTransform().basis;

        var direction = new Vector3();

        // forward-back movement
        if (Input.IsActionPressed("move_forward")) {
            // -Z is forward in Godot
            direction -= head_basis.z;
        } else if (Input.IsActionPressed("move_backward")) {
            direction += head_basis.z;
        }

        // left-right movement
        if (Input.IsActionPressed("move_left")) {
            direction -= head_basis.x;
        } else if (Input.IsActionPressed("move_right")) {
            direction += head_basis.x;
        }

        // normalize direction
        direction = direction.Normalized();

        _velocity = _velocity.LinearInterpolate(direction * _speed, _acceleration * delta);
        // gravity
        _velocity.y -= _gravity;

        // jumping
        if (Input.IsActionJustPressed("jump") && this.IsOnFloor()) {
            _velocity.y += _jump_power;
        }

        _velocity = MoveAndSlide(_velocity, Vector3.Up);
    }
}