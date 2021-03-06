﻿using System.Xml.Linq;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace Editor
{
    public sealed class CompositeSub : CompositeBase
    {        
        RowContainer bottomRowContainer;

        public CompositeSub(IEquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            bottomRowContainer = new RowContainer(this);
            bottomRowContainer.FontFactor = SubFontFactor;
            bottomRowContainer.ApplySymbolGap = false;
            ChildEquations.AddRange(new IEquationBase[] { mainRowContainer, bottomRowContainer });
        }
        
        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(mainRowContainer.Serialize());
            thisElement.Add(bottomRowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            mainRowContainer.DeSerialize(xElement.Elements().First());
            bottomRowContainer.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;                
                mainRowContainer.Left = Left;
                bottomRowContainer.Left = mainRowContainer.Right;
            }
        }

        public override void CalculateWidth()
        {
            Width = mainRowContainer.Width + bottomRowContainer.Width;
        }

        public override void CalculateHeight()
        {
            Height = mainRowContainer.Height + bottomRowContainer.Height - SubOverlap;
        }


        public override double RefY
        {
            get
            {
                return mainRowContainer.RefY;
            }
        }


        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                mainRowContainer.Top = value;
                bottomRowContainer.Top = mainRowContainer.Bottom - SubOverlap;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (mainRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainRowContainer;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            else if (bottomRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = bottomRowContainer;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            return false;
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == Key.Down)
            {
                if (ActiveChild == mainRowContainer)
                {
                    ActiveChild = bottomRowContainer;
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == bottomRowContainer)
                {
                    ActiveChild = mainRowContainer;
                    return true;
                }
            }
            return false;
        }
    }
}
