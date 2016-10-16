using System;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeBottomTop : CompositeBase
    {
        private RowContainer TopRow { get; }
        private RowContainer BottomRow { get; }

        public CompositeBottomTop(EquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            BottomRow = new RowContainer(this);
            TopRow = new RowContainer(this)
            {
                FontFactor = BottomRow.FontFactor = SubFontFactor,
                ApplySymbolGap = BottomRow.ApplySymbolGap = false
            };
            childEquations.AddRange(new EquationBase[] { mainRowContainer, BottomRow, TopRow });
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                BottomRow.MidX = MidX;
                mainRowContainer.MidX = MidX;
                TopRow.MidX = MidX;
            }
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(Math.Max(mainRowContainer.Width, BottomRow.Width), TopRow.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + BottomRow.Height + TopRow.Height + bottomGap;            
        }

        public override double RefY => TopRow.Height + mainRowContainer.RefY;

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                TopRow.Top = value;
                mainRowContainer.Top = TopRow.Bottom;
                BottomRow.Top = mainRowContainer.Bottom + bottomGap;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            bool returnValue = false;
            if (mainRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainRowContainer;
                returnValue = true;
            }
            else if (BottomRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = BottomRow;
                returnValue = true;
            }
            else if (TopRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = TopRow;
                returnValue = true;
            }
            ActiveChild.ConsumeMouseClick(mousePoint);
            return returnValue;
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
                        ActiveChild = BottomRow;
                        point.Y = ActiveChild.Top + 1;
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }
                    if (ActiveChild == TopRow)
                    {
                        Point point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = mainRowContainer;
                        point.Y = ActiveChild.Top + 1;
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }
                    break;

                case VirtualKey.Up:
                    if (ActiveChild == BottomRow)
                    {
                        Point point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = mainRowContainer;
                        point.Y = ActiveChild.Bottom - 1;
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }

                    if (ActiveChild == mainRowContainer)
                    {
                        Point point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = TopRow;
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
