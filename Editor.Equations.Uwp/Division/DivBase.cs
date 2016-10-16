using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public abstract class DivBase : EquationContainer
    {
        protected RowContainer TopEquation;
        protected RowContainer BottomEquation;

        protected DivBase(EquationContainer parent, bool isSmall = false)
            : base(parent)
        {
            if (isSmall) 
            {
                SubLevel++;
            }
            ActiveChild = TopEquation = new RowContainer(this);
            BottomEquation = new RowContainer(this);
            if (isSmall)
            {
                TopEquation.FontFactor = SubFontFactor;
                BottomEquation.FontFactor = SubFontFactor;
                TopEquation.ApplySymbolGap = false;
                BottomEquation.ApplySymbolGap = false;
            }
            childEquations.AddRange(new EquationBase[] { TopEquation, BottomEquation});
        }
        
        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (BottomEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = BottomEquation;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            else if (TopEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = TopEquation;
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
                if (ActiveChild == TopEquation)
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = BottomEquation;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == VirtualKey.Up)
            {
                if (ActiveChild == BottomEquation)
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = TopEquation;
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
