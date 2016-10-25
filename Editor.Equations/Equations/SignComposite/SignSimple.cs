using System;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SignSimple : EquationContainer
    {
        private RowContainer MainEquation { get; set; }
        private readonly StaticSign _sign;            

        public SignSimple(IEquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = MainEquation = new RowContainer(this);
            _sign = new StaticSign(this, symbol, useUpright);  
            ChildEquations.AddRange(new IEquationBase[] {MainEquation, _sign});            
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_sign.Symbol.GetType().Name, _sign.Symbol));
            parameters.Add(new XElement(typeof(bool).FullName, _sign.UseItalicIntegralSign));
            thisElement.Add(parameters);            
            thisElement.Add(MainEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            MainEquation.DeSerialize(xElement.Element(MainEquation.GetType().Name));
            CalculateSize();
        }
        
        public override void CalculateWidth()
        {
            Width = _sign?.Width ?? 0d + MainEquation?.Width ?? 0d;
        }

        public override void CalculateHeight()
        {
            Height = Math.Max(_sign?.RefY ?? 0d, MainEquation?.RefY ?? 0d) + Math.Max(_sign?.RefY ?? 0d, (MainEquation?.Height ?? 0d) - (MainEquation?.RefY ?? 0d));
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _sign.MidY = MidY;
                MainEquation.MidY = MidY;
            }
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _sign.Left = value;
                MainEquation.Left = _sign.Right;
            }
        }

        public override double RefY => Math.Max(_sign.RefY, MainEquation.RefY);
    }
}