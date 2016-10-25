﻿using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Input;
using System.Windows;

namespace Editor
{
    public sealed class MatrixEquation : EquationContainer
    {
        int columns = 1;
        int rows = 1;
        double CellSpace { get { return FontSize * .7; } }

        public override Thickness Margin 
        {
            get { return new Thickness(FontSize * .15, 0, FontSize * .15, 0); }
        }

        public MatrixEquation(IEquationContainer parent, int rows, int columns)
            : base(parent)
        {
            this.rows = rows;
            this.columns = columns;
            for (int i = 0; i < columns * rows; i++)
            {
                ChildEquations.Add(new RowContainer(this));
            }
            ActiveChild = ChildEquations.First();
        }

        public override XElement Serialize()
        {
            XElement thisElement = new XElement(GetType().Name);
            XElement parameters = new XElement("parameters");
            parameters.Add(new XElement(typeof(int).FullName, rows));
            parameters.Add(new XElement(typeof(int).FullName, columns));
            thisElement.Add(parameters);
            foreach (EquationBase eb in ChildEquations)
            {
                thisElement.Add(eb.Serialize());
            }
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = xElement.Elements(typeof(RowContainer).Name).ToArray();
            for (int i = 0; i < ChildEquations.Count; i++)
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
                double[] rowRefYs = new double[rows];
                double[] topOffsets = new double[rows + 1];

                for (int i = 0; i < rows; i++)
                {
                    rowRefYs[i] = ChildEquations.Skip(i * columns).Take(columns).Max(x => x.RefY);
                    topOffsets[i + 1] = ChildEquations.Skip(i * columns).Take(columns).Max(x => x.Height) + topOffsets[i];
                }

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        ChildEquations[i * columns + j].MidY = Top + rowRefYs[i] + topOffsets[i] + CellSpace * i;
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
                double[] columnRefXs = new double[columns];
                double[] leftOffsets = new double[columns + 1];
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        columnRefXs[i] = Math.Max(ChildEquations[j * columns + i].RefX, columnRefXs[i]);
                        leftOffsets[i + 1] = Math.Max(ChildEquations[j * columns + i].Width, leftOffsets[i + 1]);
                    }
                    leftOffsets[i + 1] += leftOffsets[i];
                }
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        ChildEquations[j * columns + i].MidX = value + columnRefXs[i] + leftOffsets[i] + CellSpace * i;
                    }
                }
            }
        }

        public override void CalculateWidth()
        {
            double[] columnWidths = new double[columns];
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    columnWidths[i] = Math.Max(ChildEquations[j * columns + i].Width, columnWidths[i]);
                }
            }
            Width = columnWidths.Sum() + CellSpace * (columns - 1);
        }

        public override void CalculateHeight()
        {
            double[] rowHeights = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                rowHeights[i] = ChildEquations.Skip(i * columns).Take(columns).Max(x => x.Height);
            }
            Height = rowHeights.Sum() + CellSpace * (rows - 1);
        }

        public override double RefY
        {
            get
            {
                if (rows == 1)
                {
                    return ChildEquations.Max(x => x.RefY);
                }
                else if (rows % 2 == 0)
                {
                    //return childEquations.Take(rows / 2 * columns).Sum(x => x.Height) - CellSpace / 2 + FontSize * .3;
                    double[] rowHeights = new double[rows / 2];
                    for (int i = 0; i < rows / 2; i++)
                    {
                        rowHeights[i] = ChildEquations.Skip(i * columns).Take(columns).Max(x => x.Height);
                    }
                    return rowHeights.Sum() + CellSpace * rows/2 - CellSpace/2 + FontSize * .1;
                }
                else
                {
                    //return childEquations.Skip(rows / 2 * columns).Take(columns).Max(x => x.MidY) - Top;
                    double[] rowHeights = new double[rows / 2 + 1];
                    for (int i = 0; i < rows / 2; i++)
                    {
                        rowHeights[i] = ChildEquations.Skip(i * columns).Take(columns).Max(x => x.Height);
                    }
                    rowHeights[rows / 2] = ChildEquations.Skip(rows / 2 * columns).Take(columns).Max(x => x.RefY);
                    return rowHeights.Sum() + CellSpace * (rows / 2);// -FontSize * .1;
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
            int currentIndex = ChildEquations.IndexOf(ActiveChild);
            if (key == Key.Right)
            {
                if (currentIndex % columns < columns - 1)//not last column?
                {
                    ActiveChild = ChildEquations[currentIndex + 1];
                    return true;
                }
            }
            else if (key == Key.Left)
            {
                if (currentIndex % columns > 0)//not last column?
                {
                    ActiveChild = ChildEquations[currentIndex - 1];
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (currentIndex / columns > 0)//not in first row?
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[currentIndex - columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Down)
            {
                if (currentIndex / columns < rows - 1)//not in last row?
                {
                    Point point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[currentIndex + columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
