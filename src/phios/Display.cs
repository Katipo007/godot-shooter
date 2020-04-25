using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace Phios
{
    [Tool]
    public class Display : Spatial
    {
        [Export(PropertyHint.ResourceType, "BitmapFont")]
        public BitmapFont Font { get; private set; }

        public Camera MainCamera { get; private set; }

        [Export]
        public Material BackgroundMaterial { get; private set; }

        [Export(PropertyHint.Range, "4,1000,1,allow_greater")]
        public int DisplayWidth { get; private set; } = 80;
        [Export(PropertyHint.Range, "4,1000,1,allow_greater")]
        public int DisplayHeight { get; private set; } = 40;
        [Export]
        public readonly bool AutoSize = false;

        [Export]
        public Curve ColorLerpCurve { get; set; }

        [Export]
        public Color ClearColor { get; private set; } = Color.Color8(0, 0, 0, 255);

        //

        [Export]
        private float _quadScale = 0.5f;
        private float _quadWidth;
        private float _quadHeight;

        /// <summary>
        /// Set if this if you expect the display to change frequently.
        /// </summary>
        [Export]
        private readonly bool _dynamic = false;

        private IDisplayMesh _backgroundMesh = null;
        private IDisplayMesh _foregroundMesh = null;

        [Export(PropertyHint.Range, "0,10,1,allow_greater,allow_lesser")]
        private uint _nReservedLayers = 1;

        [Export(PropertyHint.Range, "0,10,1,allow_greater,allow_lesser")]
        private int _nInitialLayers = 3;
        private int _nLayers = 0;

        private SC.Dictionary<int, Cell[, ]> _cells = new SC.Dictionary<int, Cell[, ]>();
        private SC.LinkedList<Cell> _cellList = new SC.LinkedList<Cell>();
        private SC.LinkedList<int>[, ] _topLayers;

        /// <summary>
        /// Stores cells which have had their states updated and need re-rendering
        /// </summary>
        /// <typeparam name="Cell"></typeparam>
        /// <returns></returns>
        private SC.Queue<Cell> _updatedCells = new SC.Queue<Cell>();
        private Vector3 zero3 = Vector3.Zero;
        private Vector2 zero2 = Vector2.Zero;

        public Display()
        {

        }

        public override string _GetConfigurationWarning()
        {
            if (Font == null || !(Font is BitmapFont))
                return "No font is set!";

            if (!HasNode("Camera") || !(GetNode("Camera") is Camera))
                return "Missing a 'Camera' with name 'Camera'";

            if (!HasNode("StaticBody") || !(GetNode("StaticBody") is StaticBody))
                return "Missing 'StaticBody' with name 'StaticBody'";
            else
            {
                var staticBody = GetNode<StaticBody>("StaticBody");

                var collisionShape = staticBody.GetNode("CollisionShape") as CollisionShape;
                if (collisionShape == null)
                    return "No collision shape";
                if (collisionShape.Shape == null || !(collisionShape.Shape is BoxShape))
                {
                    return "Invalid collision shape, requires a box shape.";
                }
            }

            return "";
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (_dynamic)
            {
                _backgroundMesh = new DisplayMeshDynamic();
                _foregroundMesh = new DisplayMeshDynamic();
            }
            else
            {
                _backgroundMesh = new DisplayMesh();
                _foregroundMesh = new DisplayMesh();
            }
            ((Node) _foregroundMesh).Name = "ForegroundMesh";
            ((Node) _backgroundMesh).Name = "BackgroundMesh";
            AddChild((Node) _foregroundMesh);
            AddChild((Node) _backgroundMesh);

            if (!Engine.EditorHint)
            {
                Font.Init();

                if (!Font.Loaded)
                {
                    GD.PushError("Font is not loaded yet!");
                    return;
                }
            }

            MainCamera = GetNode<Camera>("Camera");

            // calculate quad size
            _quadWidth = _quadScale;
            _quadHeight = ((float) Font.Get("GlyphHeight") / (float) Font.Get("GlyphWidth")) * ((float) Font.Get("QuadHeightScale")) * _quadScale;

            // derive display height from width
            if (AutoSize)
            {
                var screen = GetViewport();
                if (screen != null)
                {
                    int maxDisplayHeight = Mathf.RoundToInt((screen.Size.y / screen.Size.x) * DisplayWidth / _quadHeight);
                    DisplayHeight = maxDisplayHeight;
                }
            }

            // initialize display meshes
            _backgroundMesh.Initialize(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, -0.001f);
            _foregroundMesh.Initialize(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, 0f);
            _backgroundMesh.MaterialOverride = BackgroundMaterial;
            _foregroundMesh.MaterialOverride = (Material) Font.Get("BitmapFontMaterial");

            // initialize collision shape
            CollisionShape collisionShape = GetNode<CollisionShape>("StaticBody/CollisionShape");
            (collisionShape.Shape as BoxShape).Extents = new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * _quadHeight * 0.5f, 0.05f);
            collisionShape.Translation = new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * -_quadHeight * 0.5f, 0);

            // update camera orthographic size
            MainCamera.SetOrthogonal(Mathf.Max(DisplayHeight * _quadHeight * 1.0f, 0), 0, 2);
            MainCamera.Translation = new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * -_quadHeight * 0.5f, 0.5f);

            GD.Print($"Phios Display size: {DisplayWidth}x{DisplayHeight}");

            // pre-populate top layers
            _topLayers = new SC.LinkedList<int>[DisplayWidth, DisplayHeight];
            for (int y = 0; y < DisplayHeight; y++)
            {
                for (int x = 0; x < DisplayWidth; x++)
                {
                    _topLayers[x, y] = new SC.LinkedList<int>();
                }
            }

            // pre-populate layers
            for (int layerIndex = -(int) _nReservedLayers; layerIndex < _nInitialLayers; layerIndex++)
            {
                Cell[, ] layer = new Cell[DisplayWidth, DisplayHeight];
                for (int y = 0; y < DisplayHeight; y++)
                {
                    for (int x = 0; x < DisplayWidth; x++)
                    {
                        layer[x, y] = CreateCell(layerIndex, x, y);
                    }
                }
                _cells.Add(layerIndex, layer);
            }
            _nLayers = _nInitialLayers;
        }

        private Cell CreateCell(int layerIndex, int x, int y)
        {
            // create a new cell at layer and position
            Cell cell = new Cell();
            cell.Layer = layerIndex;
            cell.Position = new Vector2(x, y);
            cell.Owner = this;
            _cellList.AddLast(cell);
            _updatedCells.Enqueue(cell);
            return cell;
        }

        public int GetNumLayers()
        {
            return _nLayers;
        }

        public Cell GetCell(int layer, int x, int y, bool createIfNeeded = true)
        {
            // get layer cells, add if not already exists
            Cell[, ] layerCells = null;
            if (!_cells.TryGetValue(layer, out layerCells))
            {
                if (createIfNeeded)
                {
                    layerCells = new Cell[DisplayWidth, DisplayHeight];
                    _cells.Add(layer, layerCells);
                    _nLayers = Mathf.Max(layer + 1, _nLayers);
                }
                else
                    return null;
            }

            // get cell, add if not already exists
            if (x >= 0 && y >= 0 && x < DisplayWidth && y < DisplayHeight)
            {
                Cell cell = layerCells[x, y];
                if (cell == null)
                {
                    if (createIfNeeded)
                        cell = layerCells[x, y] = CreateCell(layer, x, y);
                    else
                        return null;
                }

                return cell;
            }
            // position out of bounds
            else
            {
                return null;
            }
        }

        public void AddCellAsTopLayer(Cell c)
        {

            // get top layers for cell
            SC.LinkedList<int> topLayersForCell = _topLayers[(int) c.Position.x, (int) c.Position.y];

            // add layer to end
            if (topLayersForCell.Count == 0 ||
                (topLayersForCell.Last.Value >= 0 && topLayersForCell.Last.Value < c.Layer) ||
                (topLayersForCell.Last.Value >= 0 && c.Layer < 0) ||
                (topLayersForCell.Last.Value < 0 && c.Layer < 0 && c.Layer < topLayersForCell.Last.Value))
            {
                topLayersForCell.AddLast(c.Layer);
                return;
            }
            // add layer to beginning
            else if (
                (topLayersForCell.First.Value >= 0 && c.Layer >= 0 && topLayersForCell.First.Value > c.Layer) ||
                (topLayersForCell.First.Value < 0 && c.Layer >= 0) ||
                (topLayersForCell.First.Value < 0 && c.Layer < 0 && topLayersForCell.First.Value < c.Layer))
            {
                topLayersForCell.AddFirst(c.Layer);
                return;
            }

            // insert layer
            SC.LinkedListNode<int> current = topLayersForCell.First;
            while (current.Next != null)
            {

                // layer already exists
                if (current.Value == c.Layer || current.Next.Value == c.Layer)
                {
                    return;
                }

                // found a position to insert layer
                if ((current.Value >= 0 && c.Layer >= 0 && current.Next.Value >= 0 && current.Value < c.Layer && current.Next.Value > c.Layer) ||
                    (current.Value >= 0 && c.Layer >= 0 && current.Next.Value < 0 && current.Value < c.Layer) ||
                    (current.Value >= 0 && c.Layer < 0 && current.Next.Value < 0 && current.Next.Value < c.Layer) ||
                    (current.Value < 0 && c.Layer < 0 && current.Value > c.Layer && current.Next.Value < c.Layer))
                {
                    topLayersForCell.AddAfter(current, c.Layer);
                    return;
                }

                current = current.Next;
            }
        }

        public void RemoveCellAsTopLayer(Cell c)
        {
            // get top layers for cell
            SC.LinkedList<int> topLayersForCell = _topLayers[(int) c.Position.x, (int) c.Position.y];

            // remove layer
            if (topLayersForCell.Contains(c.Layer))
            {
                topLayersForCell.Remove(c.Layer);
            }
        }

        public void MarkCellAsUpdated(Cell c)
        {
            _updatedCells.Enqueue(c);
        }

        public SC.LinkedList<int> GetTopLayersForCell(int x, int y)
        {

            if (x >= 0 && y >= 0 && x < DisplayWidth && y < DisplayHeight)
            {
                return _topLayers[x, y];
            }
            else
            {
                return new SC.LinkedList<int>();
            }

        }

        public Color GetBackgroundColorForCell(int x, int y, params int[] excludedLayers)
        {

            SC.List<int> excludedLayersList = new SC.List<int>(excludedLayers);

            // get background color of cell previous to excluded layers
            SC.LinkedList<int> topLayer = GetTopLayersForCell(x, y);
            if (topLayer.First != null && !excludedLayersList.Contains(topLayer.First.Value))
            {
                SC.LinkedListNode<int> topLayerNode = topLayer.Last;
                while (excludedLayersList.Contains(topLayerNode.Value))
                {
                    topLayerNode = topLayerNode.Previous;
                }
                Cell cell = GetCell(topLayerNode.Value, x, y);
                return (cell != null) ? cell.BackgroundColor : ClearColor;
            }

            return ClearColor;

        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            if (Engine.EditorHint)
                return;

            // process cells
            SC.LinkedListNode<Cell> cellNode = _cellList.First;
            while (cellNode != null)
            {
                cellNode.Value.Update(delta);
                cellNode = cellNode.Next;
            }

            // update display meshes
            if (_updatedCells.Count > 0)
            {
                Cell currentCell;
                Vector2 pos;
                int x, y;
                while (_updatedCells.Count > 0)
                {
                    currentCell = _updatedCells.Dequeue();
                    // get cell position
                    pos = currentCell.Position;
                    x = (int) pos.x;
                    y = (int) pos.y;

                    // get top layer for cell
                    var topLayersForCell = _topLayers[x, y];

                    // get cell at top layer
                    Cell cell = null;
                    if (topLayersForCell.First != null)
                    {
                        cell = _cells[topLayersForCell.Last.Value][x, y];
                    }

                    // empty cell
                    if (cell == null || cell.Content == "")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int vert = (y * DisplayWidth + x) * 4 + i;
                            // update display mesh vertices, uvs and colours
                            _foregroundMesh.MeshVertices[vert].x = 0;
                            _foregroundMesh.MeshVertices[vert].y = 0;
                            _foregroundMesh.MeshUVs[vert] = zero2;
                            _foregroundMesh.MeshColors[vert] = ClearColor;
                            _backgroundMesh.MeshColors[vert] = cell != null ? cell.BackgroundColor : ClearColor;
                        }
                    }
                    // filled cell
                    else
                    {
                        var glyph = Font.GetGlyph(cell.Content);
                        for (int i = 0; i < 4; i++)
                        {
                            int vert = (y * DisplayWidth + x) * 4 + i;

                            // update display mesh vertices, uvs and colours
                            _foregroundMesh.MeshVertices[vert].x = x * _quadWidth + glyph.Vertices[i].x * _quadWidth;
                            _foregroundMesh.MeshVertices[vert].y = -y * _quadHeight + glyph.Vertices[i].y * _quadHeight - _quadHeight;
                            _foregroundMesh.MeshUVs[vert] = glyph.UVs[i];
                            _foregroundMesh.MeshColors[vert] = cell.ForegroundColor;
                            _backgroundMesh.MeshColors[vert] = cell.BackgroundColor;
                        }
                    }
                }

                // apply display mesh updates
                _backgroundMesh.UpdateMesh();
                _foregroundMesh.UpdateMesh();
            }
        }
    } // end class
} // end namespace
