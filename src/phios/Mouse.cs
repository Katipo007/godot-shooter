using System;
using Godot;

namespace Phios
{
    public class Mouse : Node
    {
        [Export]
        public bool HideNativeCursor { get; set; } = false;

        [Export]
        public int ReservedLayer { get; set; } = -1;

        [Export]
        public string CursorUp { get; set; } = "░";

        [Export]
        public string CursorDown { get; set; } = "█";

        [Export]
        public Color CursorColor { get; set; } = Colors.Yellow;

        [Export]
        public float CursorFadeTime { get; set; } = 0.25f;

        [Export]
        public bool FadeToClear { get; set; } = true;

        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// Cursor position on the screen
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position.Set(
                    value.x.Clamp(0, Display.DisplayWidth),
                    value.y.Clamp(0, Display.DisplayHeight)
                );
            }
        }

        /// <summary>
        /// Minimum percentage of the screen the cursor is allowed in
        /// </summary>
        [Export]
        public Vector2 ScreenMin { get; set; } = new Vector2(0f, 0f);

        /// <summary>
        /// Maximum percentage of the screen the cursor is allowed in
        /// </summary>
        [Export]
        public Vector2 ScreenMax { get; set; } = new Vector2(1f, 1f);

        private Vector2 _position;
        private Cell _currentCell;
        private Cell _currentCellHover;

        private IHoverAction _hoverAction;
        private bool _dragging = false;
        private Vector2 _dragStart;
        private IDragAction _dragAction;

        private Display Display;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (HideNativeCursor)
                Utils.CaptureMouse();

            this.Display = GetParent<Display>();

            Initialized = true;
        }

        public override string _GetConfigurationWarning()
        {
            if (GetParentOrNull<Display>() == null)
                return "Parent node must be a Display";

            return "";
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            if (Initialized)
            {

                // clamp mouse position to bounds
                _position.Set(
                    Utils.Clamp(_position.x, Display.DisplayWidth * ScreenMin.x, Display.DisplayWidth * ScreenMax.x),
                    Utils.Clamp(_position.y, Display.DisplayHeight * ScreenMin.y, Display.DisplayHeight * ScreenMax.y));

                // clear current cell
                if (_currentCell != null)
                {
                    // get background color to clear to
                    Color clearColor = Display.GetBackgroundColorForCell(
                        (int) _position.x,
                        (int) _position.y,
                        ReservedLayer
                    );

                    // clear cell content
                    _currentCell.SetContent(
                        "",
                        clearColor,
                        FadeToClear ? Display.ClearColor : CursorColor,
                        CursorFadeTime,
                        CursorColor,
                        CellFades.DEFAULT_REVERSE);

                    _currentCell = null;
                }

                // reset current cell hover
                _currentCellHover = null;

                // new current cell
                _currentCell = Display.GetCell(ReservedLayer, (int) _position.x, (int) _position.y);

                // get background color for current cell
                Color currentCellBackgroundColor = Display.GetBackgroundColorForCell(
                    (int) _position.x,
                    (int) _position.y,
                    ReservedLayer);

                // highlight cell
                _currentCell.SetContent(
                    Input.IsActionPressed("phios_click") ? CursorDown : CursorUp,
                    currentCellBackgroundColor,
                    CursorColor,
                    0f,
                    CursorColor,
                    "");

                // hover
                if (!_dragging)
                {
                    for (int i = Display.GetNumLayers() - 1; i >= 0; i--)
                    {
                        Cell cellHover = Display.GetCell(i, (int) _position.x, (int) _position.y);

                        // hover on topmost layer
                        if (cellHover.Content != "")
                        {
                            // set new hover cell
                            _currentCellHover = cellHover;

                            // new hover cell has a hover action
                            if (_currentCellHover.HoverAction != null)
                            {

                                // current hover action is different to new hover action
                                if (_hoverAction != _currentCellHover.HoverAction)
                                {

                                    // current hover exit
                                    if (_hoverAction != null)
                                    {
                                        _hoverAction.OnHoverExit();
                                    }

                                    // new hover enter
                                    _hoverAction = _currentCellHover.HoverAction;
                                    _hoverAction.OnHoverEnter();
                                }
                            }

                            // new hover cell has no hover action, just exit current hover action
                            else if (_hoverAction != null)
                            {
                                _hoverAction.OnHoverExit();
                                _hoverAction = null;
                            }

                            break;
                        } // end cellHover.Content != ""
                    }
                } // end !_dragging

                // click
                if (!_dragging)
                {
                    if (Input.IsActionJustPressed("phios_click") &&
                        _currentCellHover != null &&
                        _currentCellHover.ClickAction != null)
                    {
                        _currentCellHover.ClickAction.OnMouseDown();
                    }
                }

                // drag start
                if (!_dragging &&
                    Input.IsActionJustPressed("phios_drag") &&
                    _currentCellHover != null &&
                    _currentCellHover.DragAction != null)
                {
                    _dragging = true;
                    _dragStart = _currentCell.Position;
                    _dragAction = _currentCellHover.DragAction;
                    _dragAction.OnDragStart();
                }

                // drag end
                else if (_dragging &&
                    Input.IsActionJustReleased("phios_drag") &&
                    _dragAction != null)
                {
                    _dragging = false;
                    Vector2 dragDelta = _currentCell.Position - _dragStart;
                    _dragAction.OnDragDelta(dragDelta);
                    _dragAction.OnDragEnd();
                    _dragAction = null;
                }

                // drag delta
                else if (_dragging && _dragAction != null)
                {
                    Vector2 dragDelta = _currentCell.Position - _dragStart;
                    _dragAction.OnDragDelta(dragDelta);
                }

                // scroll
                if (!_dragging)
                {
                    int scrollDelta = (Input.IsActionPressed("phios_scroll_up") ? -1 : 0) + (Input.IsActionPressed("phios_scroll_down") ? 1 : 0);
                    if (scrollDelta != 0 &&
                        _currentCellHover != null &&
                        _currentCellHover.ScrollAction != null)
                    {
                        _currentCellHover.ScrollAction.OnScrollDelta(scrollDelta);
                    }
                }
            }
        }
    } // end class
} // end namespace
