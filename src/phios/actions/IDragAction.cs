using System;
using Godot;

namespace phios {
    public interface IDragAction {
        void OnDragStart();
        void OnDragEnd();
        void OnDragDelta(Vector2 delta);
    }
}