using System.Xml.Linq;

namespace Editor
{
    public class Super : SubSuperBase
    {
        RowContainer rowContainer;       

        public Super(EquationRow parent, Position position)
            : base(parent, position)
        {
            ActiveChild = rowContainer = new RowContainer(this);
            childEquations.Add(rowContainer);
            if (SubLevel == 1)
            {
                rowContainer.FontFactor = SubFontFactor;
            }
            else if (SubLevel == 2)
            {
                rowContainer.FontFactor = SubSubFontFactor;
            }            
        }
        
        protected override void CalculateHeight()
        {
           Height = rowContainer.Height + Buddy.RefY - SuperOverlap;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                rowContainer.Top = value;
            }
        }

        protected override void CalculateWidth()
        {
            Width = rowContainer.Width + Padding * 2;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                rowContainer.Left = this.Left + Padding;                
            }
        }

        public override double RefY
        {
            get
            {
                return Height;
            }
        }
    }
}
