using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Input;

namespace Editor
{
    public sealed class NRoot : EquationContainer
    {
        private readonly RowContainer _insideEquation;
        private readonly RowContainer _nthRootEquation;
        private readonly RadicalSign _radicalSign;
        private double ExtraHeight => FontSize * .15;
        private double HGap => FontSize * .5;
        private double LeftPadding => FontSize * .1;

        public NRoot(IEquationContainer parent)
            : base(parent)
        {
            _radicalSign = new RadicalSign(this);
            ActiveChild = _insideEquation = new RowContainer(this);
            _nthRootEquation = new RowContainer(this)
            {
                ApplySymbolGap = false,
                FontFactor = SubFontFactor
            };
            ChildEquations.AddRange(new EquationBase[] { _insideEquation, _radicalSign, _nthRootEquation });
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_insideEquation.Serialize());
            thisElement.Add(_nthRootEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEquation.DeSerialize(xElement.Elements().First());
            _nthRootEquation.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override bool ConsumeMouseClick(System.Windows.Point mousePoint)
        {
            if (_nthRootEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _nthRootEquation;
            }
            else if (_insideEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _insideEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
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

        private void AdjustVertical()
        {
            _insideEquation.Bottom = Bottom;
            _radicalSign.Bottom = Bottom;
            _nthRootEquation.Bottom = _radicalSign.MidY - FontSize * .05;
        }

        public override void CalculateWidth()
        {
            Width = Math.Max(_nthRootEquation.Width + HGap, _radicalSign.Width) + _insideEquation.Width + LeftPadding;            
        }

        public override void CalculateHeight()
        {
            Height = _insideEquation.Height + Math.Max(0, _nthRootEquation.Height - _insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;
        }

        public override double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                _radicalSign.Height = _insideEquation.Height + ExtraHeight;
                AdjustVertical();
            }
        }

        public override double RefY => _insideEquation.RefY + Math.Max(0, _nthRootEquation.Height - _insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;                
                if (_nthRootEquation.Width + HGap > _radicalSign.Width)
                {
                    _nthRootEquation.Left = Left + LeftPadding;
                    _radicalSign.Right = _nthRootEquation.Right + HGap;
                }
                else
                {
                    _radicalSign.Left = Left + LeftPadding;
                    _nthRootEquation.Right = _radicalSign.Right - HGap;
                }
                _insideEquation.Left = _radicalSign.Right;
            }
        }
        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }

            switch (key)
            {
                case Key.Left:
                    if (ActiveChild != _insideEquation)
                        return false;

                    ActiveChild = _nthRootEquation;
                    return true;

                case Key.Right:
                    if (ActiveChild != _nthRootEquation)
                        return false;

                    ActiveChild = _insideEquation;
                    return true;
            }
            return false;
        }
    }
}
