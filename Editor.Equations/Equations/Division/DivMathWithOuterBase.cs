﻿using System;
using System.Linq;
using System.Xml.Linq;

namespace Editor
{
    public abstract class DivMathWithOuterBase: DivMath
    {
        protected RowContainer outerEquation;

        protected DivMathWithOuterBase(IEquationContainer parent)
            : base(parent)
        {            
            outerEquation = new RowContainer(this);
            outerEquation.HAlignment = Editor.HAlignment.Right;
            //insideEquation.HAlignment = Editor.HAlignment.Right;
            ChildEquations.Add(outerEquation);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(insideEquation.Serialize());
            thisElement.Add(outerEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elements = xElement.Elements().ToArray();
            insideEquation.DeSerialize(elements[0]);
            outerEquation.DeSerialize(elements[1]);
            CalculateSize();
        }       

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        public override void CalculateWidth()
        {
            Width = Math.Max(insideEquation.Width, outerEquation.Width) + divMathSign.Width + LeftGap;
        }

        public override void CalculateHeight()
        {            
            Height = outerEquation.Height + insideEquation.Height + ExtraHeight;
            divMathSign.Height = insideEquation.FirstRow.Height + ExtraHeight;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                divMathSign.Left = value + LeftGap;
                insideEquation.Right = Right;
                outerEquation.Right = Right;
            }
        }
    }
}
