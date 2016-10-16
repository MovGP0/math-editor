using System;
using System.Linq;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class Arrow : EquationContainer
    {
        private readonly RowContainer _rowContainer1;
        private readonly RowContainer _rowContainer2;
        private DecorationDrawing _arrow1;
        private DecorationDrawing _arrow2;
        private readonly ArrowType _arrowType;
        private readonly Position _equationPosition;

        private double ArrowGap
        {
            get
            {
                if (_arrowType == ArrowType.SmallRightLeftHarpoon || _arrowType == ArrowType.RightSmallLeftHarpoon || 
                    _arrowType == ArrowType.RightLeftHarpoon)
                {
                    return FontSize * .2;
                }
                return 0; //FontSize * .02;
            }
        }

        public Arrow(EquationContainer parent, ArrowType arrowType, Position equationPosition)
            : base(parent)
        {
            _arrowType = arrowType;
            _equationPosition = equationPosition;
            SubLevel++;
            ApplySymbolGap = false;
            ActiveChild = _rowContainer1 = new RowContainer(this);
            _rowContainer1.FontFactor = SubFontFactor;
            childEquations.Add(_rowContainer1);
            CreateDecorations();

            if (equationPosition == Position.BottomAndTop)
            {
                _rowContainer2 = new RowContainer(this);
                _rowContainer2.FontFactor = SubFontFactor;
                childEquations.Add(_rowContainer2);
            }
        }

        private void CreateDecorations()
        {
            switch (_arrowType)
            {
                case ArrowType.LeftArrow:
                    _arrow1 = new DecorationDrawing(this, DecorationType.LeftArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.RightArrow:
                    _arrow1 = new DecorationDrawing(this, DecorationType.RightArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.DoubleArrow:
                    _arrow1 = new DecorationDrawing(this, DecorationType.DoubleArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.RightLeftArrow:
                case ArrowType.RightSmallLeftArrow:
                case ArrowType.SmallRightLeftArrow:
                    _arrow1 = new DecorationDrawing(this, DecorationType.RightArrow);
                    _arrow2 = new DecorationDrawing(this, DecorationType.LeftArrow);
                    childEquations.Add(_arrow1);
                    childEquations.Add(_arrow2);
                    break;
                case ArrowType.RightLeftHarpoon:
                case ArrowType.RightSmallLeftHarpoon:
                case ArrowType.SmallRightLeftHarpoon:
                    _arrow1 = new DecorationDrawing(this, DecorationType.RightHarpoonUpBarb);
                    _arrow2 = new DecorationDrawing(this, DecorationType.LeftHarpoonDownBarb);
                    childEquations.Add(_arrow1);
                    childEquations.Add(_arrow2);
                    break;
            }
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

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                foreach (EquationBase eb in childEquations)
                {
                    eb.MidX = MidX;
                }
            }
        }

        private void AdjustVertical()
        {
            if (_equationPosition == Position.Top)
            {
                _rowContainer1.Top = Top;
                if (_arrow2 != null)
                {
                    _arrow1.Top = _rowContainer1.Bottom;
                    _arrow2.Bottom = Bottom;
                }
                else
                {
                    _arrow1.Bottom = Bottom;
                }
            }
            else if (_equationPosition == Position.Bottom)
            {
                _arrow1.Top = Top;
                _rowContainer1.Bottom = Bottom;
                if (_arrow2 != null)
                {
                    _arrow2.Top = _arrow1.Bottom + ArrowGap;
                }
            }
            else if (_equationPosition == Position.BottomAndTop)
            {
                _rowContainer1.Top = Top;
                _arrow1.Top = _rowContainer1.Bottom;
                if (_arrow2 != null)
                {
                    _arrow2.Top = _arrow1.Bottom + ArrowGap;
                }
                _rowContainer2.Bottom = Bottom;
            }
        }

        public override double RefY
        {
            get
            {
                if (_equationPosition == Position.Top) //only top container
                {
                    if (_arrow2 == null)
                    {
                        return Height - LineThickness;// -arrow1.Height / 2;
                    }
                    else
                    {
                        return Height - LineThickness * 4;
                    }
                }
                else if (_equationPosition == Position.Bottom) //only bottom container
                {
                    if (_arrow2 == null)
                    {
                        return _arrow1.Height / 2;
                    }
                    else
                    {
                        return _arrow1.Height + ArrowGap / 2;
                    }
                }
                else //both top and bottom containers
                {
                    if (_arrow2 == null)
                    {
                        return _rowContainer1.Height + _arrow1.Height / 2;
                    }
                    else
                    {
                        return _rowContainer1.Height + _arrow1.Height + ArrowGap / 2;
                    }
                }
            }
        }

        protected override void CalculateHeight()
        {
            Height = childEquations.Sum(x => x.Height) + ArrowGap;
        }

        protected override void CalculateWidth()
        {
            if (_arrowType.ToString().ToLower().Contains("small"))
            {
                Width = Math.Max(_rowContainer1.Width, (_rowContainer2 != null ? _rowContainer2.Width : 0)) + FontSize * 3;
            }
            else
            {
                Width = Math.Max(_rowContainer1.Width, (_rowContainer2 != null ? _rowContainer2.Width : 0)) + FontSize * 2;
            }
            switch (_arrowType)
            {
                case ArrowType.LeftArrow:
                case ArrowType.RightArrow:
                case ArrowType.DoubleArrow:
                    _arrow1.Width = Width - FontSize * .3;
                    break;

                case ArrowType.RightLeftArrow:
                case ArrowType.RightLeftHarpoon:
                    _arrow1.Width = Width - FontSize * .3;
                    _arrow2.Width = Width - FontSize * .3;
                    break;

                case ArrowType.RightSmallLeftArrow:
                case ArrowType.RightSmallLeftHarpoon:
                    _arrow1.Width = Width - FontSize * .3;
                    _arrow2.Width = Width - FontSize * 1.5;
                    break;

                case ArrowType.SmallRightLeftArrow:
                case ArrowType.SmallRightLeftHarpoon:
                    _arrow1.Width = Width - FontSize * 1.5;
                    _arrow2.Width = Width - FontSize * .3;
                    break;
            }
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
                if (ActiveChild == _rowContainer1)
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = _rowContainer2;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == VirtualKey.Up)
            {
                if (ActiveChild == _rowContainer2)
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = _rowContainer1;
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
