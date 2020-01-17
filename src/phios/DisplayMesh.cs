using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace phios {

    [Tool]
    public class DisplayMesh : MeshInstance {
        public Display Display { get; private set; }

        public Vector3[] MeshVertices { get; private set; }

        public Vector2[] MeshUVs { get; private set; }

        public Color[] MeshColors { get; private set; }

        private ArrayMesh _mesh;
        private MeshDataTool _mdt = new MeshDataTool();
        private int[] _indexes;
        private GC.Array _arrays;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            if (Engine.EditorHint)
                return;

            Display = GetParent<Display>();

            this._mesh = this.Mesh as ArrayMesh;
            if (this._mesh == null) {
                throw new Exception("Display mesh must be of type ArrayMesh");
            }
        }

        public void Initialize(int width, int height, float quadWidth, float quadHeight, float z) {
            if (Engine.EditorHint)
                return;

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

                _indexes[quad * 6 + 0] = quad * 4 + 0;
                _indexes[quad * 6 + 1] = quad * 4 + 1;
                _indexes[quad * 6 + 2] = quad * 4 + 3;
                _indexes[quad * 6 + 3] = quad * 4 + 1;
                _indexes[quad * 6 + 4] = quad * 4 + 0;
                _indexes[quad * 6 + 5] = quad * 4 + 2;
            }

            _arrays = new GC.Array();
            _arrays.Resize((int) Mesh.ArrayType.Max);
        }

        public void UpdateMesh() {
            if (Engine.EditorHint)
                return;

            // remove old surface
            if (_mesh.GetSurfaceCount() > 0)
                _mesh.SurfaceRemove(0);

            // create a new surface from the arrays
            _arrays[(int) Mesh.ArrayType.Vertex] = MeshVertices;
            _arrays[(int) Mesh.ArrayType.TexUv] = MeshUVs;
            _arrays[(int) Mesh.ArrayType.Color] = MeshColors;
            _arrays[(int) Mesh.ArrayType.Index] = _indexes;
            _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _arrays);
        }

        public override string _GetConfigurationWarning() {
            if (Mesh == null || !(Mesh is ArrayMesh)) {
                return "DisplayMesh.Mesh is not an array mesh!";
            }

            return "";
        }

        public void UpdateEditor(int width, int height, float quadWidth, float quadHeight, float z) {
            if (!Engine.EditorHint)
                return;

            if (!(Mesh is ArrayMesh)) {
                GD.PrintErr("DisplayMesh.Mesh is not an array mesh!");
                return;
            }

            var m = Mesh as ArrayMesh;
            // remove old surface
            if (m.GetSurfaceCount() > 0)
                m.SurfaceRemove(0);

            var a = new GC.Array();
            a.Resize((int) Mesh.ArrayType.Max);
            a[(int) Mesh.ArrayType.Vertex] = new Vector3[] {
                new Vector3(0, 0, z),
                new Vector3(width * quadWidth, 0, z),
                new Vector3(width * quadWidth, height * -quadHeight, z),
                new Vector3(0, height * -quadHeight, z)
            };
            a[(int) Mesh.ArrayType.Index] = new int[] {
                0,
                1,
                2,
                2,
                3,
                0
            };
            m.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, a);
            m.SurfaceSetName(0, "Editor Preview");

            GD.Print($"{Name} DisplayMesh updated");
        }

    } // end class
} // end namespace