using System.Windows.Media;
using System.Windows;

namespace Editor
{
    public sealed class HorizontalBracketSign : EquationBase
    {
        public HorizontalBracketSignType SignType { get; private set; }
        FormattedText sign = null;
        FormattedText leftCurlyPart;
        FormattedText rightCurlyPart;

        public HorizontalBracketSign(IEquationContainer parent, HorizontalBracketSignType signType)
            : base(parent)
        {
            this.SignType = signType;
            IsStatic = true;
        }

        public override void DrawEquation(DrawingContext dc)
        {   
            switch (SignType)
            {
                case HorizontalBracketSignType.TopCurly:
                    DrawTopCurly(dc);
                    break;
                case HorizontalBracketSignType.BottomCurly:
                    DrawBottomCurly(dc);
                    break;
                case HorizontalBracketSignType.ToSquare:
                    DrawTopSquare(dc);
                    break;
                case HorizontalBracketSignType.BottomSquare:
                    DrawBottomSquare(dc);
                    break;
            }
        }

        void DrawTopSquare(DrawingContext dc)
        {
            var points = new PointCollection
            {
                new Point(Left, Top), 
                new Point(Right, Top),
                new Point(Right, Bottom),
                new Point(Right - ThinLineThickness, Bottom),
                new Point(Right - ThinLineThickness, Top + LineThickness),
                new Point(Left + ThinLineThickness, Top + LineThickness),
                new Point(Left + ThinLineThickness, Bottom)
            };
            dc.FillPolylineGeometry(new Point(Left, Bottom), points);
        }

        void DrawBottomSquare(DrawingContext dc)
        {
            var points = new PointCollection
            {
                new Point(Left, Top), 
                new Point(Left + ThinLineThickness, Top),
                new Point(Left + ThinLineThickness, Bottom - LineThickness),
                new Point(Right - ThinLineThickness, Bottom - LineThickness),
                new Point(Right - ThinLineThickness, Top),
                new Point(Right, Top),
                new Point(Right, Bottom)
            };
            dc.FillPolylineGeometry(new Point(Left, Bottom), points);
        }

