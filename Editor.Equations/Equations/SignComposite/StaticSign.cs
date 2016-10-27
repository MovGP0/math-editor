namespace Editor
{
    public sealed class StaticSign : StaticText
    {
        public SignCompositeSymbol Symbol { get; set; }

        private bool _integralSignItalic;
        public bool UseItalicIntegralSign 
        { 
            get { return _integralSignItalic; }
            set 
            { 
                _integralSignItalic = value;
                DetermineMargin(Symbol);
                FontType = DetermineFontType(Symbol, UseItalicIntegralSign);
                ReformatSign();
            }
        }

        public StaticSign(IEquationContainer parent, SignCompositeSymbol symbol, bool useItalic)
            : base(parent)
        {
            _integralSignItalic = useItalic;
            Symbol = symbol;
            Text = DetermineSignString(symbol);
            FontType = DetermineFontType(symbol, UseItalicIntegralSign);
            FontSizeFactor = DetermineFontSizeFactor(symbol);
            DetermineMargin(symbol);
            ReformatSign();
        }

        private void DetermineMargin(SignCompositeSymbol symbol)
        {            
            switch (symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                    LeftMarginFactor = 0.02;
                    break;
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    RightMarginFactor = .2;
                    LeftMarginFactor = 0.1;
                    break;
                case SignCompositeSymbol.Union:
                case SignCompositeSymbol.Intersection:
                    LeftMarginFactor = 0.1;
                    RightMarginFactor = 0.05;
                    break;
                default:
                    RightMarginFactor = 0.05;
                    break;
            }            
        }

        private static FontType DetermineFontType(SignCompositeSymbol symbol, bool useItalicIntegralSign)
        {
            switch (symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:                
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    return useItalicIntegralSign ? FontType.StixGeneral : FontType.StixIntegralsUp;

                case SignCompositeSymbol.Intersection:
                case SignCompositeSymbol.Union:
                    return FontType.StixGeneral;

                default:
                    return FontType.StixSizeOneSym;
            }
        }

        private static double DetermineFontSizeFactor(SignCompositeSymbol symbol)
        {
            switch (symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    return 1.5;
                case SignCompositeSymbol.Intersection:
                case SignCompositeSymbol.Union:
                    return 1.2;
                default:
                    return 1d;
            }
        }

        private static string DetermineSignString(SignCompositeSymbol symbol)
        {
            switch (symbol)
            {
                case SignCompositeSymbol.Sum:
                    return "\u2211";
                    
                case SignCompositeSymbol.Product:
                    return "\u220F";
                    
                case SignCompositeSymbol.CoProduct:
                    return "\u2210";
                    
                case SignCompositeSymbol.Intersection:
                    return "\u22C2";
                    
                case SignCompositeSymbol.Union:
                    return "\u22C3";
                    
                case SignCompositeSymbol.Integral:
                    return "\u222B";
                    
                case SignCompositeSymbol.DoubleIntegral:
                    return "\u222C";
                    
                case SignCompositeSymbol.TripleIntegral:
                    return "\u222D";
                    
                case SignCompositeSymbol.ContourIntegral:
                    return "\u222E";
                    
                case SignCompositeSymbol.SurfaceIntegral:
                    return "\u222F";
                    
                case SignCompositeSymbol.VolumeIntegral:
                    return "\u2230";
                    
                case SignCompositeSymbol.ClockContourIntegral:
                    return "\u2232";
                    
                case SignCompositeSymbol.AntiClockContourIntegral:
                    return "\u2233";
                    
                default: 
                    return string.Empty;
            }
        }
    }
}
