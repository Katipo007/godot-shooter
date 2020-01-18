using System;
using Godot;

public class Shotgun : Weapon
{

    [Export]
    protected float _fireRange = 10.0f;

    protected float _fireRate = 1.0f;
    protected uint _clipSize = 2;

    public override void _Ready()
    {
        base._Ready();

        _raycast.CastTo = new Vector3(0, 0, -_fireRange);
    }
}
