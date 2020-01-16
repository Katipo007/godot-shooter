using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace phios {
    public class Display : Spatial {

        [Export]
        public BitmapFont Font { get; private set; }

        public Camera MainCamera { get; private set; }

        [Export]
        public PackedScene DisplayQuadMesh { get; private set; }

        [Export]
        public Material ForegroundMaterial { get; private set; }

        [Export]
        public Material BackgroundMaterial { get; private set; }

        public DisplayMesh Background { get; private set; }
        public DisplayMesh Foreground { get; private set; }

        [Export]
        public int DisplayWidth { get; private set; } = 80;
        [Export]
        public int DisplayHeight { get; private set; } = 40;

        [Export]
        public Curve ColorLerpCurve { get; set; }

        public bool Initialized { get; private set; } = false;

        public Color ClearColor { get; private set; }

        private float _quadWidth;
        private float _quadHeight;

        [Export(PropertyHint.Range, "0;10")]
        private int _nReservedLayers = 1;

        [Export(PropertyHint.Range, "0;10")]
        private int _nInitialLayers = 3;
        private int _nLayers = 0;

        private SC.Dictionary<int, Cell[, ]> _cells = new SC.Dictionary<int, Cell[, ]>();
        private SC.LinkedList<Cell> _cellList = new SC.LinkedList<Cell>();
        private SC.LinkedList<int>[, ] _topLayers;
        private Vector3 zero3 = Vector3.Zero;
        private Vector2 zero2 = Vector2.Zero;

        public Display() {
            ClearColor = Color.Color8(0, 0, 0, 0);
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            MainCamera = GetNode<Camera>("Camera");

            // calculate quad size
            _quadWidth = 1f;
            _quadHeight = Font.Height; // (font.GetGlyphHeight() / font.GetGlyphWidth()) * (font.useRexPaintFont ? font.rexPaintQuadHeightScale : font.quadHeightScale);

            // instantiate quads
            // TODO

            // add quads to combine instances
            // TODO

            // combine quads to foreground and background
            // TODO

            // update camera orthographic size
            MainCamera.SetOrthogonal(Mathf.Max(DisplayHeight * _quadHeight * 0.5f, Background.Translation.y), 0, 2);

            // destroy original quads
            // TODO

            GD.Print($"Phios Display size: {DisplayWidth}x{DisplayHeight}");

            // pre-populate top layers
            _topLayers = new SC.LinkedList<int>[DisplayWidth, DisplayHeight];
            for (int y = 0; y < DisplayHeight; y++) {
                for (int x = 0; x < DisplayWidth; x++) {
                    _topLayers[x, y] = new SC.LinkedList<int>();
                }
            }

            // pre-populate layers
            for (int layerIndex = -_nReservedLayers; layerIndex < _nInitialLayers; layerIndex++) {
                Cell[, ] layer = new Cell[DisplayWidth, DisplayHeight];
                for (int y = 0; y < DisplayHeight; y++) {
                    for (int x = 0; x < DisplayWidth; x++) {
                        layer[x, y] = CreateCell(layerIndex, x, y);
                    }
                }
                _cells.Add(layerIndex, layer);
            }
            _nLayers = _nInitialLayers;

            // we are now initialized
            Initialized = true;
        }

        private Cell CreateCell(int layerIndex, int x, int y) {
            // create a new cell at layer and position
            Cell cell = new Cell();
            cell.Layer = layerIndex;
            cell.Position = new Vector2(x, y);
            _cellList.AddLast(cell);
            return cell;
        }

        public int GetNumLayers() {
            if (Initialized)
                return _nLayers;
            else
                throw new Exception("Display is not yet initialized!");
        }

        public Cell GetCell(int layer, int x, int y) {
            if (Initialized) {
                // get layer cells, add if not already exists
                Cell[, ] layerCells = null;
                if (!_cells.TryGetValue(layer, out layerCells)) {
                    layerCells = new Cell[DisplayWidth, DisplayHeight];
                    _cells.Add(layer, layerCells);
                    _nLayers = Mathf.Max(layer + 1, _nLayers);
                }

                // get cell, add if not already exists
                if (x >= 0 && y >= 0 && x < DisplayWidth && y < DisplayHeight) {
                    Cell cell = layerCells[x, y];
                    if (cell == null) {
                        cell = layerCells[x, y] = CreateCell(layer, x, y);
                    }

                    return cell;
                }
                // position out of bounds
                else {
                    return null;
                }
            } else {
                throw new Exception("Display is not yet initialized!");
            }
        }

        public void AddCellAsTopLayer(Cell c) {
            if (Initialized) {
                // get top layers for cell
                SC.LinkedList<int> topLayersForCell = _topLayers[(int) c.Position.x, (int) c.Position.y];

                // add layer to end
                if (topLayersForCell.Count == 0 ||
                    (topLayersForCell.Last.Value >= 0 && topLayersForCell.Last.Value < c.Layer) ||
                    (topLayersForCell.Last.Value >= 0 && c.Layer < 0) ||
                    (topLayersForCell.Last.Value < 0 && c.Layer < 0 && c.Layer < topLayersForCell.Last.Value)) {
                    topLayersForCell.AddLast(c.Layer);
                    return;
                }
                // add layer to beginning
                else if (
                    (topLayersForCell.First.Value >= 0 && c.Layer >= 0 && topLayersForCell.First.Value > c.Layer) ||
                    (topLayersForCell.First.Value < 0 && c.Layer >= 0) ||
                    (topLayersForCell.First.Value < 0 && c.Layer < 0 && topLayersForCell.First.Value < c.Layer)) {
                    topLayersForCell.AddFirst(c.Layer);
                    return;
                }

                // insert layer
                SC.LinkedListNode<int> current = topLayersForCell.First;
                while (current.Next != null) {

                    // layer already exists
                    if (current.Value == c.Layer || current.Next.Value == c.Layer) {
                        return;
                    }

                    // found a position to insert layer
                    if ((current.Value >= 0 && c.Layer >= 0 && current.Next.Value >= 0 && current.Value < c.Layer && current.Next.Value > c.Layer) ||
                        (current.Value >= 0 && c.Layer >= 0 && current.Next.Value < 0 && current.Value < c.Layer) ||
                        (current.Value >= 0 && c.Layer < 0 && current.Next.Value < 0 && current.Next.Value < c.Layer) ||
                        (current.Value < 0 && c.Layer < 0 && current.Value > c.Layer && current.Next.Value < c.Layer)) {
                        topLayersForCell.AddAfter(current, c.Layer);
                        return;
                    }

                    current = current.Next;
                }

            } else {
                throw new Exception("Display is not yet initialized!");
            }
        }

        public void RemoveCellAsTopLayer(Cell c) {
            if (Initialized) {
                // get top layers for cell
                SC.LinkedList<int> topLayersForCell = _topLayers[(int) c.Position.x, (int) c.Position.y];

                // remove layer
                if (topLayersForCell.Contains(c.Layer)) {
                    topLayersForCell.Remove(c.Layer);
                }
            } else {
                throw new Exception("Display is not yet initialized!");
            }
        }

        public SC.LinkedList<int> GetTopLayersForCell(int x, int y) {
            if (Initialized) {
                if (x >= 0 && y >= 0 && x < DisplayWidth && y < DisplayHeight) {
                    return _topLayers[x, y];
                } else {
                    return new SC.LinkedList<int>();
                }
            } else {
                throw new Exception("Display is not yet initialized!");
            }
        }

        public Color GetBackgroundColorForCell(int x, int y, params int[] excludedLayers) {
            if (Initialized) {
                SC.List<int> excludedLayersList = new SC.List<int>(excludedLayers);

                // get background color of cell previous to excluded layers
                SC.LinkedList<int> topLayer = GetTopLayersForCell(x, y);
                if (topLayer.First != null && !excludedLayersList.Contains(topLayer.First.Value)) {
                    SC.LinkedListNode<int> topLayerNode = topLayer.Last;
                    while (excludedLayersList.Contains(topLayerNode.Value)) {
                        topLayerNode = topLayerNode.Previous;
                    }
                    Cell cell = GetCell(topLayerNode.Value, x, y);
                    return (cell != null) ? cell.BackgroundColor : ClearColor;
                }

                return ClearColor;
            } else {
                throw new Exception("Display is not yet initialized!");
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta) {
            if (Initialized) {
                // update cells
                SC.LinkedListNode<Cell> cellNode = _cellList.First;
                while (cellNode != null) {
                    cellNode.Value.Update(delta);
                    cellNode = cellNode.Next;
                }

                // update display meshes
                // TODO

                // apply display mesh updates
                Background.UpdateMesh();
                Foreground.UpdateMesh();

                // take screenshots
                if (Input.IsActionJustPressed("screenshot")) {
                    var image = GetViewport().GetTexture().GetData();
                    image.FlipY();
                    image.SavePng("user://screenshots/phiosScreenshot.png");
                }
            }
        }

        public bool IsInitialized() {
            return Initialized;
        }
    } // end class
} // end namespace