        void DrawBottomCurly(DrawingContext dc)
        {
            if (Width < FontSize * 5)
            {
                sign.DrawTextTopLeftAligned(dc, Location);
            }
            else
            {
                var line = FontFactory.GetFormattedText("\uE14B", FontType.StixNonUnicode, FontSize * .55);
                leftCurlyPart.DrawTextTopLeftAligned(dc, Location);
                sign.DrawTextTopLeftAligned(dc, new Point(MidX - sign.GetFullWidth() * .5, Top + leftCurlyPart.Extent - line.Extent));
                rightCurlyPart.DrawTextTopLeftAligned(dc, new Point(Right - rightCurlyPart.GetFullWidth(), Top));
                var left = Left + leftCurlyPart.GetFullWidth() * .85;
                var right = MidX - sign.GetFullWidth() * .4;
                while (left < right)
                {
                    line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent));
                    left += line.GetFullWidth() * .8;
                    var shoot = (left + line.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent));
                        break;
                    }
                }
                left = MidX + sign.GetFullWidth() * .4;
                right = Right - rightCurlyPart.GetFullWidth() * .8;
                while (left < right)
                {
                    line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent));
                    left += line.GetFullWidth() * .8;
                    var shoot = (left + line.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent));
                        break;
                    }
                }

                //dc.DrawLine(StandardPen, new Point(Left + leftCurlyPart.GetFullWidth() * .96, Top + leftCurlyPart.Extent - LineThickness * .5),
                //                         new Point(MidX - sign.GetFullWidth() * .48, Top + leftCurlyPart.Extent - LineThickness * .5));
                //dc.DrawLine(StandardPen, new Point(MidX + sign.GetFullWidth() * .48, Top + rightCurlyPart.Extent - LineThickness * .5),
                //                         new Point(Right - rightCurlyPart.GetFullWidth() * .96, Top + rightCurlyPart.Extent - LineThickness * .5));
            }
        }

        void DrawTopCurly(DrawingContext dc)
        {
            if (Width < FontSize * 5)
            {
                sign.DrawTextTopLeftAligned(dc, Location);
            }
            else
            {
                var extension = FontFactory.GetFormattedText("\uE14A", FontType.StixNonUnicode, FontSize * .55);
                //dc.DrawLine(new Pen(Brushes.Red, 1), Location, new Point(Right, Top));
                leftCurlyPart.DrawTextTopLeftAligned(dc, new Point(Left, Top + sign.Extent - extension.Extent));
                sign.DrawTextTopLeftAligned(dc, new Point(MidX - sign.GetFullWidth() * .5, Top));
                rightCurlyPart.DrawTextTopLeftAligned(dc, new Point(Right - rightCurlyPart.GetFullWidth(), Top + sign.Extent - extension.Extent));
                var left = Left + leftCurlyPart.GetFullWidth() * .9;
                var right = MidX - sign.GetFullWidth() * .4;
                //var geometry = extension.BuildGeometry(new Point(0, Top + sign.Extent - extension.Height - extension.OverhangAfter));
                //PointCollection points = new PointCollection { new Point(right, geometry.Bounds.Top),
                //                                                   new Point(right, geometry.Bounds.Bottom),
                //                                                   new Point(left, geometry.Bounds.Bottom),
                //                                                 };
                //dc.FillPolylineGeometry(new Point(left, geometry.Bounds.Top), points);                
                while (left < right)
                {
                    extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent));
                    left += extension.GetFullWidth() * .8;
                    var shoot = (left + extension.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent));
                        break;
                    }
                }
                left = MidX + sign.GetFullWidth() * .4;
                right = Right - rightCurlyPart.GetFullWidth() * .8;
                //points = new PointCollection { new Point(right, geometry.Bounds.Top),
                //                                                   new Point(right, geometry.Bounds.Bottom),
                //                                                   new Point(left, geometry.Bounds.Bottom),
                //                                                 };
                //dc.FillPolylineGeometry(new Point(left, geometry.Bounds.Top), points);
                while (left < right)
                {
                    extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent));
                    left += extension.GetFullWidth() * .8;
                    var shoot = (left + extension.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent));
                        break;
                    }
                }
            }
        }

        public override double Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                AdjustHeight();
            }
        }

        private void AdjustHeight()
        {
            if (SignType == HorizontalBracketSignType.BottomSquare || SignType == HorizontalBracketSignType.ToSquare)
            {
                Height = FontSize * .3;
            }
            else if (Width < FontSize * 5)
            {
                CreateSingleCharacterCurlySign();
            }
            else
            {
                if (SignType == HorizontalBracketSignType.BottomCurly)
                {
                    CreateBrokenCurlyBottom();
                }
                else
                {
                    CreateBrokenCurlyTop();
                }
                Height = FontSize * .5;
            }
        }

        private void CreateBrokenCurlyTop()
        {
            var fontSize = FontSize * .55;
            leftCurlyPart = FontFactory.GetFormattedText("\uE13B", FontType.StixNonUnicode, fontSize); //Top left of overbrace 
            sign = FontFactory.GetFormattedText("\uE140", FontType.StixNonUnicode, fontSize); //middle of overbrace
            rightCurlyPart = FontFactory.GetFormattedText("\uE13C", FontType.StixNonUnicode, fontSize); //Top right of overbrace             
        }

        private void CreateBrokenCurlyBottom()
        {
            var fontSize = FontSize * .55;
            leftCurlyPart = FontFactory.GetFormattedText("\uE13D", FontType.StixNonUnicode, fontSize); //Top left of overbrace 
            sign = FontFactory.GetFormattedText("\uE141", FontType.StixNonUnicode, fontSize); //middle of overbrace
            rightCurlyPart = FontFactory.GetFormattedText("\uE13E", FontType.StixNonUnicode, fontSize); //Top right of overbrace
        }

        private void CreateSingleCharacterCurlySign()
        {
            var signStr = SignType == HorizontalBracketSignType.TopCurly ? "\u23DE" : "\u23DF";
            FontType fontType;
            if (Width < FontSize)
            {
                fontType = FontType.StixSizeOneSym;
            }
            else if (Width < FontSize * 2)
            {
                fontType = FontType.StixSizeTwoSym;
            }
            else if (Width < FontSize * 3)
            {
                fontType = FontType.StixSizeThreeSym;
            }
            else if (Width < FontSize * 4)
            {
                fontType = FontType.StixSizeFourSym;
            }
            else
            {
                fontType = FontType.StixSizeFiveSym;
            }
            double fontSize = 4;
            do
            {
                sign = FontFactory.GetFormattedText(signStr, fontType, fontSize++);
            }
            while (sign.Width < Width);
            Height = sign.Extent * 1.1;
        }
    }
}
