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

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            Display = GetParent<Display>();

            this._mesh = this.Mesh as ArrayMesh;
            if (this._mesh == null) {
                throw new Exception("Display mesh must be of type ArrayMesh");
            }
        }

        public void CombineQuads() {
            throw new NotImplementedException();
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
            _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            // set the material
            _mesh.SurfaceSetMaterial(0, Display.ForegroundMaterial);
        }

    } // end class
} // end namespace