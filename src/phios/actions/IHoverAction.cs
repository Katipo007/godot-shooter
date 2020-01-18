using System;
using Godot;

namespace Phios
{
    public interface IHoverAction
    {
        void OnHoverEnter();
        void OnHoverExit();
    }
}
