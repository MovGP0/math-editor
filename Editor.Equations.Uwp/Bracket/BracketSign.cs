using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Editor
{
    public class BracketSign : EquationBase
    {
        public BracketSignType SignType { get; set; }
        TextBlock signText; //used by certain brackets
        TextBlock signText2; //used by certain brackets
        TextBlock midText; //for bigger curly brackets
        TextBlock extension; //for bigger curly brackets       

        private double BracketBreakLimit => FontSize * 2.8;

        private readonly double _leftPaddingFactor = 0.02;
        private readonly double _rightPaddingFactor = 0.02;
        private double SignLeft => Left + LeftPadding;
        private double SignRight => Right - RightPadding;
        private double LeftPadding => FontSize * _leftPaddingFactor;
        private double RightPadding => FontSize * _rightPaddingFactor;

        public BracketSign(EquationContainer parent, BracketSignType entityType)
            : base(parent)
        {
            SignType = entityType;
            IsStatic = true;
            if (new[] {BracketSignType.LeftRound, BracketSignType.LeftCurly, BracketSignType.LeftAngle, 
                        BracketSignType.LeftCeiling, BracketSignType.LeftFloor, BracketSignType.LeftSquare,
                        BracketSignType.LeftSquareBar}.Contains(entityType))
            {
                _leftPaddingFactor = 0.02;
                _rightPaddingFactor = 0;
            }
            else if (entityType == BracketSignType.LeftBar || entityType == BracketSignType.LeftDoubleBar ||
                     entityType == BracketSignType.RightBar || entityType == BracketSignType.RightDoubleBar)
            {
                _leftPaddingFactor = 0.06;
                _rightPaddingFactor = 0.06;
            }
        }

        private void CreateTextBrackets()
        {
            switch (SignType)
            {
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    CreateRoundTextBracket();
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    CreateCurlyTextBracket();
                    break;
            }
        }

        private void CreateRoundTextBracket()
        {
            if (Height < FontSize * 1.2)
            {
                string signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.StixGeneral, signText);
            }
            if (Height < FontSize * 1.5)
            {
                string signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.StixSizeOneSym, signText);
            }
            else if (Height < FontSize * 1.9)
            {
                string signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.StixSizeTwoSym, signText);
            }
            else if (Height < FontSize * 2.5)
            {
                string signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.StixSizeThreeSym, signText);
            }
            else if (Height < BracketBreakLimit)
            {
                string signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.StixSizeFourSym, signText);
            }
            else
            {
                string text1 = SignType == BracketSignType.LeftRound ? "\u239b" : "\u239e";
                string text2 = SignType == BracketSignType.LeftRound ? "\u239d" : "\u23a0";
                string ext = SignType == BracketSignType.LeftRound ? "\u239c" : "\u239f";
                signText = FontFactory.GetFormattedText(text1, FontType.StixSizeOneSym, FontSize * .5);
                signText2 = FontFactory.GetFormattedText(text2, FontType.StixSizeOneSym, FontSize * .5);
                extension = FontFactory.GetFormattedText(ext, FontType.StixSizeOneSym, FontSize * .5);
            }
        }

        private void CreateCurlyTextBracket()
        {
            if (Height < FontSize * 1.5)
            {
                string signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.StixSizeOneSym, signText);
            }
            else if (Height < FontSize * 1.9)
            {
                string signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.StixSizeTwoSym, signText);
            }
            else if (Height < FontSize * 2.5)
            {
                string signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.StixSizeThreeSym, signText);
            }
            else if (Height < BracketBreakLimit)
            {
                string signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.StixSizeFourSym, signText);
            }
            else
            {
                var text1 = SignType == BracketSignType.LeftCurly ? "\u23a7" : "\u23ab";
                var midtex = SignType == BracketSignType.LeftCurly ? "\u23a8" : "\u23ac";
                var text2 = SignType == BracketSignType.LeftCurly ? "\u23a9" : "\u23ad";
                signText = FontFactory.GetFormattedText(text1, FontType.StixSizeOneSym, FontSize * .5);
                midText = FontFactory.GetFormattedText(midtex, FontType.StixSizeOneSym, FontSize * .5);
                extension = FontFactory.GetFormattedText("\u23AA", FontType.StixSizeOneSym, FontSize * .5);
                signText2 = FontFactory.GetFormattedText(text2, FontType.StixSizeOneSym, FontSize * .5);
            }
        }

        private void FitSignToHeight(FontType fontType, string unicodeCharText)
        {
            var factor = .4;
            do
            {
                signText = FontFactory.GetFormattedText(unicodeCharText, fontType, FontSize * factor);
                factor += .02;
            }
            while (Height > signText.Height);
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
                if (SignType == BracketSignType.LeftRound || SignType == BracketSignType.RightRound ||
                    SignType == BracketSignType.LeftCurly || SignType == BracketSignType.RightCurly
                    )
                {
                    CreateTextBrackets();
                }
                DetermineWidth();
            }
        }

        private void DetermineWidth()
        {
            double width = FontSize * .2;
            switch (SignType)
            {
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    width = signText.Width;
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    if (Height < BracketBreakLimit)
                    {
                        width = signText.Width;
                    }
                    else
                    {
                        width = FontSize * .3;
                    }
                    break;
                case BracketSignType.LeftBar:
                case BracketSignType.RightBar:
                    width = ThinLineThickness + FontSize * 0.05;
                    break;
                case BracketSignType.LeftDoubleBar:
                case BracketSignType.RightDoubleBar:
                    width = ThinLineThickness * 2 + FontSize * 0.05;
                    break;
                case BracketSignType.LeftAngle:
                case BracketSignType.RightAngle:
                    width = FontSize * .12 + Height * 0.1;
                    break;
                case BracketSignType.LeftSquareBar:
                case BracketSignType.RightSquareBar:
                    width = LineThickness * 2 + FontSize * 0.15;
                    break;
            }
            Width = width + LeftPadding + RightPadding;            
        }

        public override void DrawEquation(dynamic dc)
        {
            //dc.DrawRectangle(Brushes.Yellow, null, Bounds);
            switch (SignType)
            {
                case BracketSignType.LeftAngle:
                    PaintLeftAngle(dc);
                    break;
                case BracketSignType.RightAngle:
                    PaintRightAngle(dc);
                    break;
                case BracketSignType.LeftBar:
                    dc.DrawLine(ThinPen, new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    break;
                case BracketSignType.RightBar:
                    dc.DrawLine(ThinPen, new Point(SignRight, Top), new Point(SignRight, Bottom));
                    //PaintVerticalBar(dc);
                    break;
                case BracketSignType.LeftCeiling:
                    PaintLeftCeiling(dc);
                    break;
                case BracketSignType.RightCeiling:
                    PaintRightCeiling(dc);
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    PaintCurly(dc);
                    break;
                case BracketSignType.LeftDoubleBar:
                case BracketSignType.RightDoubleBar:
                    dc.DrawLine(ThinPen, new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    dc.DrawLine(ThinPen, new Point(SignRight, Top), new Point(SignRight, Bottom));
                    break;
                case BracketSignType.LeftFloor:
                    PaintLeftFloor(dc);
                    break;
                case BracketSignType.RightFloor:
                    PaintRightFloor(dc);
                    break;
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    PaintRound(dc);
                    break;
                case BracketSignType.LeftSquare:
                    PaintLeftSquare(dc);
                    break;
                case BracketSignType.RightSquare:
                    PaintRightSquare(dc);
                    break;
                case BracketSignType.LeftSquareBar:
                    PaintLeftSquareBar(dc);
                    break;
                case BracketSignType.RightSquareBar:
                    PaintRightSquareBar(dc);
                    break;
            }
        }

        private void PaintVerticalBar(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Bottom),                                                            
                                                            new Point(SignLeft, Bottom),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintLeftCeiling(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Top + ThinLineThickness),
                                                            new Point(SignLeft + ThinLineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft + ThinLineThickness, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintRightCeiling(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignRight - ThinLineThickness, Bottom),
                                                            new Point(SignRight - ThinLineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft, Top + ThinLineThickness),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintLeftFloor(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignLeft + ThinLineThickness, Top),
                                                            new Point(SignLeft + ThinLineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintRightFloor(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                            new Point(SignLeft, Bottom - ThinLineThickness),
                                                            new Point(SignRight - ThinLineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight - ThinLineThickness, Top),
                                                         };
            dc.FillPolylineGeometry(new Point(SignRight, Top), points);
        }

        private void PaintLeftSquareBar(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Top + ThinLineThickness),
                                                            new Point(SignLeft + ThinLineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft + ThinLineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
            dc.DrawLine(ThinPen, new Point(SignLeft + FontSize * .12, Top + ThinLineThickness * .5), new Point(SignLeft + FontSize * .12, Bottom - ThinLineThickness * .5));
        }

        private void PaintRightSquareBar(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                            new Point(SignLeft, Bottom - ThinLineThickness),
                                                            new Point(SignRight - ThinLineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight - ThinLineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft, Top + ThinLineThickness),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
            dc.DrawLine(ThinPen, new Point(SignRight - FontSize * .12, Top + ThinLineThickness * .5), new Point(SignRight - FontSize * .12, Bottom - ThinLineThickness * .5));
        }

        private void PaintLeftSquare(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Top + ThinLineThickness),
                                                            new Point(SignLeft + LineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft + LineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom - ThinLineThickness),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintRightSquare(dynamic dc)
        {
            PointCollection points = new PointCollection {  
                                                            new Point(SignRight, Top),
                                                            new Point(SignRight, Bottom),
                                                            new Point(SignLeft, Bottom),
                                                            new Point(SignLeft, Bottom - ThinLineThickness),
                                                            new Point(SignRight - LineThickness, Bottom - ThinLineThickness),
                                                            new Point(SignRight - LineThickness, Top + ThinLineThickness),
                                                            new Point(SignLeft, Top + ThinLineThickness),
                                                         };
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points);
        }

        private void PaintRound(dynamic dc)
        {
            if (Height < BracketBreakLimit)
            {
                if (SignType == BracketSignType.LeftRound)
                {
                    signText.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top));
                }
                else
                {
                    signText.DrawTextTopRightAligned(dc, new Point(SignRight, Top));
                }
            }
            else
            {
                if (SignType == BracketSignType.LeftRound)
                {
                    double left = Math.Floor(SignLeft);
                    signText.DrawTextTopLeftAligned(dc, new Point(left, Top));
                    signText2.DrawTextBottomLeftAligned(dc, new Point(left, Bottom));
                    double top = Top + signText.Extent * .9;
                    double bottom = Bottom - signText2.Extent * .9;
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    double padding = extension.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    PointCollection points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);

                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(SignLeft + pen.Thickness * .68, top),
                    //                 new Point(SignLeft + pen.Thickness * .68, bottom));

                    ////double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    ////double padding = extension.OverhangLeading;
                    ////var geometry = extension.BuildGeometry(new Point(SignLeft - padding, top - topExtra));
                    ////Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    ////dc.DrawLine(pen, new Point(SignLeft + pen.Thickness - padding * 1.2, top), 
                    ////                 new Point(SignLeft + LineThickness - padding * 1.2, bottom));
                    //var geometry2 = extension.BuildGeometry(new Point(SignLeft - 10, (top - topExtra)/2));
                    //var geometry3 = extension.BuildGeometry(new Point(SignLeft - 20, 10));
                    //double factor = (bottom - top) / (extension.Extent);
                    //ScaleTransform scale = new ScaleTransform(1.0, factor);
                    //scale.CenterY = extension.Extent / 2;
                    //scale.CenterY = geometry2.Bounds.Height;
                    //geometry2.Transform = scale;
                    //ScaleTransform scale2 = new ScaleTransform(1.0, 3);
                    //geometry3.Transform = scale2;                    
                    //dc.DrawGeometry(Brushes.Red, null, geometry);
                    //dc.DrawGeometry(Brushes.Blue, null, geometry2);                    
                    //dc.DrawGeometry(Brushes.Green, null, geometry3);
                    //var geo = Geometry.Combine(geometry, geometry, GeometryCombineMode.Intersect, scale);
                    //dc.DrawGeometry(Brushes.HotPink, null, geo);
                    //dc.PushTransform(scale);
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    //double padding = extension.OverhangLeading;
                    //dc.DrawText(extension, new Point(SignLeft - padding, top - (topExtra/factor)));
                    //dc.Pop();

                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(SignLeft, top));
                    //    top += extension.Extent *.85;
                    //    double shoot = (top + extension.Extent) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(SignLeft, top));
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    signText.DrawTextTopRightAligned(dc, new Point(SignRight, Top));
                    signText2.DrawTextBottomRightAligned(dc, new Point(SignRight, Bottom));
                    double top = Top + signText.Extent * .9;
                    double bottom = Bottom - signText2.Extent * .9;
                    var geometry = extension.BuildGeometry(new Point(SignRight - extension.GetFullWidth() - extension.OverhangLeading, 0));

                    PointCollection points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    ////double padding = extension.OverhangLeading;
                    ////var geometry = extension.BuildGeometry(new Point(SignLeft, top));
                    ////Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    ////dc.DrawLine(pen, new Point(SignRight - pen.Thickness * .65, top),
                    ////                 new Point(SignRight - pen.Thickness * .65, bottom));
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(SignRight - pen.Thickness * .68, top),
                    //                 new Point(SignRight - pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopRightAligned(dc, new Point(left + signText.GetFullWidth(), top));
                    //    top += extension.Extent * .85;
                    //    double shoot = (top + extension.Extent) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopRightAligned(dc, new Point(left + signText.GetFullWidth(), top));
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void PaintCurly(dynamic dc)
        {
            if (Height < BracketBreakLimit)
            {
                signText.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top));
            }
            else
            {
                if (SignType == BracketSignType.LeftCurly)
                {
                    double left = SignLeft + midText.GetFullWidth() - extension.GetFullWidth();
                    signText.DrawTextTopLeftAligned(dc, new Point(left, Top));
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(left, Top), new Point(left, Bottom));
                    midText.DrawTextTopLeftAligned(dc, new Point(SignLeft, MidY - midText.Extent / 2));
                    signText2.DrawTextBottomLeftAligned(dc, new Point(left, Bottom));
                    double top = Top + signText.Extent * .9;
                    double bottom = MidY - midText.Extent * .4;


                    double padding = extension.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    PointCollection points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);

                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, top), new Point(Right, top));
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, bottom), new Point(Right, bottom));
                    //double padding = extension.OverhangLeading;
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    //var geometry = extension.BuildGeometry(new Point(SignLeft + midText.GetFullWidth(), top));
                    //double factor = ((bottom - top) / (extension.Extent)) * .95;
                    //ScaleTransform transform = new ScaleTransform(1, factor);
                    //transform.CenterY = extension.Extent / 2;
                    //geometry.Transform = transform;
                    //dc.DrawGeometry(Brushes.Red, null, geometry);
                    //dc.PushTransform(transform);
                    //Pen pen = new Pen(Brushes.Black, extension.GetFullWidth());
                    //dc.DrawText(extension, new Point(left - padding, (top/factor) - topExtra));
                    //dc.Pop();
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .65, top), new Point(left + pen.Thickness * .65, bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .8) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                    top = MidY + midText.Extent * .4;
                    bottom = Bottom - signText2.Extent * .9;

                    points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    double left = SignLeft + signText.GetFullWidth() - extension.GetFullWidth();
                    signText.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top));
                    midText.DrawTextTopLeftAligned(dc, new Point(left, MidY - midText.Extent / 2));
                    signText2.DrawTextBottomLeftAligned(dc, new Point(SignLeft, Bottom));
                    double top = Top + signText.Extent * .9;
                    double bottom = MidY - midText.Extent * .4;

                    double padding = extension.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    PointCollection points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);

                    //double padding = extension.OverhangLeading;
                    //var geometry = extension.BuildGeometry(new Point(SignLeft, top));
                    //Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                    top = MidY + midText.Extent * .4;
                    bottom = Bottom - signText2.Extent * .9;
                    points = new PointCollection { new Point(geometry.Bounds.Right, top),
                                                                   new Point(geometry.Bounds.Right, bottom),
                                                                   new Point(geometry.Bounds.Left, bottom),
                                                                 };
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));

                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void PaintLeftAngle(dynamic dc)
        {
            PointCollection points = new PointCollection { new Point(SignLeft, MidY), new Point(SignRight, Bottom) };
            dc.DrawPolyline(new Point(SignRight, Top), points, ThinPen);
        }

        private void PaintRightAngle(dynamic dc)
        {
            PointCollection points = new PointCollection { new Point(SignRight, MidY), new Point(SignLeft, Bottom) };
            dc.DrawPolyline(new Point(SignLeft, Top), points, ThinPen);
        }
    }
}
