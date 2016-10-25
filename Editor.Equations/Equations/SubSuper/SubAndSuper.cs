using System;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Input;

namespace Editor
{
    public class SubAndSuper : SubSuperBase
    {
        readonly IRowContainer _superEquation;
        readonly IRowContainer _subEquation;

        public SubAndSuper(IEquationRow parent, Position position)
            : base(parent, position)
        {
            ActiveChild = _superEquation = new RowContainer(this);
            _subEquation = new RowContainer(this);
            ChildEquations.Add(_superEquation);
            ChildEquations.Add(_subEquation);
            if (SubLevel == 1)
            {
                _superEquation.FontFactor = SubFontFactor;
                _subEquation.FontFactor = SubFontFactor;
            }
            else if (SubLevel >= 2)
            {
                _superEquation.FontFactor = SubSubFontFactor;
                _subEquation.FontFactor = SubSubFontFactor;
            }
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(Position.GetType().Name, Position));
            thisElement.Add(parameters);
            thisElement.Add(_superEquation.Serialize());
            thisElement.Add(_subEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elements = xElement.Elements(typeof(RowContainer).Name).ToArray();
            _superEquation.DeSerialize(elements[0]);
            _subEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override void CalculateWidth()
        {
            Width = Math.Max(_subEquation.Width, _superEquation.Width) + Padding * 2;
        }

        public override void CalculateHeight()
        {
            if (Buddy.GetType() == typeof(TextEquation))
            {
                double superHeight = _superEquation.Height + Buddy.RefY - SuperOverlap; ;
                double subHeight = _subEquation.Height - SubOverlap;
                Height = subHeight + superHeight;
            }
            else
            {
                Height = Buddy.Height - SuperOverlap - SubOverlap * 2 + _subEquation.Height + _superEquation.Height;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                _superEquation.Top = value;
                _subEquation.Bottom = Bottom;
            }
        }

        public override double RefY => _superEquation.Height + Buddy.RefY - SuperOverlap;

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }

            switch (key)
            {
                case Key.Down:
                    if (ActiveChild == _superEquation)
                    {
                        ActiveChild = _subEquation;
                        return true;
                    }
                    break;

                case Key.Up:
                    if (ActiveChild == _subEquation)
                    {
                        ActiveChild = _superEquation;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                if (Position == Position.Right)
                {
                    _subEquation.Left = Left + Padding;
                    _superEquation.Left = Left + Padding;
                }
                else
                {
                    _subEquation.Right = Right - Padding;
                    _superEquation.Right = Right - Padding;
                }
            }
        }
    }
}
