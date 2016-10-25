namespace Editor
{
    public abstract class SubSuperBase : EquationContainer
    {
        public Position Position { get; set; }                
        protected double Padding => buddy != null && buddy.GetType() == typeof(TextEquation) 
            ? FontSize*.01 
            : FontSize*.05;

        protected double SuperOverlap => FontSize * 0.35;

        protected double SubOverlap 
        { 
            get 
            {
                var te = buddy as TextEquation;
                double oha = 0;
                if (te != null)
                {
                    oha = te.GetCornerDescent(Position);
                }
                return FontSize * .1 - oha; 
            } 
        }
        
        IEquationBase buddy;
        protected IEquationBase Buddy
        {
            get { return buddy ?? ParentEquation.ActiveChild; }
            set { buddy = value; }
        }

        protected SubSuperBase(IEquationContainer parent, Position position)
            : base(parent)
        {
            ApplySymbolGap = false;
            SubLevel++;
            Position = position;            
        }

        public void SetBuddy(IEquationBase buddy)
        {
            Buddy = buddy;
            CalculateHeight();
        }
    }
}
