using System.Linq;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SquareRoot : EquationContainer
    {
        private readonly RowContainer _insideEquation;
        private readonly RadicalSign _radicalSign;
        private double ExtraHeight => FontSize * .15;
        private double LeftGap => FontSize * .1;

        public SquareRoot(IEquationContainer parent)
            : base(parent)
        {            
            _radicalSign = new RadicalSign(this);
            ActiveChild = _insideEquation = new RowContainer(this);
            ChildEquations.Add(_insideEquation);
            ChildEquations.Add(_radicalSign);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_insideEquation.Serialize());            
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEquation.DeSerialize(xElement.Elements().First());            
            CalculateSize();
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                AdjustVertical();
            }
        }

        private void AdjustVertical()
        {
            _insideEquation.Bottom = Bottom;
            _radicalSign.Top = Top;
        }

        public override void CalculateWidth()
        {
            Width = _insideEquation.Width + _radicalSign.Width + LeftGap;
        }

        public override void CalculateHeight()
        {
            Height = _insideEquation.Height + ExtraHeight;
            _radicalSign.Height = Height;
            AdjustVertical();
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _radicalSign.Left = value + LeftGap;
                _insideEquation.Left = _radicalSign.Right;
            }
        }

        public override double RefY => _insideEquation.RefY + ExtraHeight;
    }
}
