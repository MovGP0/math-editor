using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Editor
{
    public static class PenManager
    {
        private static readonly Dictionary<double, Pen> BevelPens = new Dictionary<double, Pen>();
        private static readonly Dictionary<double, Pen> MiterPens = new Dictionary<double, Pen>();
        private static readonly Dictionary<double, Pen> RoundPens = new Dictionary<double, Pen>();
        private static readonly Dictionary<double, Pen> WhitePens = new Dictionary<double, Pen>();

        private static readonly object BevelLock = new object();
        private static readonly object MiterLock = new object();
        private static readonly object RoundLock = new object();
        private static readonly object WhiteLock = new object();

        public static Pen GetWhitePen(double thickness)
        {
            return GetPen(WhiteLock, WhitePens, thickness, PenLineJoin.Miter, Brushes.White);
        }

        public static Pen GetPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            switch (lineJoin)
            {
                case PenLineJoin.Bevel:
                    return GetPen(BevelLock, BevelPens, thickness, lineJoin);
                case PenLineJoin.Miter:
                    return GetPen(MiterLock, MiterPens, thickness, lineJoin);
                case PenLineJoin.Round:
                    return GetPen(RoundLock, RoundPens, thickness, lineJoin);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lineJoin), lineJoin, null);
            }
        }

        private static Pen GetPen(object lockObj, IDictionary<double, Pen> penDictionary, double thickness, PenLineJoin lineJoin, Brush brush=null)
        {
            lock (lockObj)
            {
                thickness = Math.Round(thickness, 1);
                if (penDictionary.ContainsKey(thickness))
                    return penDictionary[thickness];

                var pen = new Pen(brush ?? Brushes.Black, thickness)
                {
                    LineJoin = lineJoin
                };
                pen.Freeze();
                penDictionary.Add(thickness, pen);
                return penDictionary[thickness];
            }
        }
    }
}
