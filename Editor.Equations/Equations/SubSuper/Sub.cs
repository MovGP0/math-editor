﻿using System.Xml.Linq;

namespace Editor
{
    public class Sub : SubSuperBase
    {
        RowContainer rowContainer;

        public Sub(IEquationRow parent, Position position)
            : base(parent, position)
        {   
            ActiveChild = rowContainer = new RowContainer(this);
            ChildEquations.Add(rowContainer);
            if (SubLevel == 1)
            {
                rowContainer.FontFactor = SubFontFactor;
            }
            else if (SubLevel == 2)
            {
                rowContainer.FontFactor = SubSubFontFactor;
            }            
        }

        public override XElement Serialize()
        {
            XElement thisElement = new XElement(GetType().Name);
            XElement parameters = new XElement("parameters");
            parameters.Add(new XElement(Position.GetType().Name, Position));
            thisElement.Add(parameters);
            thisElement.Add(rowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            rowContainer.DeSerialize(xElement.Element(rowContainer.GetType().Name));
            CalculateSize();
        }

        public override void CalculateWidth()
        {
            Width = rowContainer.Width + Padding * 2;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                rowContainer.Left = this.Left + Padding;
            }
        }

        public override System.Windows.Thickness Margin
        {
            get
            {
                double left = 0;
                TextEquation te = Buddy as TextEquation;
                if (te != null)
                {
                    left += te.OverhangTrailing;
                }
                return new System.Windows.Thickness(left, 0, 0, 0);
            }
        }

        public override void CalculateHeight()
        {
            Height = rowContainer.Height - SubOverlap;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                rowContainer.Bottom = Bottom;
            }
        }

        public override double RefY
        {
            get
            {
                return 0;
            }
        }
    }
}
