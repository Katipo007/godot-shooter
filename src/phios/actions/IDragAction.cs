using System;
using Godot;

namespace Phios
{
    public interface IDragAction
    {
        void OnDragStart();
        void OnDragEnd();
        void OnDragDelta(Vector2 delta);
    }
}
