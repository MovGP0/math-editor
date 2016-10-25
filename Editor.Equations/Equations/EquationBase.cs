using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    public static class EquationBaseCommon
    {
        private const double DefaultFontSize = 20;
        public static TextManager TextManager = new TextManager();
        public static double LineFactor = 0.06;
        public static double LineThickness => DefaultFontSize * LineFactor;
        public static double ThinLineThickness => DefaultFontSize * LineFactor * 0.7;
        public static Pen StandardPen => PenManager.GetPen(LineThickness);
        public static Pen ThinPen => PenManager.GetPen(ThinLineThickness);
        public static Pen StandardMiterPen => PenManager.GetPen(LineThickness, PenLineJoin.Miter);
        public static Pen ThinMiterPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Miter);
        public static Pen StandardRoundPen => PenManager.GetPen(LineThickness, PenLineJoin.Round);
        public static Pen ThinRoundPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Round);
        public static double SubFontFactor = 0.6;
        public static double SubSubFontFactor = 0.7;
        public static Brush DebugBrush;
    }

    public abstract class EquationBase : IEquationBase
    {
        //do not use this.. just for debugging
        private int IndexInChildrenOfParent => ParentEquation.GetIndex(this);

        protected static TextManager TextManager = new TextManager();
        private static readonly Thickness ZeroMargin = new Thickness();
        public virtual Thickness Margin => ZeroMargin;

        protected static double LineFactor = 0.06;
        public virtual bool ApplySymbolGap { get; set; }

        public virtual HashSet<int> GetUsedTextFormats() { return null; }
        public virtual void ResetTextFormats(Dictionary<int, int> formatMapping) { }

        protected double LineThickness => _fontSize * LineFactor;
        protected double ThinLineThickness => _fontSize * LineFactor * 0.7;
        protected Pen StandardPen => PenManager.GetPen(LineThickness);
        protected Pen ThinPen => PenManager.GetPen(ThinLineThickness);
        protected Pen StandardMiterPen => PenManager.GetPen(LineThickness, PenLineJoin.Miter);
        protected Pen ThinMiterPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Miter);
        protected Pen StandardRoundPen => PenManager.GetPen(LineThickness, PenLineJoin.Round);
        protected Pen ThinRoundPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Round);

        public HAlignment HAlignment { get; set; }
        public VAlignment VAlignment { get; set; }
        public bool IsStatic { get; set; }
        public int SubLevel { get; set; }
        protected double SubFontFactor = 0.6;
        protected double SubSubFontFactor = 0.7;
        public static event EventHandler<EventArgs> SelectionAvailable;
        public static event EventHandler<EventArgs> SelectionUnavailable;

        private static bool _isSelecting;
        protected static bool IsSelecting
        {
            get { return _isSelecting; }
            set
            {
                _isSelecting = value;
                
                if (_isSelecting)
                {
                    if (SelectionAvailable == null)
                        throw new InvalidOperationException($"{nameof(SelectionAvailable)} was null");

                    SelectionAvailable(null, EventArgs.Empty); //there MUST always be one handler attached!
                }
                else
                {
                    if (SelectionUnavailable == null)
                        throw new InvalidOperationException($"{nameof(SelectionUnavailable)} was null");
                    SelectionUnavailable(null, EventArgs.Empty); //there MUST always be one handler attached!
                }
            }
        }

        public static bool ShowNesting { get; set; }
        public IEquationContainer ParentEquation { get; set; }
        private Point _location;
        private double _width;
        private double _height;
        private double _fontSize = 20;
        private double _fontFactor = 1;
        private readonly Pen _boxPen = new Pen(Brushes.Black, 1);
        public int SelectionStartIndex { get; set; }
        public int SelectedItems { get; set; } //this is a directed value (as on a real line!!)

        protected Brush DebugBrush;
        public byte R { get; } = 80;
        public byte G { get; } = 80;
        public byte B { get; } = 80;

        protected EquationBase(IEquationContainer parent)
        {
            ParentEquation = parent;
            if (parent != null)
            {
                SubLevel = parent.SubLevel;
                _fontSize = parent.FontSize;
                ApplySymbolGap = parent.ApplySymbolGap;
                R = (byte)(parent.R + 15);
                G = (byte)(parent.R + 15);
                B = (byte)(parent.R + 15);
            }
            DebugBrush = new SolidColorBrush(Color.FromArgb(100, R, G, B));
            DebugBrush.Freeze();
            _boxPen.Freeze();
        }

        public virtual bool ConsumeMouseClick(Point mousePoint) { return false; }
        public virtual void HandleMouseDrag(Point mousePoint) { }
        public virtual IEquationBase Split(IEquationContainer newParent) { return null; }
        public virtual void ConsumeText(string text) { }
        public virtual void ConsumeFormattedText(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo) { }
        public virtual bool ConsumeKey(Key key) { return false; }
        public virtual Point GetVerticalCaretLocation() { return _location; }
        public virtual double GetVerticalCaretLength() { return _height; }
        public virtual void CalculateWidth() { }
        public virtual void CalculateHeight() { }
        public virtual XElement Serialize() { return null; }
        public virtual void DeSerialize(XElement xElement) { }
        public virtual void StartSelection() { SelectedItems = 0; }
        public virtual bool Select(Key key) { return false; }
        public virtual void DeSelect() { SelectedItems = 0; }
        public virtual void RemoveSelection(bool registerUndo) { }
        public virtual Rect GetSelectionBounds() { return Rect.Empty; }
        public virtual CopyDataObject Copy(bool removeSelection) { return null; } //copy & cut
        public virtual void Paste(XElement xe) { }
        public virtual void SetCursorOnKeyUpDown(Key key, Point point) { }
        public virtual void ModifySelection(string operation, string argument, bool applied, bool addUndo) { }

        public virtual void CalculateSize()
        {
            CalculateWidth();
            CalculateHeight();
        }
        public virtual void SelectAll() { }
        public virtual string GetSelectedText() { return string.Empty; }
      
        public virtual void DrawEquation(DrawingContext dc)
        {
            if (ShowNesting)
            {
                dc.DrawRectangle(DebugBrush, null, Bounds);
            }
        }

        public virtual double FontFactor
        {
            get { return _fontFactor; }
            set
            {
                _fontFactor = value;
                FontSize = _fontSize; //fontsize needs adjustement!
            }
        }

        public virtual double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = Math.Min(1000, Math.Max(value * _fontFactor, 4));
            }
        }

        public virtual double RefX => _width / 2;

        public virtual double RefY => _height / 2;

        public double RefYReverse => _height - RefY;

        public virtual double Width
        {
            get { return _width; }
            set
            {
                _width = value;
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                _height = value > 0 ? value : 0;
            }
        }

        public Point Location
        {
            get { return _location; }
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }

        public virtual double Left
        {
            get { return _location.X; }
            set { _location.X = value; }
        }

        public virtual double Top
        {
            get { return _location.Y; }
            set { _location.Y = value; }
        }

        public double MidX
        {
            get { return _location.X + RefX; }
            set { Left = value - RefX; }
        }

        public double MidY
        {
            get { return _location.Y + RefY; }
            set { Top = value - RefY; }
        }

        public virtual double Right
        {
            get { return _location.X + _width; }
            set { Left = value - _width; }
        }

        public virtual double Bottom
        {
            get { return _location.Y + _height; }
            set { Top = value - _height; }
        }

        public Size Size => new Size(_width, _height);

        public Rect Bounds => new Rect(_location, Size);
    }
}
