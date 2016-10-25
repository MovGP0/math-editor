using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public sealed class Box : EquationContainer
    {
        private readonly BoxType _boxType;
        private readonly RowContainer _insideEq;
        private readonly double _paddingFactor = 0.2;
        private double TopPadding => _boxType == BoxType.All || _boxType == BoxType.LeftTop || _boxType == BoxType.RightTop ? 0 : 0;

        private double BottomPadding => _boxType == BoxType.All || _boxType == BoxType.LeftBottom || _boxType == BoxType.RightBottom
            ? _paddingFactor*FontSize
            : 0;

        private double LeftPadding => _boxType == BoxType.All || _boxType == BoxType.LeftTop || _boxType == BoxType.LeftBottom
            ? _paddingFactor*FontSize
            : 0;

        private double RightPadding => _boxType == BoxType.All || _boxType == BoxType.RightTop || _boxType == BoxType.RightBottom
            ? _paddingFactor*FontSize
            : 0;

        private Point LeftTop => new Point(Left + LeftPadding / 2, Top + TopPadding / 2);
        private Point RightTop => new Point(Right - RightPadding / 2, Top + TopPadding / 2);
        private Point LeftBottom => new Point(Left + LeftPadding / 2, Bottom - BottomPadding / 2);
        private Point RightBottom => new Point(Right - RightPadding / 2, Bottom - BottomPadding / 2);

        public Box(IEquationContainer parent, BoxType boxType)
            :base (parent)
        {
            _boxType = boxType;
            ActiveChild = _insideEq = new RowContainer(this);
            ChildEquations.Add(_insideEq);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_boxType.GetType().Name, _boxType));
            thisElement.Add(parameters);
            thisElement.Add(_insideEq.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEq.DeSerialize(xElement.Element(_insideEq.GetType().Name));
            CalculateSize();
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            switch (_boxType)
            {
                case BoxType.All:
                    dc.DrawPolyline(LeftTop, new PointCollection{RightTop, RightBottom, LeftBottom, LeftTop, RightTop}, StandardMiterPen);
                    break;
                case BoxType.LeftBottom:
                    dc.DrawPolyline(LeftTop, new PointCollection { LeftBottom, RightBottom }, StandardMiterPen);
                    break;
                case BoxType.LeftTop:
                    dc.DrawPolyline(RightTop, new PointCollection { LeftTop, LeftBottom }, StandardMiterPen);
                    break;
                case BoxType.RightBottom:
                    dc.DrawPolyline(RightTop, new PointCollection { RightBottom, LeftBottom }, StandardMiterPen);
                    break;
                case BoxType.RightTop:
                    dc.DrawPolyline(LeftTop, new PointCollection { RightTop, RightBottom }, StandardMiterPen);
                    break;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _insideEq.Top = value + TopPadding;        
            }
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _insideEq.Left = value + LeftPadding;
            }
        }

        public override void CalculateWidth()
        {
            Width = _insideEq.Width + LeftPadding + RightPadding; 
        }

        public override void CalculateHeight()
        {
            Height = _insideEq.Height + TopPadding + BottomPadding;            
        }

        public override double RefY => _insideEq.RefY + TopPadding;
    }
}
