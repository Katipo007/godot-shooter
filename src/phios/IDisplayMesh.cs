using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace Phios
{

    public interface IDisplayMesh
    {
        Material MaterialOverride { get; set; }

        Display Display { get; }

        Vector3[] MeshVertices { get; }

        Vector2[] MeshUVs { get; }

        Color[] MeshColors { get; }

        void Initialize(int width, int height, float quadWidth, float quadHeight, float z);

        void UpdateMesh();

    } // end class
} // end namespace
