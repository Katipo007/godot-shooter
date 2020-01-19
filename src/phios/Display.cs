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

        public DisplayMesh Background { get; private set; }
        public DisplayMesh Foreground { get; private set; }

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

        [Export]
        private float _quadScale = 0.5f;
        private float _quadWidth;
        private float _quadHeight;

        [Export(PropertyHint.Range, "0,10,1,allow_greater,allow_lesser")]
        private uint _nReservedLayers = 1;

        [Export(PropertyHint.Range, "0,10,1,allow_greater,allow_lesser")]
        private int _nInitialLayers = 3;
        private int _nLayers = 0;

        private SC.Dictionary<int, Cell[, ]> _cells = new SC.Dictionary<int, Cell[, ]>();
        private SC.LinkedList<Cell> _cellList = new SC.LinkedList<Cell>();
        private SC.LinkedList<int>[, ] _topLayers;
        private Vector3 zero3 = Vector3.Zero;
        private Vector2 zero2 = Vector2.Zero;

        public Display()
        {

        }

        [Export]
        private bool _EditorUpdate
        {
            get { return false; }
            set
            {
                if (!Engine.EditorHint)
                    return;

                if (value != true)
                    return;

                var warning = _GetConfigurationWarning();
                if (warning != "")
                {
                    GD.PrintErr(warning);
                    return;
                }

                if (Font == null || !(Font is BitmapFont))
                    return;

                Background = GetNode("Background") as DisplayMesh;
                Foreground = GetNode("Foreground") as DisplayMesh;

                _quadWidth = _quadScale;
                _quadHeight = Mathf.Clamp(((float) Font.Get("GlyphHeight") / (float) Font.Get("GlyphWidth")) * ((float) Font.Get("QuadHeightScale")), 0f, 100000f) * _quadScale;

                if (Background != null)
                {
                    Background.UpdateEditor(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, -0.001f);
                }
                else
                    GD.PrintErr("Failed to get Background DisplayMesh");

                if (Foreground != null)
                {
                    Foreground.UpdateEditor(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, 0f);
                }
                else
                    GD.PrintErr("Failed to get Foreground DisplayMesh");

                // initialize collision shape
                CollisionShape collisionShape = GetNode<CollisionShape>("StaticBody/CollisionShape");
                (collisionShape.Shape as BoxShape).SetExtents(new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * _quadHeight * 0.5f, 0.05f));
                collisionShape.Translation = new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * -_quadHeight * 0.5f, 0);

                GD.Print($"{Name} Display updated");
            }
        }

        public override string _GetConfigurationWarning()
        {
            if (Font == null || !(Font is BitmapFont))
                return "No font is set!";

            if (!HasNode("Background") || !(GetNode("Background") is MeshInstance))
                return "Missing a 'MeshInstance' with name 'Background'";

            if (!HasNode("Foreground") || !(GetNode("Foreground") is MeshInstance))
                return "Missing a 'MeshInstance' with name 'Foreground'";

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
            if (Engine.EditorHint)
                return;

            Font.Init();

            if (!Font.Loaded)
                GD.PushError("Font is not loaded yet!");

            MainCamera = GetNode<Camera>("Camera");
            Background = GetNode<DisplayMesh>("Background");
            Foreground = GetNode<DisplayMesh>("Foreground");

            // calculate quad size
            _quadWidth = _quadScale;
            _quadHeight = (Font.GlyphHeight / Font.GlyphWidth) * (Font.QuadHeightScale) * _quadScale;

            // derive display height from width
            if (AutoSize)
            {
                var screen = GetViewport();
                int maxDisplayHeight = Mathf.RoundToInt((screen.Size.y / screen.Size.x) * DisplayWidth / _quadHeight);
                DisplayHeight = maxDisplayHeight;
            }

            // initialize display meshes
            Background.Initialize(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, -0.001f);
            Foreground.Initialize(DisplayWidth, DisplayHeight, _quadWidth, _quadHeight, 0f);
            Background.MaterialOverride = BackgroundMaterial;
            Foreground.MaterialOverride = Font.BitmapFontMaterial;

            // initialize collision shape
            CollisionShape collisionShape = GetNode<CollisionShape>("StaticBody/CollisionShape");
            (collisionShape.Shape as BoxShape).SetExtents(new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * _quadHeight * 0.5f, 0.05f));
            collisionShape.Translation = new Vector3(DisplayWidth * _quadWidth * 0.5f, DisplayHeight * -_quadHeight * 0.5f, 0);

            // update camera orthographic size
            MainCamera.SetOrthogonal(Mathf.Max(DisplayHeight * _quadHeight * 1.0f, Background.Translation.y), 0, 2);
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
            if (!Engine.EditorHint)
            {
                // update cells
                SC.LinkedListNode<Cell> cellNode = _cellList.First;
                while (cellNode != null)
                {
                    cellNode.Value.Update(delta);
                    cellNode = cellNode.Next;
                }

                // update display meshes
                for (int y = 0; y < DisplayHeight; y++)
                {
                    for (int x = 0; x < DisplayWidth; x++)
                    {
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
                                Foreground.MeshVertices[vert].x = 0;
                                Foreground.MeshVertices[vert].y = 0;
                                Foreground.MeshUVs[vert] = zero2;
                                Foreground.MeshColors[vert] = ClearColor;
                                Background.MeshColors[vert] = cell != null ? cell.BackgroundColor : ClearColor;
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
                                Foreground.MeshVertices[vert].x = x * _quadWidth + glyph.Vertices[i].x * _quadWidth;
                                Foreground.MeshVertices[vert].y = -y * _quadHeight + glyph.Vertices[i].y * _quadHeight - _quadHeight;
                                Foreground.MeshUVs[vert] = glyph.UVs[i];
                                Foreground.MeshColors[vert] = cell.ForegroundColor;
                                Background.MeshColors[vert] = cell.BackgroundColor;
                            }
                        }
                    }
                }

                // apply display mesh updates
                Background.UpdateMesh();
                Foreground.UpdateMesh();

                // take screenshots
                if (Input.IsActionJustPressed("screenshot"))
                {
                    var image = GetViewport().GetTexture().GetData();
                    image.FlipY();
                    image.SavePng("user://screenshots/phiosScreenshot.png");
                    image = null;
                }
            }
        }
    } // end class
} // end namespace
