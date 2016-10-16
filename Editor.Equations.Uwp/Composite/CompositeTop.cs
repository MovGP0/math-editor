using System;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class CompositeTop : CompositeBase
    {
        private readonly RowContainer _topRowContainer;       

        public CompositeTop(EquationContainer parent)
            : base(parent)
        {
            SubLevel++;
            _topRowContainer = new RowContainer(this)
            {
                FontFactor = SubFontFactor,
                ApplySymbolGap = false
            };
            childEquations.AddRange(new EquationBase[] { mainRowContainer, _topRowContainer });
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _topRowContainer.MidX = MidX;
                mainRowContainer.MidX = MidX;
            }
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(mainRowContainer.Width, _topRowContainer.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + _topRowContainer.Height;            
        }

        public override double RefY => Height - mainRowContainer.Height + mainRowContainer.RefY;

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _topRowContainer.Top = value;
                mainRowContainer.Top = _topRowContainer.Bottom;
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
            if (_topRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = _topRowContainer;
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
                    if (ActiveChild == _topRowContainer)
                    {
                        ActiveChild = mainRowContainer;
                        return true;
                    }
                    break;
                case VirtualKey.Up:
                    if (ActiveChild == mainRowContainer)
                    {
                        ActiveChild = _topRowContainer;
                        return true;
                    }
                    break;
            }
            return false;
        }        
    }
}
