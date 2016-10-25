using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Input;
using System.Windows;

namespace Editor
{
    public sealed class MatrixEquation : EquationContainer
    {
        readonly int _columns = 1;
        readonly int _rows = 1;
        double CellSpace => FontSize * .7;

        public override Thickness Margin => new Thickness(FontSize * .15, 0, FontSize * .15, 0);

        public MatrixEquation(IEquationContainer parent, int rows, int columns)
            : base(parent)
        {
            _rows = rows;
            _columns = columns;
            for (var i = 0; i < columns * rows; i++)
            {
                ChildEquations.Add(new RowContainer(this));
            }
            ActiveChild = ChildEquations.First();
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(typeof(int).FullName, _rows));
            parameters.Add(new XElement(typeof(int).FullName, _columns));
            thisElement.Add(parameters);
            foreach (var eb in ChildEquations)
            {
                thisElement.Add(eb.Serialize());
            }
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elements = xElement.Elements(typeof(RowContainer).Name).ToArray();
            for (var i = 0; i < ChildEquations.Count; i++)
            {
                ChildEquations[i].DeSerialize(elements[i]);
            }
            CalculateSize();
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                var rowRefYs = new double[_rows];
                var topOffsets = new double[_rows + 1];

                for (var i = 0; i < _rows; i++)
                {
                    rowRefYs[i] = ChildEquations.Skip(i * _columns).Take(_columns).Max(x => x.RefY);
                    topOffsets[i + 1] = ChildEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height) + topOffsets[i];
                }

                for (var i = 0; i < _rows; i++)
                {
                    for (var j = 0; j < _columns; j++)
                    {
                        ChildEquations[i * _columns + j].MidY = Top + rowRefYs[i] + topOffsets[i] + CellSpace * i;
                    }
                }
            }
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                var columnRefXs = new double[_columns];
                var leftOffsets = new double[_columns + 1];
                for (var i = 0; i < _columns; i++)
                {
                    for (var j = 0; j < _rows; j++)
                    {
                        columnRefXs[i] = Math.Max(ChildEquations[j * _columns + i].RefX, columnRefXs[i]);
                        leftOffsets[i + 1] = Math.Max(ChildEquations[j * _columns + i].Width, leftOffsets[i + 1]);
                    }
                    leftOffsets[i + 1] += leftOffsets[i];
                }
                for (var i = 0; i < _columns; i++)
                {
                    for (var j = 0; j < _rows; j++)
                    {
                        ChildEquations[j * _columns + i].MidX = value + columnRefXs[i] + leftOffsets[i] + CellSpace * i;
                    }
                }
            }
        }

        public override void CalculateWidth()
        {
            var columnWidths = new double[_columns];
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    columnWidths[i] = Math.Max(ChildEquations[j * _columns + i].Width, columnWidths[i]);
                }
            }
            Width = columnWidths.Sum() + CellSpace * (_columns - 1);
        }

        public override void CalculateHeight()
        {
            var rowHeights = new double[_rows];
            for (var i = 0; i < _rows; i++)
            {
                rowHeights[i] = ChildEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
            }
            Height = rowHeights.Sum() + CellSpace * (_rows - 1);
        }

        public override double RefY
        {
            get
            {
                if (_rows == 1)
                {
                    return ChildEquations.Max(x => x.RefY);
                }

                if (_rows % 2 == 0)
                {
                    //return childEquations.Take(rows / 2 * columns).Sum(x => x.Height) - CellSpace / 2 + FontSize * .3;
                    var rowHeights = new double[_rows / 2];
                    for (var i = 0; i < _rows / 2; i++)
                    {
                        rowHeights[i] = ChildEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
                    }
                    return rowHeights.Sum() + CellSpace * _rows/2 - CellSpace/2 + FontSize * .1;
                }
                else
                {
                    //return childEquations.Skip(rows / 2 * columns).Take(columns).Max(x => x.MidY) - Top;
                    var rowHeights = new double[_rows / 2 + 1];
                    for (var i = 0; i < _rows / 2; i++)
                    {
                        rowHeights[i] = ChildEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
                    }
                    rowHeights[_rows / 2] = ChildEquations.Skip(_rows / 2 * _columns).Take(_columns).Max(x => x.RefY);
                    return rowHeights.Sum() + CellSpace * (_rows / 2);// -FontSize * .1;
                }
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            var currentIndex = ChildEquations.IndexOf(ActiveChild);
            if (key == Key.Right)
            {
                if (currentIndex % _columns < _columns - 1)//not last column?
                {
                    ActiveChild = ChildEquations[currentIndex + 1];
                    return true;
                }
            }
            else if (key == Key.Left)
            {
                if (currentIndex % _columns > 0)//not last column?
                {
                    ActiveChild = ChildEquations[currentIndex - 1];
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (currentIndex / _columns > 0)//not in first row?
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[currentIndex - _columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Down)
            {
                if (currentIndex / _columns < _rows - 1)//not in last row?
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[currentIndex + _columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
