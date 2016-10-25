using System;
using System.Xml.Linq;

namespace Editor
{
    public abstract class Bracket : EquationContainer
    {
        protected RowContainer insideEq;
        protected BracketSign bracketSign;
        protected double ExtraHeight { get; set; }

        protected Bracket(IEquationContainer parent)
            : base(parent)
        {
            ExtraHeight = FontSize * 0.2;
            ActiveChild = insideEq = new RowContainer(this);
        }
        
        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(bracketSign.SignType.GetType().Name, bracketSign.SignType));
            thisElement.Add(parameters);
            thisElement.Add(insideEq.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            insideEq.DeSerialize(xElement.Element(insideEq.GetType().Name));
            CalculateSize();
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                insideEq.MidY = MidY;
                bracketSign.MidY = MidY;
            }
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        public override void CalculateWidth()
        {
            Width = insideEq.Width + bracketSign.Width;
        }

        public override void CalculateHeight()
        {
            ExtraHeight = FontSize * 0.2;
            var upperMax = insideEq.RefY;
            var lowerMax = insideEq.RefYReverse;
            Height = Math.Max(upperMax, lowerMax) * 2 + ExtraHeight;
            bracketSign.Height = Height;
        }
    }
}
