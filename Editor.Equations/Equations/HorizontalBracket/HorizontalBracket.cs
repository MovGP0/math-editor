﻿using System;
using System.Linq;
using System.Xml.Linq;

namespace Editor
{
    public abstract class HorizontalBracket : EquationContainer
    {
        protected RowContainer topEquation;
        protected HorizontalBracketSign bracketSign;
        protected RowContainer bottomEquation;

        protected HorizontalBracket(IEquationContainer parent, HorizontalBracketSignType signType)
            : base(parent)
        {
            topEquation = new RowContainer(this);
            bottomEquation = new RowContainer(this);
            bracketSign = new HorizontalBracketSign(this, signType);
            ChildEquations.Add(topEquation);
            ChildEquations.Add(bracketSign);
            ChildEquations.Add(bottomEquation);            
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(bracketSign.SignType.GetType().Name, bracketSign.SignType));
            thisElement.Add(parameters);
            thisElement.Add(topEquation.Serialize());
            thisElement.Add(bottomEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elements = xElement.Elements(typeof(RowContainer).Name).ToArray();
            topEquation.DeSerialize(elements[0]);
            bottomEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                bracketSign.MidX = MidX;
                topEquation.MidX = MidX;
                bottomEquation.MidX = MidX;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                AdjustChildrenVertical();
            }
        }

        void AdjustChildrenVertical()
        {
            topEquation.Top = Top;
            bracketSign.Top = topEquation.Bottom;
            bottomEquation.Top = bracketSign.Bottom;            
        }

        public override void CalculateWidth()
        {
            Width = Math.Max(topEquation.Width, bottomEquation.Width) + FontSize * .6;
            bracketSign.Width = Width - FontSize * .2;            
        }

        public override void CalculateHeight()
        {
            Height = topEquation.Height + bottomEquation.Height + bracketSign.Height;
            AdjustChildrenVertical();
        }

        public override double RefY
        {
            get
            {
                if (bracketSign.SignType == HorizontalBracketSignType.TopCurly || bracketSign.SignType == HorizontalBracketSignType.ToSquare)
                {
                    return Height - bottomEquation.RefY;
                }
                else
                {
                    return topEquation.RefY;
                }
            }
        }
    }
}
