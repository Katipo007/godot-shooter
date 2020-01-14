using System;
using Godot;

public class Weapon : Node {
    [Export]
    protected float _fireRate = 0.5f;
    [Export]
    protected uint _clipSize = 5;
    [Export]
    protected float _reloadRate = 1;

    protected uint _currentAmmo;

    protected bool _canFire = true;
    protected bool _reloading = false;

    protected RayCast _raycast;

    protected Label _ammoLabel;

    public Weapon() {
        _currentAmmo = _clipSize;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _raycast = GetNode<RayCast>("../Head/Camera/WeaponRayCast");
        _ammoLabel = GetNode<Label>("/root/World/HUD/AmmoLabel");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    async public override void _Process(float delta) {

        if (_reloading)
            _ammoLabel.Text = $"Ammo:\nReloading...";
        else
            _ammoLabel.Text = $"Ammo:\n{_currentAmmo} / {_clipSize}";

        // fire weapon
        if (Input.IsActionJustPressed("primary_fire") && _canFire) {
            if (_currentAmmo > 0 && !_reloading) {
                GD.Print("Fired weapon");
                _canFire = false;
                _currentAmmo -= 1;
                CheckCollision();

                var timer = GetTree().CreateTimer(_fireRate);
                await ToSignal(timer, "timeout");

                _canFire = true;

                if (_currentAmmo == 0)
                    Reload();
            }
            // reload
            else if (!_reloading) {
                Reload();
            }
        }

        if (Input.IsActionJustPressed("reload") && !_reloading) {
            Reload();
        }
    }

    private void CheckCollision() {
        if (_raycast.IsColliding()) {
            var collider = _raycast.GetCollider();
            if ((collider as Node).IsInGroup("Enemies")) {
                (collider as Node).QueueFree();
                GD.Print("Killed " + (collider as Node).Name);
            }
        }
    }

    async private void Reload() {
        GD.Print("Reloading");
        _reloading = true;

        var timer = GetTree().CreateTimer(_fireRate);
        await ToSignal(timer, "timeout");

        _reloading = false;
        _currentAmmo = _clipSize;
        GD.Print("Reload complete");
    }
}