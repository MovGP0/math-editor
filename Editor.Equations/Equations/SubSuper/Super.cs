using System.Xml.Linq;

namespace Editor
{
    public sealed class Super : SubSuperBase
    {
        private readonly RowContainer _rowContainer;       

        public Super(IEquationContainer parent, Position position)
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

        public override void CalculateHeight()
        {
           Height = _rowContainer.Height + Buddy.RefY - SuperOverlap;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _rowContainer.Top = value;
            }
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

        public override double RefY => Height;
    }
}
