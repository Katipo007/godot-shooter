using System;
using Godot;

public class Weapon : Node {
    [Export]
    private float _fireRate = 0.5f;
    [Export]
    private uint _clipSize = 5;
    [Export]
    private float _reloadRate = 1;

    private uint _currentAmmo;

    private bool _canFire = true;
    private bool _reloading = false;

    public Weapon() {
        _currentAmmo = _clipSize;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    async public override void _Process(float delta) {
        // fire weapon
        if (Input.IsActionJustPressed("primary_fire") && _canFire) {
            if (_currentAmmo > 0 && !_reloading) {
                GD.Print("Fired weapon");
                _canFire = false;
                _currentAmmo -= 1;

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