using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace Phios
{

    [Tool]
    public class DisplayMeshDynamic : ImmediateGeometry, IDisplayMesh
    {
        public Display Display { get; private set; }

        public Vector3[] MeshVertices { get; private set; }

        public Vector2[] MeshUVs { get; private set; }

        public Color[] MeshColors { get; private set; }

        private int _len;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (Engine.EditorHint)
                return;

            Display = GetParent<Display>();
            this.CastShadow = ShadowCastingSetting.Off;
        }

        public void Initialize(int width, int height, float quadWidth, float quadHeight, float z)
        {
            if (Engine.EditorHint)
                return;

            _len = width * height * 4;

            // setup data arrays
            MeshVertices = new Vector3[_len];
            MeshUVs = new Vector2[_len];
            MeshColors = new Color[_len];

            Color defaultColor = Colors.Magenta;

            // setup each quad
            var len = width * height;
            for (int quad = 0; quad < len; quad++)
            {
                float x1 = (quad % width) * quadWidth;
                float y1 = (quad / width) * -quadHeight;
                float x2 = x1 + quadWidth;
                float y2 = y1 - quadHeight;

                int p = quad * 4;
                MeshVertices[p] = new Vector3(x1, y1, z);
                MeshUVs[p] = new Vector2(0, 0);
                MeshColors[p] = defaultColor;
                ++p;

                MeshVertices[p] = new Vector3(x2, y2, z);
                MeshUVs[p] = new Vector2(1, 1);
                MeshColors[p] = defaultColor;
                ++p;

                MeshVertices[p] = new Vector3(x2, y1, z);
                MeshUVs[p] = new Vector2(1, 0);
                MeshColors[p] = defaultColor;
                ++p;

                MeshVertices[p] = new Vector3(x1, y2, z);
                MeshUVs[p] = new Vector2(0, 1);
                MeshColors[p] = defaultColor;
                ++p;
            }
        }

        public void UpdateMesh()
        {
            if (Engine.EditorHint)
                return;
            Clear();
            Begin(Mesh.PrimitiveType.Triangles);

            for (int i = 0; i < _len; i += 4)
            {
                SetColor(MeshColors[i]);
                SetUv(MeshUVs[i]);
                AddVertex(MeshVertices[i]);

                SetColor(MeshColors[i + 1]);
                SetUv(MeshUVs[i + 1]);
                AddVertex(MeshVertices[i + 1]);

                SetColor(MeshColors[i + 3]);
                SetUv(MeshUVs[i + 3]);
                AddVertex(MeshVertices[i + 3]);

                SetColor(MeshColors[i + 1]);
                SetUv(MeshUVs[i + 1]);
                AddVertex(MeshVertices[i + 1]);

                SetColor(MeshColors[i]);
                SetUv(MeshUVs[i]);
                AddVertex(MeshVertices[i]);

                SetColor(MeshColors[i + 2]);
                SetUv(MeshUVs[i + 2]);
                AddVertex(MeshVertices[i + 2]);
            }

            End();
        }

    } // end class
} // end namespace
