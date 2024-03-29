using System;
using Godot;

public class Player : KinematicBody
{
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

    private RayCast _weapon_raycast;
    private RayCast _interaction_raycast;

    /// <summary>
    /// Active phios display cursor
    /// </summary>
    private Phios.Mouse _activeMouse = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _head = GetNode<Spatial>("Head");
        _camera = GetNode<Camera>("Head/Camera");

        _weapon_raycast = GetNode<RayCast>("Head/Camera/WeaponRayCast");
        _interaction_raycast = GetNode<RayCast>("Head/Camera/InteractRayCast");

        Utils.CaptureMouse();
        mouse_invert_x = (bool) GameState.Instance.Settings["controls"]["invertMouseX"];
        mouse_invert_y = (bool) GameState.Instance.Settings["controls"]["invertMouseY"];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            // Input.SetMouseMode(Input.MouseMode.Visible);
            if (_activeMouse != null)
            {
                _activeMouse = null;
            }
        }

        // TODO: Disengage Phios mouse if we move too far away
    }

    public override void _Input(InputEvent input)
    {
        InputEventMouseMotion motion = input as InputEventMouseMotion;

        // handle mouse look
        if (motion != null)
        {
            var mouseDeltaX = motion.Relative.x * mouse_sensitivity * (mouse_invert_x ? -1 : 1);
            var mouseDeltaY = motion.Relative.y * mouse_sensitivity * (mouse_invert_y ? -1 : 1);

            // we are using a Phios display mouse, so move it
            if (_activeMouse != null)
            {
                _activeMouse.Position += new Vector2(mouseDeltaX, mouseDeltaY);
            }

            // move the camera
            else
            {
                _head.RotateY(Mathf.Deg2Rad(-mouseDeltaX));
                if (((_camera.RotationDegrees.x - mouseDeltaY) > -90.0f) && ((_camera.RotationDegrees.x - mouseDeltaY) < 90.0f))
                {
                    _camera.RotateX(Mathf.Deg2Rad(-mouseDeltaY));
                }
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var head_basis = _head.GlobalTransform.basis;
        var direction = new Vector3();

        // if we aren't trying to use a Phios mouse allow movement
        if (_activeMouse == null)
        {
            // forward-back movement
            if (Input.IsActionPressed("move_forward"))
            {
                // -Z is forward in Godot
                direction -= head_basis.z;
            }
            else if (Input.IsActionPressed("move_backward"))
            {
                direction += head_basis.z;
            }

            // left-right movement
            if (Input.IsActionPressed("move_left"))
            {
                direction -= head_basis.x;
            }
            else if (Input.IsActionPressed("move_right"))
            {
                direction += head_basis.x;
            }
        }

        // normalize direction
        direction = direction.Normalized();

        var old_y_velocity = _velocity.y;
        _velocity = _velocity.LinearInterpolate(direction * _speed, _acceleration * delta);
        // gravity
        _velocity.y = old_y_velocity - _gravity;

        // jumping
        if ((_activeMouse == null) && Input.IsActionJustPressed("jump") && this.IsOnFloor())
        {
            _velocity.y += _jump_power;
        }

        _velocity = MoveAndSlide(_velocity, Vector3.Up);
    }

    public void SetActiveMouse(Phios.Mouse mouse)
    {
        this._activeMouse = mouse;
    }
}
