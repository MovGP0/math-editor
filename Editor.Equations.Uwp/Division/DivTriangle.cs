﻿namespace Editor
{
    internal sealed class DivTriangle : EquationContainer 
    {
        private readonly RowContainer _insideEquation;
        private readonly DivTriangleSign _divTriangleSign;
        private readonly bool _isFixed;

        private double ExtraHeight => FontSize * .2;

        private double VerticalGap => FontSize * .1;

        private double LeftGap => FontSize * .1;

        public DivTriangle(EquationContainer parent, bool isFixed)
            : base(parent)
        {
            _isFixed = isFixed;
            _divTriangleSign = new DivTriangleSign(this);
            ActiveChild = _insideEquation = new RowContainer(this);
            childEquations.Add(_insideEquation);
            childEquations.Add(_divTriangleSign);
        }
        
        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                AdjustVertical();
            }
        }

        void AdjustVertical()
        {
            _divTriangleSign.Bottom = Bottom;
            _insideEquation.Top = Top; //Bottom - VerticalGap;            
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = _insideEquation.Width + _divTriangleSign.Width + LeftGap * 2;
        }

        protected override void CalculateHeight()
        {            
            if (_isFixed)
            {
                 _divTriangleSign.Height = _insideEquation.LastRow.Height + ExtraHeight;
            }
            else
            {
                 _divTriangleSign.Height = _insideEquation.Height + ExtraHeight;
            }
            Height = _insideEquation.Height + ExtraHeight;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                _divTriangleSign.Left = value + LeftGap;
                _insideEquation.Left = _divTriangleSign.Right + LeftGap;
            }
        }

        public override double RefY => _insideEquation.LastRow.MidY - Top;
    }
}
