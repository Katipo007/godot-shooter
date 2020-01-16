using System;
using Godot;

namespace phios {
    public interface IHoverAction {
        void OnHoverEnter();
        void OnHoverExit();
    }
}