using System;
using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public class Caret : FrameworkElement
    {
        private Point _location;
        public double CaretLength { get; set; }
        private readonly bool _isHorizontal;

        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(Caret), new FrameworkPropertyMetadata(false /* defaultValue */, FrameworkPropertyMetadataOptions.AffectsRender));

        public Caret(bool isHorizontal)
        {
            _isHorizontal = isHorizontal;
            CaretLength = 18;
            Visible = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Visible)
            {
                dc.DrawLine(PenManager.GetPen(Math.Max(1, EditorControlGlobal.RootFontSize * .8 / EditorControlGlobal.RootFontBaseSize)), _location, OtherPoint);
            }
            else if (_isHorizontal)
            {
                dc.DrawLine(PenManager.GetWhitePen(Math.Max(1, EditorControlGlobal.RootFontSize *.8 / EditorControlGlobal.RootFontBaseSize)), _location, OtherPoint);
            }
        }

        private Point OtherPoint => _isHorizontal 
            ? new Point(Left + CaretLength, Top) 
            : new Point(Left, VerticalCaretBottom);

        public void ToggleVisibility()
        {
            Dispatcher.Invoke(new Action(() => { Visible = !Visible; }));
        }

        private bool Visible
        {
            get
            {
                return (bool)GetValue(VisibleProperty);
            }
            set
            {
                SetValue(VisibleProperty, value);
            }
        }

        public Point Location
        {
            get { return _location; }
            set
            {
                _location.X = Math.Floor(value.X) + .5;
                _location.Y = Math.Floor(value.Y) + .5;
                if (Visible)
                {
                    Visible = false;
                }
            }
        }

        public double Left
        {
            get { return _location.X; }
            set
            {
                _location.X = Math.Floor(value) + .5;
                if (Visible)
                {
                    Visible = false;
                }
            }
        }

        public double Top
        {
            get { return _location.Y; }
            set
            {
                _location.Y = Math.Floor(value) + .5;
                if (Visible)
                {
                    Visible = false;
                }
            }
        }

        public double VerticalCaretBottom => _location.Y + CaretLength;
    }
}
