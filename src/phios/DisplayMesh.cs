using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace phios {
    public class DisplayMesh : MeshInstance {
        public Display Display { get; private set; }

        public Vector3[] MeshVertices { get; private set; }

        public Vector2[] MeshUVs { get; private set; }

        public Color[] MeshColors { get; private set; }

        private ArrayMesh _mesh;
        private MeshDataTool _mdt = new MeshDataTool();
        private int[] _indexes;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            Display = GetParent<Display>();

            this._mesh = this.Mesh as ArrayMesh;
            if (this._mesh == null) {
                throw new Exception("Display mesh must be of type ArrayMesh");
            }
        }

        public void Initialize(int width, int height, float quadWidth, float quadHeight, float z) {
            // setup data arrays
            MeshVertices = new Vector3[width * height * 4];
            MeshUVs = new Vector2[width * height * 4];
            MeshColors = new Color[width * height * 4];
            _indexes = new int[width * height * 6];

            Color defaultColor = Colors.Magenta;

            // setup each quad
            var len = width * height;
            for (int quad = 0; quad < len; quad++) {
                float x1 = (quad % width) * quadWidth;
                float y1 = (quad / width) * quadHeight;
                float x2 = x1 + quadWidth;
                float y2 = y1 + quadHeight;

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

                // _indexes[quad * 6 + 0] = quad * 4 + 0;
                // _indexes[quad * 6 + 1] = quad * 4 + 1;
                // _indexes[quad * 6 + 2] = quad * 4 + 3;
                // _indexes[quad * 6 + 3] = quad * 4 + 1;
                // _indexes[quad * 6 + 4] = quad * 4 + 0;
                // _indexes[quad * 6 + 5] = quad * 4 + 2;

                _indexes[quad * 6 + 0] = quad * 4 + 0;
                _indexes[quad * 6 + 1] = quad * 4 + 3;
                _indexes[quad * 6 + 2] = quad * 4 + 1;
                _indexes[quad * 6 + 3] = quad * 4 + 1;
                _indexes[quad * 6 + 4] = quad * 4 + 2;
                _indexes[quad * 6 + 5] = quad * 4 + 0;
            }
        }

        public void UpdateMesh() {
            // remove old surface
            if (_mesh.GetSurfaceCount() > 0)
                _mesh.SurfaceRemove(0);

            // create a new surface from the arrays
            var arrays = new GC.Array();
            arrays.Resize((int) Mesh.ArrayType.Max);
            arrays[(int) Mesh.ArrayType.Vertex] = MeshVertices;
            arrays[(int) Mesh.ArrayType.TexUv] = MeshUVs;
            arrays[(int) Mesh.ArrayType.Color] = MeshColors;
            arrays[(int) Mesh.ArrayType.Index] = _indexes;
            _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        }

    } // end class
} // end namespace