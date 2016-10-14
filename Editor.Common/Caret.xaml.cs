using System;
using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public partial class Caret
    {
        #region DependentProperties
        public static readonly DependencyProperty GeometryPathProperty = DependencyProperty.Register(
            nameof(GeometryPath), typeof(Geometry), typeof(Caret), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange));

        public Geometry GeometryPath
        {
            get { return (Geometry) GetValue(GeometryPathProperty); }
            set { SetValue(GeometryPathProperty, value); }
        }

        public static readonly DependencyProperty IsHorizontalProperty = DependencyProperty.Register(
            nameof(IsHorizontal), typeof(bool), typeof(Caret), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsHorizontal
        {
            get { return (bool) GetValue(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }
        
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Point), typeof(Caret), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsMeasure));
        
        /// <summary>
        /// Top-left corner of the caret
        /// </summary>
        public Point Location
        {
            get { return (Point) GetValue(LocationProperty); }
            set
            {
                var locX = Math.Floor(value.X) + .5;
                var locY = Math.Floor(value.Y) + .5;
                SetValue(LocationProperty, new Point(locX, locY));
            }
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(Caret), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double StrokeThickness
        {
            get { return (double) GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        
        public static readonly DependencyProperty CaretLengthProperty = DependencyProperty.Register(
            nameof(CaretLength), typeof(double), typeof(Caret), new FrameworkPropertyMetadata(18.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double CaretLength
        {
            get { return (double) GetValue(CaretLengthProperty); }
            set { SetValue(CaretLengthProperty, value); }
        }
        #endregion

        #region Properties
        public double Top
        {
            get { return Location.Y; }
            set
            {
                Location = new Point(Location.X, value);
            }
        }

        public double VerticalCaretBottom => Location.Y + CaretLength;
        
        public double Left
        {
            get { return Location.X; }
            set { Location = new Point(value, Location.Y); }
        }
        
        private Point OtherPoint => IsHorizontal 
            ? new Point(Left + CaretLength, Top) 
            : new Point(Left, VerticalCaretBottom);
        #endregion

        public Caret(bool isHorizontal)
        {
            IsHorizontal = isHorizontal;
            StrokeThickness = Math.Max(1, EditorControlGlobal.RootFontSize*.8/EditorControlGlobal.RootFontBaseSize);
            InitializeComponent();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var location = Location;
            var otherPoint = OtherPoint;
            
            GeometryPath = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure(location, new PathSegment[]
                    {
                        new LineSegment(otherPoint, true) 
                    }, false)
                }
            };
        
            return new Size(otherPoint.X+StrokeThickness, otherPoint.Y+StrokeThickness);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var otherPoint = OtherPoint;
            base.ArrangeOverride(arrangeBounds);
            return new Size(otherPoint.X+StrokeThickness, otherPoint.Y+StrokeThickness);
        }
    }
}
