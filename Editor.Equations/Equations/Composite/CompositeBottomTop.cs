﻿using System;
using System.Windows;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Input;

namespace Editor
{
    public sealed class CompositeBottomTop : CompositeBase
    {
        RowContainer topRow;        
        RowContainer bottomRow;

        public CompositeBottomTop(IEquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            bottomRow = new RowContainer(this);
            topRow = new RowContainer(this);
            topRow.FontFactor = bottomRow.FontFactor = SubFontFactor;
            topRow.ApplySymbolGap = bottomRow.ApplySymbolGap = false;
            ChildEquations.AddRange(new IEquationBase[] { mainRowContainer, bottomRow, topRow });
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(mainRowContainer.Serialize());
            thisElement.Add(bottomRow.Serialize());
            thisElement.Add(topRow.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elementArray = xElement.Elements().ToArray();
            mainRowContainer.DeSerialize(elementArray[0]);
            bottomRow.DeSerialize(elementArray[1]);
            topRow.DeSerialize(elementArray[2]);
            CalculateSize();
        }         
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                bottomRow.MidX = MidX;
                mainRowContainer.MidX = MidX;
                topRow.MidX = MidX;
            }
        }

        public override void CalculateWidth()
        {
            Width = Math.Max(Math.Max(mainRowContainer.Width, bottomRow.Width), topRow.Width);
        }

        public override void CalculateHeight()
        {
            Height = mainRowContainer.Height + bottomRow.Height + topRow.Height + bottomGap;            
        }

        public override double RefY
        {
            get
            {
                return topRow.Height + mainRowContainer.RefY;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                topRow.Top = value;
                mainRowContainer.Top = topRow.Bottom;
                bottomRow.Top = mainRowContainer.Bottom + bottomGap;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            var returnValue = false;
            if (mainRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainRowContainer;
                returnValue = true;
            }
            else if (bottomRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = bottomRow;
                returnValue = true;
            }
            else if (topRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = topRow;
                returnValue = true;
            }
            ActiveChild.ConsumeMouseClick(mousePoint);
            return returnValue;
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
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = bottomRow;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
                else if (ActiveChild == topRow)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = mainRowContainer;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == bottomRow)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = mainRowContainer;
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
                else if (ActiveChild == mainRowContainer)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = topRow;
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
