using System;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Input;
using Windows.System;

namespace Editor
{
    public class SubAndSuper : SubSuperBase
    {   
        RowContainer superEquation;
        RowContainer subEquation;

        public SubAndSuper(EquationRow parent, Position position)
            : base(parent, position)
        {
            ActiveChild = superEquation = new RowContainer(this);
            subEquation = new RowContainer(this);
            childEquations.Add(superEquation);
            childEquations.Add(subEquation);
            if (SubLevel == 1)
            {
                superEquation.FontFactor = SubFontFactor;
                subEquation.FontFactor = SubFontFactor;
            }
            else if (SubLevel >= 2)
            {
                superEquation.FontFactor = SubSubFontFactor;
                subEquation.FontFactor = SubSubFontFactor;
            }
        }
        
        protected override void CalculateWidth()
        {
            Width = Math.Max(subEquation.Width, superEquation.Width) + Padding * 2;
        }

        protected override void CalculateHeight()
        {
            if (Buddy.GetType() == typeof(TextEquation))
            {
                double superHeight = superEquation.Height + Buddy.RefY - SuperOverlap; ;
                double subHeight = subEquation.Height - SubOverlap;
                Height = subHeight + superHeight;
            }
            else
            {
                Height = Buddy.Height - SuperOverlap - SubOverlap * 2 + subEquation.Height + superEquation.Height;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                superEquation.Top = value;
                subEquation.Bottom = this.Bottom;
            }
        }

        public override double RefY
        {
            get
            {
                return superEquation.Height + Buddy.RefY - SuperOverlap;
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
                if (ActiveChild == superEquation)
                {
                    ActiveChild = subEquation;
                    return true;
                }
            }
            else if (key == VirtualKey.Up)
            {
                if (ActiveChild == subEquation)
                {
                    ActiveChild = superEquation;
                    return true;
                }
            }
            return false;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                if (Position == Editor.Position.Right)
                {
                    subEquation.Left = this.Left + Padding;
                    superEquation.Left = this.Left + Padding;
                }
                else
                {
                    subEquation.Right = this.Right - Padding;
                    superEquation.Right = this.Right - Padding;
                }
            }
        }
    }
}
