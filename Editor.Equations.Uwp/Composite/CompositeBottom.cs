using System;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeBottom : CompositeBase
    {        
        RowContainer bottomRowContainer;

        public CompositeBottom(EquationContainer parent)
            : base(parent)
        {
            this.SubLevel++;
            bottomRowContainer = new RowContainer(this);
            bottomRowContainer.FontFactor = SubFontFactor;
            bottomRowContainer.ApplySymbolGap = false;            
            childEquations.AddRange(new EquationBase[] { mainRowContainer, bottomRowContainer });
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                bottomRowContainer.MidX = MidX;
                mainRowContainer.MidX = MidX;
            }
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(mainRowContainer.Width, bottomRowContainer.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + bottomRowContainer.Height + bottomGap;
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
                bottomRowContainer.Top = mainRowContainer.Bottom + bottomGap;
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

        public override bool ConsumeKey(VirtualKey key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }

            switch (key)
            {
                case VirtualKey.Down:
                    if (ActiveChild == mainRowContainer)
                    {
                        Point point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = bottomRowContainer;
                        point.Y = ActiveChild.Top + 1;
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }
                    break;
                case VirtualKey.Up:
                    if (ActiveChild == bottomRowContainer)
                    {
                        Point point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = mainRowContainer;
                        point.Y = ActiveChild.Bottom - 1;
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
