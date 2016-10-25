using System.Xml.Linq;

namespace Editor
{
    public sealed class Sub : SubSuperBase
    {
        private readonly RowContainer _rowContainer;

        public Sub(IEquationContainer parent, Position position)
            : base(parent, position)
        {   
            ActiveChild = _rowContainer = new RowContainer(this);
            ChildEquations.Add(_rowContainer);

            switch (SubLevel)
            {
                case 1:
                    _rowContainer.FontFactor = SubFontFactor;
                    break;
                case 2:
                    _rowContainer.FontFactor = SubSubFontFactor;
                    break;
            }            
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(Position.GetType().Name, Position));
            thisElement.Add(parameters);
            thisElement.Add(_rowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _rowContainer.DeSerialize(xElement.Element(_rowContainer.GetType().Name));
            CalculateSize();
        }

        public override void CalculateWidth()
        {
            Width = _rowContainer.Width + Padding * 2;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _rowContainer.Left = Left + Padding;
            }
        }

        public override System.Windows.Thickness Margin
        {
            get
            {
                double left = 0;
                var te = Buddy as TextEquation;
                if (te != null)
                {
                    left += te.OverhangTrailing;
                }
                return new System.Windows.Thickness(left, 0, 0, 0);
            }
        }

        public override void CalculateHeight()
        {
            Height = _rowContainer.Height - SubOverlap;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _rowContainer.Bottom = Bottom;
            }
        }

        public override double RefY => 0;
    }
}
