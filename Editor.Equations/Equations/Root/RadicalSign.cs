﻿using System.Windows.Media;
using System.Windows;

namespace Editor
{
    public sealed class RadicalSign : EquationBase
    {
        private static readonly double WidthFactor = .9;
        public RadicalSign(IEquationContainer parent)
            : base(parent)
        {
            Width = FontSize * WidthFactor;
            IsStatic = true;
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
                Width = value * WidthFactor;
            }
        }

        public override void DrawEquation(DrawingContext dc)
        {
            dc.DrawPolyline(new Point(Left, Bottom - Height * .4),
                                   new PointCollection 
                                    {   
                                        new Point(Left + FontSize * .2, Bottom - Height * .5), 
                                        new Point(Left + FontSize * .2, Bottom - Height * .5),
                                        new Point(Left + FontSize * .4, Bottom),
                                        new Point(Left + FontSize * .4, Bottom),
                                        new Point(Right - FontSize * .1, Top + FontSize * .1),
                                        new Point(Right - FontSize * .1, Top + FontSize * .1),
                                        new Point(ParentEquation.Right, Top + FontSize * .1),
                                    },
                                   StandardPen);
        }
    }
}
