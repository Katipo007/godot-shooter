using System;
using Godot;

namespace Phios
{
    public interface IScrollAction
    {
        void OnScrollDelta(int delta);
    }
}
