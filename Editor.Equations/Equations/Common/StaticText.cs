﻿using System.Windows.Media;
using System.Windows;

namespace Editor
{
    public abstract class StaticText : EquationBase
    {
        protected string Text { get; set; }        
        protected FontType FontType { get; set; }
        protected double FontSizeFactor = 1;
        protected FontWeight FontWeight = FontWeights.Normal;
        protected double TopOffestFactor = 0;
        protected double LeftMarginFactor = 0;
        protected double RightMarginFactor = 0;

        FormattedText formattedText;

        protected StaticText(IEquationContainer parent)
            :base(parent)
        {
            IsStatic = true;
        }

        public override void DrawEquation(DrawingContext dc)
        {
            //dc.DrawText(formattedText, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height));
            formattedText.DrawTextTopLeftAligned(dc, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height));
        }

        public override double FontSize
        {
            get
            {
                return base.FontSize;
            }
            set
            {
                base.FontSize = value;
                ReformatSign();            
            }
        }

        protected void ReformatSign()
        {
            formattedText = FontFactory.GetFormattedText(Text, FontType, FontSize * FontSizeFactor, FontWeight);
            Width = formattedText.GetFullWidth() + LeftMarginFactor * FontSize + RightMarginFactor * FontSize; // * WidthFactor;
            Height = formattedText.Extent;
        }

        public double OverhangTrailing => formattedText.OverhangTrailing;

        public double OverhangLeading => formattedText.OverhangLeading;
    }
}
