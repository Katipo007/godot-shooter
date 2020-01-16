using System;
using Godot;

namespace phios {
    public interface IDragAction {
        void OnDragDown();
        void OnDragDelta(Vector2 delta);
    }
}