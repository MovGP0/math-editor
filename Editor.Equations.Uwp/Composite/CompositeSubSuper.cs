using System;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeSubSuper: CompositeBase
    {
        private RowContainer SuperRow { get; }
        private RowContainer SubRow { get; }  

        public CompositeSubSuper(EquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            SubRow = new RowContainer(this);
            SuperRow = new RowContainer(this)
            {
                FontFactor = SubRow.FontFactor = SubFontFactor,
                ApplySymbolGap = SubRow.ApplySymbolGap = false
            };
            childEquations.AddRange(new EquationBase[] { mainRowContainer, SubRow, SuperRow });
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                mainRowContainer.Left = value;
                SubRow.Left = mainRowContainer.Right;
                SuperRow.Left = mainRowContainer.Right;
            }
        }

        protected override void CalculateWidth()
        {
            Width = mainRowContainer.Width + Math.Max(SubRow.Width, SuperRow.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + SubRow.Height - SubOverlap + SuperRow.Height - SuperOverlap;
        }

        public override double RefY => SuperRow.Height - SuperOverlap + mainRowContainer.RefY;

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                SuperRow.Top = value;
                mainRowContainer.Top = SuperRow.Bottom - SuperOverlap;
                SubRow.Bottom = Bottom;
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
            else if (SubRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = SubRow;
                returnValue = true;
            }
            else if (SuperRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = SuperRow;
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
                        ActiveChild = SubRow;
                        return true;
                    }
                    if (ActiveChild == SuperRow)
                    {
                        ActiveChild = mainRowContainer;
                        return true;
                    }
                    return false;

                case VirtualKey.Up:
                    if (ActiveChild == SubRow)
                    {
                        ActiveChild = mainRowContainer;
                        return true;
                    }
                    if (ActiveChild == mainRowContainer)
                    {
                        ActiveChild = SuperRow;
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
