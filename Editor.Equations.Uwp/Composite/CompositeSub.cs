using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeSub : CompositeBase
    {
        private RowContainer BottomRowContainer { get; }

        public CompositeSub(EquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            BottomRowContainer = new RowContainer(this)
            {
                FontFactor = SubFontFactor,
                ApplySymbolGap = false
            };
            childEquations.AddRange(new EquationBase[] { mainRowContainer, BottomRowContainer });
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;                
                mainRowContainer.Left = Left;
                BottomRowContainer.Left = mainRowContainer.Right;
            }
        }

        protected override void CalculateWidth()
        {
            Width = mainRowContainer.Width + BottomRowContainer.Width;
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + BottomRowContainer.Height - SubOverlap;
        }
        
        public override double RefY => mainRowContainer.RefY;
        
        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                mainRowContainer.Top = value;
                BottomRowContainer.Top = mainRowContainer.Bottom - SubOverlap;
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

            if (BottomRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = BottomRowContainer;
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
                        ActiveChild = BottomRowContainer;
                        return true;
                    }
                    return false;

                case VirtualKey.Up:
                    if (ActiveChild == BottomRowContainer)
                    {
                        ActiveChild = mainRowContainer;
                        return true;
                    }
                    return false;

                default: 
                    return false;
            }
        }
    }
}
