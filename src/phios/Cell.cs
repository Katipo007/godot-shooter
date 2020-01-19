using System;
using Godot;

namespace Phios
{
    public class CellFades
    {
        public static string DEFAULT_REVERSE = "░▒▓█";
        public static string DEFAULT = "█▓▒░";
    }

    public class Cell
    {
        public int Layer { get; set; }
        public Vector2 Position { get; set; }
        public Display Owner { get; set; }
        public string Content { get; set; } = "";
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }

        #region Actions
        public IHoverAction HoverAction { get; set; }
        public IClickAction ClickAction { get; set; }
        public IDragAction DragAction { get; set; }
        public IScrollAction ScrollAction { get; set; }
        #endregion

        private string _targetContent = "";
        private Color _targetColor;
        private float _fadeLeft = 0f;
        private float _fadeMax = 0f;
        private Color _fadeColor;
        private string _fades = "";
        private bool _fadeFinished = true;

        public static readonly Color ClearColour = Color.Color8(0, 0, 0, 0);

        public void Clear()
        {
            SetContent("", ClearColour, ClearColour);
        }

        public void Clear(float fadeTime, Color fadeColor)
        {
            SetContent("", ClearColour, ClearColour, fadeTime, fadeColor, CellFades.DEFAULT_REVERSE);
        }

        public void SetContent(string content, Color backgroundColor, Color foregroundColor)
        {
            SetContent(content, backgroundColor, foregroundColor, 0f, foregroundColor, "");
        }

        public void SetContent(string content, Color backgroundColor, Color foregroundColor, float fadeTime, Color fadeColor)
        {
            SetContent(content, backgroundColor, foregroundColor, fadeTime, fadeColor, CellFades.DEFAULT);
        }

        public void SetContent(string content, Color backgroundColor, Color foregroundColor, float fadeMax, Color fadeColor, string fades)
        {
            // set targets
            _targetContent = content;
            this.BackgroundColor = backgroundColor;
            _targetColor = foregroundColor;

            // fade
            if (fadeMax > 0f)
            {
                _fadeMax = (float) GD.RandRange(0f, fadeMax);
                _fadeLeft = _fadeMax;

                this.ForegroundColor = fadeColor;
                _fadeColor = fadeColor;
                _fades = fades;
                _fadeFinished = false;
            }
            // instant
            else
            {
                _fadeLeft = 0f;
                _fadeMax = 0f;
                _fadeFinished = true;
            }

            // add cell to top layer
            if (_targetContent != "")
            {
                Owner.AddCellAsTopLayer(this);
            }
        }

        public void Update(float deltaTime)
        {
            // display initialized
            if (Owner != null)
            {
                // fade
                if (_fadeLeft > 0f)
                {
                    Content = (_targetContent.Trim().Length > 0 || Content.Trim().Length > 0) ? _fades.Substring(Mathf.RoundToInt((_fadeLeft / _fadeMax) * (_fades.Length - 1)), 1) : _targetContent;
                    ForegroundColor = _targetColor.LinearInterpolate(_fadeColor, Owner.ColorLerpCurve.Interpolate(_fadeLeft / _fadeMax));
                    _fadeLeft -= deltaTime;
                }
                // fade finished
                else
                {
                    // remove cell from top layer
                    if (!_fadeFinished && _targetContent == "")
                    {
                        Owner.RemoveCellAsTopLayer(this);
                    }

                    _fadeFinished = true;
                    _fadeLeft = 0f;
                    Content = _targetContent;
                    ForegroundColor = _targetColor;
                }
            }
        }
    } // end class
} // end namespace
