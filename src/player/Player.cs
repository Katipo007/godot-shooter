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

    private Vector3 _velocity;

    private Spatial _head;
    private Camera _camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _head = GetNode<Spatial>("Head");
        _camera = GetNode<Camera>("Head/Camera");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {

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

        _velocity = MoveAndSlide(_velocity);
    }
}