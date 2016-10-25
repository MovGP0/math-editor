namespace Editor
{
    public class DivRegularSmall : DivRegular
    {
        public DivRegularSmall(IEquationContainer parent)
            : base(parent, true)
        {            
        }
    }

    public class DivDoubleBar : DivRegular
    {
        public DivDoubleBar(IEquationContainer parent)
            : base(parent)
        {
            barCount = 2;
        }
    }

    public class DivTripleBar : DivRegular
    {
        public DivTripleBar(IEquationContainer parent)
            : base(parent)
        {
            barCount = 3;
        }
    }

    public class DivSlantedSmall : DivSlanted
    {
        public DivSlantedSmall(IEquationContainer parent)
            : base(parent, true)
        {            
        }
    }

    public class DivHorizSmall : DivHorizontal
    {
        public DivHorizSmall(IEquationContainer parent)
            : base(parent, true)
        {           
        }
    }   

}
