using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeSuper : CompositeBase
    {
        private RowContainer TopRowContainer { get; }       

        public CompositeSuper(EquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            TopRowContainer = new RowContainer(this)
            {
                FontFactor = SubFontFactor,
                ApplySymbolGap = false
            };
            childEquations.AddRange(new EquationBase[] { mainRowContainer, TopRowContainer });
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                mainRowContainer.Left = Left;
                TopRowContainer.Left = mainRowContainer.Right;
                
            }
        }

        protected override void CalculateWidth()
        {
            Width = mainRowContainer.Width + TopRowContainer.Width;
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + TopRowContainer.Height - SuperOverlap;            
        }

        public override double RefY => mainRowContainer.RefY + TopRowContainer.Height - SuperOverlap;

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                TopRowContainer.Top = value;
                mainRowContainer.Bottom = Bottom;
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
            if (TopRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = TopRowContainer;
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

            if (key == VirtualKey.Down)
            {
                if (ActiveChild == TopRowContainer)
                {
                    ActiveChild = mainRowContainer;
                    return true;
                }
                return false;
            }

            if (key == VirtualKey.Up)
            {
                if (ActiveChild == mainRowContainer)
                {
                    ActiveChild = TopRowContainer;
                    return true;
                }
                return false;
            }

            return false;
        }        
    }
}
