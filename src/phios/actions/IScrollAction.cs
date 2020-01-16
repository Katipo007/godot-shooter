using System;
using Godot;

namespace phios {
    public interface IScrollAction {
        void OnScrollDelta(int delta);
    }
}