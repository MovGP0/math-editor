using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.IO;
using System.Windows.Media.Imaging;

namespace Editor
{
    public sealed class RowContainer : EquationContainer, IRowContainer
    {
        readonly double _lineSpaceFactor;
        private double LineSpace => _lineSpaceFactor * FontSize;

        public IEquationBase FirstRow => ChildEquations.First();
        public IEquationBase LastRow => ChildEquations.Last();

        public override void Paste(XElement xe)
        {
            if (((IEquationRow)ActiveChild).ActiveChild.GetType() == typeof(TextEquation) && xe.Name.LocalName == GetType().Name)
            {
                var children = xe.Element("ChildRows");
                var newRows = new List<IEquationRow>();
                foreach (var xElement in children.Elements())
                {
                    var row = new EquationRow(this);
                    row.DeSerialize(xElement);
                    newRows.Add(row);
                    row.FontSize = FontSize;
                }
                if (newRows.Count > 0)
                {
                    var activeText = (TextEquation)((IEquationRow)ActiveChild).ActiveChild;
                    var action = new RowContainerPasteAction(this)
                    {
                        ActiveEquation = ActiveChild,
                        ActiveEquationSelectedItems = ActiveChild.SelectedItems,
                        ActiveEquationSelectionIndex = ActiveChild.SelectionStartIndex,
                        ActiveTextInChildRow = activeText,
                        TextEquationDecorations = activeText.GetDecorations(),
                        CaretIndexOfActiveText = activeText.CaretIndex,
                        TextEquationContents = activeText.Text,
                        TextEquationFormats = activeText.GetFormats(),
                        TextEquationModes = activeText.GetModes(),
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        SelectedItemsOfTextEquation = activeText.SelectedItems,
                        SelectionStartIndexOfTextEquation = activeText.SelectionStartIndex,
                        HeadTextOfPastedRows = newRows[0].GetFirstTextEquation().Text,
                        TailTextOfPastedRows = newRows.Last().GetLastTextEquation().Text,
                        HeadFormatsOfPastedRows = newRows[0].GetFirstTextEquation().GetFormats(),
                        TailFormatsOfPastedRows = newRows.Last().GetLastTextEquation().GetFormats(),
                        HeadModeOfPastedRows = newRows[0].GetFirstTextEquation().GetModes(),
                        TailModesOfPastedRows = newRows.Last().GetLastTextEquation().GetModes(),                        
                        HeadDecorationsOfPastedRows = newRows[0].GetFirstTextEquation().GetDecorations(),
                        TailDecorationsOfPastedRows = newRows.Last().GetLastTextEquation().GetDecorations(),
                        Equations = newRows
                    };
                    var newRow = (IEquationRow)ActiveChild.Split(this);
                    ((IEquationRow)ActiveChild).Merge(newRows[0]);
                    var index = ChildEquations.IndexOf(ActiveChild) + 1;
                    ChildEquations.InsertRange(index, newRows.Skip(1));
                    newRows.Last().Merge(newRow);
                    newRows.Add(newRow);
                    ActiveChild = ChildEquations[index + newRows.Count - 3];
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.Paste(xe);
            }
        }

        public override void ConsumeText(string text)
        {
            if (((IEquationRow)ActiveChild).ActiveChild.GetType() == typeof(TextEquation))
            {
                var lines = new List<string>();
                using (var reader = new StringReader(text))
                {
                    string s;
                    while ((s = reader.ReadLine()) != null)
                    {
                        lines.Add(s);
                    }
                }
                if (lines.Count == 1)
                {
                    ActiveChild.ConsumeText(lines[0]);
                }
                else if (lines.Count > 1)
                {
                    var newEquations = new List<IEquationRow>();
                    var activeText = (TextEquation)((IEquationRow)ActiveChild).ActiveChild;
                    var action = new RowContainerTextAction(this)
                    {
                        ActiveEquation = ActiveChild,
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        ActiveEquationSelectedItems = ActiveChild.SelectedItems,
                        ActiveEquationSelectionIndex = ActiveChild.SelectionStartIndex,
                        ActiveTextInRow = activeText,
                        CaretIndexOfActiveText = activeText.CaretIndex,
                        SelectedItemsOfTextEquation = activeText.SelectedItems,
                        SelectionStartIndexOfTextEquation = activeText.SelectionStartIndex,
                        TextEquationContents = activeText.Text,
                        TextEquationFormats = activeText.GetFormats(),
                        FirstLineOfInsertedText = lines[0],
                        Equations = newEquations
                    };
                    UndoManager.DisableAddingActions = true;
                    ActiveChild.ConsumeText(lines[0]);
                    action.FirstFormatsOfInsertedText = activeText.GetFormats();
                    var splitRow = (IEquationRow)ActiveChild.Split(this);
                    if (!splitRow.IsEmpty)
                    {
                        ChildEquations.Add(splitRow);
                    }
                    var activeIndex = ChildEquations.IndexOf(ActiveChild);
                    var i = 1;
                    for (; i < lines.Count; i++)
                    {
                        var row = new EquationRow(this);
                        row.ConsumeText(lines[i]);
                        ChildEquations.Insert(activeIndex + i, row);
                        newEquations.Add(row);
                    }
                    UndoManager.DisableAddingActions = false;
                    newEquations.Add(splitRow);
                    ActiveChild = ChildEquations[activeIndex + lines.Count - 1];
                    ((TextEquation)((IEquationRow)ActiveChild).ActiveChild).MoveToEnd();
                    SelectedItems = 0;
                    action.ActiveEquationAfterChange = ActiveChild;
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.ConsumeText(text);
            }
        }

        public void DrawVisibleRows(DrawingContext dc, double top, double bottom)
        {
            if (IsSelecting)
            {
                try { DrawSelectionRegion(dc); }
                catch { }
            }
            foreach (var eb in ChildEquations)
            {
                if (eb.Bottom >= top)
                {
                    eb.DrawEquation(dc);
                }
                if (eb.Bottom >= bottom)
                {
                    break;
                }
            }
        }

        public override void DrawEquation(DrawingContext dc)
        {
            if (IsSelecting)
            {
                DrawSelectionRegion(dc);
            }
            base.DrawEquation(dc);
        }

        private void DrawSelectionRegion(DrawingContext dc)
        {
            var topSelectedRowIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var topEquation = ChildEquations[topSelectedRowIndex];
            var rect = topEquation.GetSelectionBounds();
            dc.DrawRectangle(Brushes.LightGray, null, rect);

            var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - topSelectedRowIndex;
            if (count > 0)
            {
                rect.Union(new Point(topEquation.Right, rect.Bottom + LineSpace + 1));
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                var bottomEquation = ChildEquations[topSelectedRowIndex + count];
                rect = bottomEquation.GetSelectionBounds();
                rect.Union(new Point(bottomEquation.Left, bottomEquation.Top));
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                for (var i = topSelectedRowIndex + 1; i < topSelectedRowIndex + count; i++)
                {
                    var equation = ChildEquations[i];
                    rect = equation.Bounds;
                    rect.Union(new Point(rect.Left, rect.Bottom + LineSpace + 1));
                    dc.DrawRectangle(Brushes.LightGray, null, rect);
                }
            }
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
                var firstRow = (IEquationRow)ChildEquations[startIndex];
                var lastRow = (IEquationRow)ChildEquations[endIndex];
                var firstText = firstRow.GetFirstSelectionText();
                var lastText = lastRow.GetLastSelectionText();
                var equations = new List<IEquationBase>();
                var action = new RowContainerRemoveAction(this)
                {
                    ActiveEquation = ActiveChild,
                    HeadEquationRow = firstRow,
                    TailEquationRow = lastRow,
                    HeadTextEquation = firstText,
                    TailTextEquation = lastText,
                    SelectionStartIndex = SelectionStartIndex,
                    SelectedItems = SelectedItems,
                    FirstRowActiveIndex = firstRow.ActiveChildIndex,
                    FirstRowSelectionIndex = firstRow.SelectionStartIndex,
                    FirstRowSelectedItems = firstRow.SelectedItems,
                    LastRowActiveIndex = lastRow.ActiveChildIndex,
                    LastRowSelectionIndex = lastRow.SelectionStartIndex,
                    LastRowSelectedItems = lastRow.SelectedItems,
                    FirstTextCaretIndex = firstText.CaretIndex,
                    LastTextCaretIndex = lastText.CaretIndex,
                    FirstTextSelectionIndex = firstText.SelectionStartIndex,
                    LastTextSelectionIndex = lastText.SelectionStartIndex,
                    FirstTextSelectedItems = firstText.SelectedItems,
                    LastTextSelectedItems = lastText.SelectedItems,
                    FirstText = firstText.Text,
                    LastText = lastText.Text,
                    FirstFormats = firstText.GetFormats(),
                    LastFormats = lastText.GetFormats(),
                    FirstModes = firstText.GetModes(),
                    LastModes = lastText.GetModes(),
                    FirstRowDeletedContent = firstRow.DeleteTail(),
                    LastRowDeletedContent = lastRow.DeleteHead(),
                    FirstDecorations = firstRow.GetFirstTextEquation().GetDecorations(),
                    LastDecorations = lastRow.GetLastTextEquation().GetDecorations(),
                    Equations = equations,
                    FirstRowActiveIndexAfterRemoval = firstRow.ActiveChildIndex,
                };

                firstText.RemoveSelection(false); //.DeleteSelectedText();
                lastText.RemoveSelection(false); //.DeleteSelectedText();                
                firstRow.Merge(lastRow);
                for (var i = endIndex; i > startIndex; i--)
                {
                    equations.Add(ChildEquations[i]);
                    ChildEquations.RemoveAt(i);
                }
                SelectedItems = 0;
                equations.Reverse();
                ActiveChild = firstRow;
                if (registerUndo)
                {
                    UndoManager.AddUndoAction(action);
                }
            }
            else
            {
                ActiveChild.RemoveSelection(registerUndo);
            }
            CalculateSize();
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                var firstRow = (IEquationRow)ChildEquations[startIndex];
                var lastRow = (IEquationRow)ChildEquations[startIndex + count];
                var firstRowSelectedItems = firstRow.GetSelectedEquations();
                var lastRowSelectedItems = lastRow.GetSelectedEquations();

                var newFirstRow = new EquationRow(this);
                var newLastRow = new EquationRow(this);
                newFirstRow.GetFirstTextEquation().ConsumeFormattedText(firstRowSelectedItems.First().GetSelectedText(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedFormats(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedModes(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedDecorations(), false);
                newLastRow.GetFirstTextEquation().ConsumeFormattedText(lastRowSelectedItems.Last().GetSelectedText(),
                                                                       ((TextEquation)lastRowSelectedItems.Last()).GetSelectedFormats(),
                                                                       ((TextEquation)lastRowSelectedItems.Last()).GetSelectedModes(),
                                                                       ((TextEquation)lastRowSelectedItems.First()).GetSelectedDecorations(),
                                                                       false);

                firstRowSelectedItems.RemoveAt(0);
                lastRowSelectedItems.RemoveAt(lastRowSelectedItems.Count - 1);
                newFirstRow.AddChildren(firstRowSelectedItems, false);
                newLastRow.AddChildren(lastRowSelectedItems, true);
                var equations = new List<IEquationBase>();
                for (var i = startIndex + 1; i < startIndex + count; i++)
                {
                    equations.Add(ChildEquations[i]);
                }
                equations.Add(newLastRow);
                foreach (var eb in equations)
                {
                    eb.Left = 1;
                }
                var left = firstRow.GetFirstSelectionText().Right - Left;
                var firstTextRect = firstRow.GetFirstSelectionText().GetSelectionBounds();
                if (!firstTextRect.IsEmpty)
                {
                    left = firstTextRect.Left - Left;
                }
                newFirstRow.Left = left + 1;
                equations.Insert(0, newFirstRow);
                double nextY = 1;
                var width = firstRow.Width;
                double height = 0;
                foreach (var eb in equations)
                {
                    eb.Top = nextY;
                    nextY += eb.Height + LineSpace;
                    width = eb.Width > width ? eb.Width : width;
                    height += eb.Height + LineSpace;
                }
                height -= LineSpace;
                var bitmap = new RenderTargetBitmap((int)(Math.Ceiling(width + 2)), (int)(Math.Ceiling(height + 2)), 96, 96, PixelFormats.Default);
                var dv = new DrawingVisual();
                IsSelecting = false;
                using (var dc = dv.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, bitmap.Width, bitmap.Height));
                    foreach (var eb in equations)
                    {
                        eb.DrawEquation(dc);
                    }
                }
                IsSelecting = true;
                bitmap.Render(dv);
                var thisElement = new XElement(GetType().Name);
                var children = new XElement("ChildRows");
                foreach (var eb in equations)
                {
                    eb.SelectAll();
                    children.Add(eb.Serialize());
                }
                thisElement.Add(children);
                foreach (var eb in equations)
                {
                    eb.DeSelect();
                }
                Left = Left;
                Top = Top;
                if (removeSelection)
                {
                    RemoveSelection(true);
                }
                return new CopyDataObject { Image = bitmap, Text = null, XElement = thisElement };
            }
            else
            {
                return base.Copy(removeSelection);
            }
        }

        public override void SelectAll()
        {
            base.SelectAll();
            ((IEquationRow)ChildEquations.Last()).MoveToEnd();
        }

        public override string GetSelectedText()
        {
            var stringBulider = new StringBuilder("");
            foreach (var eb in ChildEquations)
            {
                stringBulider.Append(eb.GetSelectedText() + Environment.NewLine);
            }
            return stringBulider.ToString();
        }

        public override bool Select(Key key)
        {
            if (key == Key.Left)
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild != ChildEquations.First())
                {
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                    SelectedItems--;
                    if (SelectedItems < 0)
                    {
                        ((IEquationRow)ActiveChild).MoveToEnd();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else if (key == Key.Right)
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild != ChildEquations.Last())
                {
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                    SelectedItems++;
                    if (SelectedItems > 0)
                    {
                        ((IEquationRow)ActiveChild).MoveToStart();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else if (key == Key.Home)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && SelectionStartIndex > 0)
                {
                    ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToStart();
                    for (var i = SelectionStartIndex - 1; i >= 0; i--)
                    {
                        ((IEquationRow)ChildEquations[i]).MoveToEnd();
                        ChildEquations[i].StartSelection();
                        ((IEquationRow)ChildEquations[i]).SelectToStart();
                    }
                    SelectedItems = -SelectionStartIndex;
                    ActiveChild = ChildEquations.First();
                }
                else
                {
                    ((IEquationRow)ActiveChild).SelectToStart();
                }
                return true;
            }
            else if (key == Key.End)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && SelectionStartIndex < ChildEquations.Count - 1)
                {
                    ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToEnd();
                    for (var i = SelectionStartIndex + 1; i < ChildEquations.Count; i++)
                    {
                        ((IEquationRow)ChildEquations[i]).MoveToStart();
                        ChildEquations[i].StartSelection();
                        ((IEquationRow)ChildEquations[i]).SelectToEnd();
                    }
                    SelectedItems = ChildEquations.Count - SelectionStartIndex - 1;
                    ActiveChild = ChildEquations.Last();
                }
                else
                {
                    ((IEquationRow)ActiveChild).SelectToEnd();
                }
                return true;
            }
            else if (key == Key.Up && SelectionStartIndex >= 0 && ChildEquations.IndexOf(ActiveChild) > 0)
            {
                var point = ChildEquations[SelectionStartIndex].GetVerticalCaretLocation();
                ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToStart();
                for (var i = SelectionStartIndex - 1; i > ChildEquations.IndexOf(ActiveChild); i--)
                {
                    ((IEquationRow)ChildEquations[i]).MoveToEnd();
                    ChildEquations[i].StartSelection();
                    ((IEquationRow)ChildEquations[i]).SelectToStart();
                }
                point.Y = ActiveChild.MidY;
                ((IEquationRow)ActiveChild).MoveToEnd();
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(point);
                SelectedItems = ChildEquations.IndexOf(ActiveChild) - SelectionStartIndex;
                return true;
            }
            else if (key == Key.Down && SelectionStartIndex < ChildEquations.Count && ChildEquations.IndexOf(ActiveChild) < ChildEquations.Count - 1)
            {
                var point = ChildEquations[SelectionStartIndex].GetVerticalCaretLocation();
                ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToEnd();
                for (var i = SelectionStartIndex + 1; i < ChildEquations.Count; i++)
                {
                    ((IEquationRow)ChildEquations[i]).MoveToStart();
                    ChildEquations[i].StartSelection();
                    ((IEquationRow)ChildEquations[i]).SelectToEnd();
                }
                point.Y = ActiveChild.MidY;
                ((IEquationRow)ActiveChild).MoveToStart();
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(point);
                SelectedItems = ChildEquations.IndexOf(ActiveChild) - SelectionStartIndex;
                return true;
            }
            return false;
        }

        public RowContainer(IEquationContainer parent, double lineSpaceFactor = 0)
            : base(parent)
        {
            IEquationRow newLine = new EquationRow(this);
            AddLine(newLine);
            Height = newLine.Height;
            Width = newLine.Width;
            _lineSpaceFactor = lineSpaceFactor;
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("ChildRows");
            foreach (var childRow in ChildEquations)
            {
                children.Add(childRow.Serialize());
            }
            thisElement.Add(children);
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var children = xElement.Element("ChildRows");
            ChildEquations.Clear();
            foreach (var xe in children.Elements())
            {
                var row = new EquationRow(this);
                row.DeSerialize(xe);
                ChildEquations.Add(row);
            }
            if (ChildEquations.Count == 0)
            {
                ChildEquations.Add(new EquationRow(this));
            }
            ActiveChild = ChildEquations.First();
            CalculateSize();
        }

        void AddLine(IEquationRow newRow)
        {
            var index = 0;
            if (ChildEquations.Count > 0)
            {
                index = ChildEquations.IndexOf((IEquationRow)ActiveChild) + 1;
            }
            ChildEquations.Insert(index, newRow);
            ActiveChild = newRow;
            CalculateSize();
        }

        public override IEquationBase Split(IEquationContainer newParent)
        {
            var newRow = (IEquationRow)ActiveChild.Split(this);
            if (newRow != null)
            {
                var activeRow = ActiveChild as EquationRow;
                var rca = new RowContainerAction(this, ChildEquations.IndexOf(activeRow), activeRow.ActiveChildIndex, activeRow.TextLength, newRow) { UndoFlag = false };                
                UndoManager.AddUndoAction(rca);
                AddLine(newRow);
            }
            CalculateSize();
            return newRow;
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            foreach (var eb in ChildEquations)
            {
                var rect = new Rect(0, eb.Top, double.MaxValue, eb.Height);
                if (rect.Contains(mousePoint))
                {
                    ActiveChild = eb;
                    return eb.ConsumeMouseClick(mousePoint);
                }
            }
            return false;
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (mousePoint.Y > ActiveChild.Top - LineSpace && mousePoint.Y < ActiveChild.Bottom + LineSpace)
            {
                ActiveChild.HandleMouseDrag(mousePoint);
            }
            else
            {
                if (mousePoint.Y > ActiveChild.Bottom + LineSpace)
                {
                    for (var i = ChildEquations.IndexOf(ActiveChild) + 1; i < ChildEquations.Count; i++)
                    {
                        ActiveChild = ChildEquations[i];
                        if (ActiveChild.Top <= mousePoint.Y && ActiveChild.Bottom + LineSpace >= mousePoint.Y)
                        {
                            break;
                        }
                    }
                    for (var i = SelectionStartIndex + 1; i < ChildEquations.IndexOf(ActiveChild); i++)
                    {
                        ((IEquationRow)ChildEquations[i]).MoveToStart();
                        ChildEquations[i].StartSelection();
                        ((IEquationRow)ChildEquations[i]).SelectToEnd();
                    }
                    if (ChildEquations.IndexOf(ActiveChild) > SelectionStartIndex)
                    {
                        ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToEnd();
                        var row = ActiveChild as EquationRow;
                        row.MoveToStart();
                        row.StartSelection();
                    }

                }
                else if (mousePoint.Y < ActiveChild.Top - LineSpace)
                {
                    for (var i = ChildEquations.IndexOf(ActiveChild) - 1; i >= 0; i--)
                    {
                        ActiveChild = ChildEquations[i];
                        if (ActiveChild.Top - LineSpace <= mousePoint.Y && ActiveChild.Bottom >= mousePoint.Y)
                        {
                            break;
                        }
                    }
                    for (var i = SelectionStartIndex - 1; i > ChildEquations.IndexOf(ActiveChild); i--)
                    {
                        ((IEquationRow)ChildEquations[i]).MoveToEnd();
                        ChildEquations[i].StartSelection();
                        ((IEquationRow)ChildEquations[i]).SelectToStart();
                    }
                    if (ChildEquations.IndexOf(ActiveChild) < SelectionStartIndex)
                    {
                        ((IEquationRow)ChildEquations[SelectionStartIndex]).SelectToStart();
                        var row = ActiveChild as EquationRow;
                        row.MoveToEnd();
                        row.StartSelection();
                    }
                }
                ActiveChild.HandleMouseDrag(mousePoint);
                SelectedItems = ChildEquations.IndexOf(ActiveChild) - SelectionStartIndex;
            }
            StatusBarHelper.PrintStatusMessage("ActiveStart " + ActiveChild.SelectionStartIndex + ", ActiveItems" + ActiveChild.SelectedItems);
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                switch (HAlignment)
                {
                    case HAlignment.Left:
                        foreach (var eb in ChildEquations)
                        {
                            eb.Left = value;
                        }
                        break;
                    case HAlignment.Right:
                        foreach (var eb in ChildEquations)
                        {
                            eb.Right = Right;
                        }
                        break;
                    case HAlignment.Center:
                        foreach (var eb in ChildEquations)
                        {
                            eb.MidX = MidX;
                        }
                        break;
                }
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                var nextY = value;
                foreach (var eb in ChildEquations)
                {
                    eb.Top = nextY;
                    nextY += eb.Height + LineSpace;
                }
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (key == Key.Home)
                {
                    ActiveChild = ChildEquations.First();
                    ((IEquationRow)ActiveChild).MoveToStart();
                }
                else if (key == Key.End)
                {
                    ActiveChild = ChildEquations.Last();
                    ((IEquationRow)ActiveChild).MoveToEnd();
                }
                return true;
            }
            var result = false;
            if (ActiveChild.ConsumeKey(key))
            {
                result = true;
            }
            else if (key == Key.Enter)
            {
                Split(this);
                ((IEquationRow)ActiveChild).MoveToStart();
                result = true;
            }
            else if (key == Key.Delete)
            {
                if (ActiveChild != ChildEquations.Last())
                {
                    var activeRow = ActiveChild as EquationRow;
                    var rowToRemove = (IEquationRow)ChildEquations[ChildEquations.IndexOf(activeRow) + 1];
                    UndoManager.AddUndoAction(new RowContainerAction(this, ChildEquations.IndexOf(activeRow), activeRow.ActiveChildIndex, activeRow.TextLength, rowToRemove));
                    activeRow.Merge(rowToRemove);
                    ChildEquations.RemoveAt(ChildEquations.IndexOf(activeRow) + 1);
                }
                result = true;
            }
            else if (!result)
            {
                if (key == Key.Up && ActiveChild != ChildEquations.First())
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    result = true;
                }
                else if (key == Key.Down && ActiveChild != ChildEquations.Last())
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    result = true;
                }
                else if (key == Key.Left)
                {
                    if (ActiveChild != ChildEquations.First())
                    {
                        ActiveChild = ChildEquations[ChildEquations.IndexOf((IEquationRow)ActiveChild) - 1];
                        result = true;
                    }
                }
                else if (key == Key.Right)
                {
                    if (ActiveChild != ChildEquations.Last())
                    {
                        ActiveChild = ChildEquations[ChildEquations.IndexOf((IEquationRow)ActiveChild) + 1];
                        result = true;
                    }
                }
                else if (key == Key.Back)
                {
                    if (ActiveChild != ChildEquations.First())
                    {
                        var activeRow = ActiveChild as EquationRow;
                        var previousRow = (IEquationRow)ChildEquations[ChildEquations.IndexOf(activeRow) - 1];
                        var index = previousRow.ActiveChildIndex;
                        previousRow.MoveToEnd();
                        UndoManager.AddUndoAction(new RowContainerAction(this, ChildEquations.IndexOf(previousRow), previousRow.ActiveChildIndex, previousRow.TextLength, activeRow));
                        previousRow.Merge(activeRow);
                        ChildEquations.Remove(activeRow);
                        ActiveChild = previousRow;
                        result = true;
                    }
                }
            }
            CalculateSize();
            return result;
        }

        public override void CalculateWidth()
        {
            double maxLeftHalf = 0;
            double maxRightHalf = 0;
            foreach (var eb in ChildEquations)
            {
                if (eb.RefX > maxLeftHalf)
                {
                    maxLeftHalf = eb.RefX;
                }
                if (eb.Width - eb.RefX > maxRightHalf)
                {
                    maxRightHalf = eb.Width - eb.RefX;
                }
                eb.Left = Left;
            }
            Width = maxLeftHalf + maxRightHalf;
        }

        public override void CalculateHeight()
        {
            double height = 0;
            foreach (var eb in ChildEquations)
            {
                height += eb.Height + LineSpace;
            }
            Height = height;
            var nextY = Top;
            foreach (var eb in ChildEquations)
            {
                eb.Top = nextY;
                nextY += eb.Height + LineSpace;
            }
        }

        public override double RefY
        {
            get
            {
                var count = ChildEquations.Count;
                if (count == 1)
                {
                    return ChildEquations.First().RefY;
                }
                else if (count % 2 == 0)
                {
                    return ChildEquations[(count + 1) / 2].Top - Top - LineSpace / 2;
                }
                else
                {
                    return ChildEquations[count / 2].MidY - Top;
                    //base.RefY;
                }
            }
        }

        public void ProcessUndo(EquationAction action)
        {
            var type = action.GetType();
            if (type == typeof(RowContainerAction))
            {
                ProcessRowContainerAction(action);
                IsSelecting = false;
            }
            else if (type == typeof(RowContainerTextAction))
            {
                ProcessRowContainerTextAction(action);
            }
            else if (type == typeof(RowContainerPasteAction))
            {
                ProcessRowContainerPasteAction(action);
            }
            else if (type == typeof(RowContainerFormatAction))
            {
                ProcessRowContainerFormatAction(action);
            }
            else if (type == typeof(RowContainerRemoveAction))
            {
                ProcessRowContainerRemoveAction(action);
            }
            CalculateSize();
            ParentEquation.ChildCompletedUndo(this);
        }

        private void ProcessRowContainerPasteAction(EquationAction action)
        {
            var pasteAction = action as RowContainerPasteAction;
            var activeRow = (IEquationRow)pasteAction.ActiveEquation;
            if (pasteAction.UndoFlag)
            {
                SelectedItems = pasteAction.SelectedItems;
                SelectionStartIndex = pasteAction.SelectionStartIndex;
                pasteAction.ActiveTextInChildRow.ResetTextEquation(pasteAction.CaretIndexOfActiveText, pasteAction.SelectionStartIndexOfTextEquation,
                                                                   pasteAction.SelectedItemsOfTextEquation, pasteAction.TextEquationContents,
                                                                   pasteAction.TextEquationFormats, pasteAction.TextEquationModes,
                                                                   pasteAction.TextEquationDecorations);
                activeRow.ResetRowEquation(pasteAction.ActiveTextInChildRow, pasteAction.ActiveEquationSelectionIndex, pasteAction.ActiveEquationSelectedItems);
                activeRow.Truncate();
                activeRow.Merge(pasteAction.Equations.Last());
                foreach (var eb in pasteAction.Equations)
                {
                    ChildEquations.Remove(eb);
                }
                activeRow.CalculateSize();
                ActiveChild = activeRow;
            }
            else
            {
                activeRow.ResetRowEquation(pasteAction.ActiveTextInChildRow, pasteAction.ActiveEquationSelectionIndex, pasteAction.ActiveEquationSelectedItems);
                var newRow = (IEquationRow)activeRow.Split(this);
                pasteAction.Equations[pasteAction.Equations.Count - 2].GetLastTextEquation().SetFormattedText(pasteAction.TailTextOfPastedRows, pasteAction.TailFormatsOfPastedRows, pasteAction.TailModesOfPastedRows);
                activeRow.Merge(pasteAction.Equations.First());
                var index = ChildEquations.IndexOf(ActiveChild) + 1;
                ChildEquations.InsertRange(index, pasteAction.Equations.Skip(1));
                ChildEquations.RemoveAt(ChildEquations.Count - 1);
                pasteAction.Equations[pasteAction.Equations.Count - 2].Merge(newRow);
                ActiveChild = ChildEquations[index + pasteAction.Equations.Count - 3];
                ((IEquationRow)ActiveChild).MoveToEnd();
                FontSize = FontSize;
                SelectedItems = 0;
            }
        }

        private void ProcessRowContainerTextAction(EquationAction action)
        {
            var textAction = action as RowContainerTextAction;
            ActiveChild = textAction.ActiveEquation;
            var activeRow = (IEquationRow)ActiveChild;
            activeRow.ResetRowEquation(textAction.ActiveTextInRow, textAction.ActiveEquationSelectionIndex, textAction.ActiveEquationSelectedItems);
            if (textAction.UndoFlag)
            {
                textAction.ActiveTextInRow.ResetTextEquation(textAction.CaretIndexOfActiveText, textAction.SelectionStartIndexOfTextEquation, textAction.SelectedItemsOfTextEquation, textAction.TextEquationContents, textAction.TextEquationFormats, textAction.TextEquationModes, textAction.TextEquationDecoration);
                UndoManager.DisableAddingActions = true;
                ActiveChild.ConsumeFormattedText(textAction.FirstLineOfInsertedText, textAction.FirstFormatsOfInsertedText, textAction.FirstModesOfInsertedText, textAction.FirstDecorationsOfInsertedText, false);
                UndoManager.DisableAddingActions = false;
                var splitRow = (IEquationRow)ActiveChild.Split(this);
                ChildEquations.InsertRange(ChildEquations.IndexOf(ActiveChild) + 1, textAction.Equations);
                if (splitRow.IsEmpty)
                {
                    ChildEquations.Remove(textAction.Equations.Last());
                }
                ActiveChild = textAction.ActiveEquationAfterChange;
                textAction.ActiveTextInRow.MoveToEnd();
                SelectedItems = 0; 
            }
            else
            {
                SelectedItems = textAction.SelectedItems;
                SelectionStartIndex = textAction.SelectionStartIndex;
                activeRow.Merge((IEquationRow)textAction.Equations.Last());
                textAction.ActiveTextInRow.ResetTextEquation(textAction.CaretIndexOfActiveText, textAction.SelectionStartIndexOfTextEquation, 
                                                             textAction.SelectedItemsOfTextEquation, textAction.TextEquationContents, 
                                                             textAction.TextEquationFormats, textAction.FirstModesOfInsertedText,
                                                             textAction.FirstDecorationsOfInsertedText);
                foreach (var eb in textAction.Equations)
                {
                    if (ChildEquations.Contains(eb))
                    {
                        ChildEquations.Remove(eb);
                    }
                }
            }
            activeRow.CalculateSize();
        }

        private void ProcessRowContainerRemoveAction(EquationAction action)
        {
            var rowAction = action as RowContainerRemoveAction;
            if (rowAction.UndoFlag)
            {
                ChildEquations.InsertRange(ChildEquations.IndexOf(rowAction.HeadEquationRow) + 1, rowAction.Equations);
                rowAction.HeadEquationRow.ActiveChildIndex = rowAction.FirstRowActiveIndexAfterRemoval;
                rowAction.HeadEquationRow.Truncate();
                rowAction.HeadEquationRow.ResetRowEquation(rowAction.FirstRowActiveIndex, rowAction.FirstRowSelectionIndex, rowAction.FirstRowSelectedItems, rowAction.FirstRowDeletedContent, true);
                rowAction.TailEquationRow.ResetRowEquation(rowAction.LastRowActiveIndex, rowAction.LastRowSelectionIndex, rowAction.LastRowSelectedItems, rowAction.LastRowDeletedContent, false);
                rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex, rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats, rowAction.FirstModes, rowAction.FirstDecorations);
                rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex, rowAction.LastTextSelectedItems, rowAction.LastText, rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
                foreach (var eb in rowAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                rowAction.HeadEquationRow.FontSize = FontSize;
                rowAction.TailEquationRow.FontSize = FontSize;
                SelectedItems = rowAction.SelectedItems;
                SelectionStartIndex = rowAction.SelectionStartIndex;
                ActiveChild = rowAction.ActiveEquation;
                IsSelecting = true;
            }
            else
            {
                rowAction.HeadEquationRow.ResetRowEquation(rowAction.FirstRowActiveIndex, rowAction.FirstRowSelectionIndex, rowAction.FirstRowSelectedItems);
                rowAction.TailEquationRow.ResetRowEquation(rowAction.LastRowActiveIndex, rowAction.LastRowSelectionIndex, rowAction.LastRowSelectedItems);
                rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex, rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats, rowAction.FirstModes, rowAction.FirstDecorations);
                rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex, rowAction.LastTextSelectedItems, rowAction.LastText, rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
                rowAction.HeadTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.TailTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.HeadEquationRow.DeleteTail();
                rowAction.TailEquationRow.DeleteHead();
                rowAction.HeadEquationRow.Merge(rowAction.TailEquationRow);
                var index = ChildEquations.IndexOf(rowAction.HeadEquationRow);
                for (var i = index + rowAction.Equations.Count; i > index; i--)
                {
                    ChildEquations.RemoveAt(i);
                }
                ActiveChild = rowAction.HeadEquationRow;
                SelectedItems = 0;
                IsSelecting = false;
            }
        }

        private void ProcessRowContainerAction(EquationAction action)
        {
            var containerAction = action as RowContainerAction;
            if (containerAction.UndoFlag)
            {
                var activeRow = (IEquationRow)ChildEquations[containerAction.Index];
                activeRow.SetCurrentChild(containerAction.ChildIndexInRow, containerAction.CaretIndex);
                activeRow.Truncate(containerAction.ChildIndexInRow + 1, containerAction.CaretIndex);
                ChildEquations.Insert(containerAction.Index + 1, containerAction.Equation);
                ActiveChild = containerAction.Equation;
                ActiveChild.FontSize = FontSize;
            }
            else
            {
                ((IEquationRow)ChildEquations[containerAction.Index]).Merge((IEquationRow)ChildEquations[containerAction.Index + 1]);
                ChildEquations.Remove(ChildEquations[containerAction.Index + 1]);
                ActiveChild = ChildEquations[containerAction.Index];
                ((IEquationRow)ActiveChild).SetCurrentChild(containerAction.ChildIndexInRow, containerAction.CaretIndex);
            }
        }

        public override void ModifySelection(string operation, string argument, bool applied, bool addUndo)
        {
            if (IsSelecting)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                if (endIndex - startIndex > 0)
                {
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        ChildEquations[i].ModifySelection(operation, argument, applied, false);
                    }
                    if (addUndo)
                    {
                        var ecfa = new RowContainerFormatAction(this)
                        {
                            Operation = operation,
                            Argument = argument,
                            Applied = applied,
                            SelectionStartIndex = SelectionStartIndex,
                            SelectedItems = SelectedItems,
                            ActiveChild = ActiveChild,
                            FirstRowActiveChildIndex = ((IEquationRow)ChildEquations[startIndex]).ActiveChildIndex,
                            FirstRowSelectionStartIndex = ChildEquations[startIndex].SelectionStartIndex,
                            FirstRowSelectedItems = ChildEquations[startIndex].SelectedItems,
                            LastRowActiveChildIndex = ((IEquationRow)ChildEquations[endIndex]).ActiveChildIndex,
                            LastRowSelectionStartIndex = ChildEquations[endIndex].SelectionStartIndex,
                            LastRowSelectedItems = ChildEquations[endIndex].SelectedItems,
                            FirstTextCaretIndex = ((IEquationRow)ChildEquations[startIndex]).GetFirstSelectionText().CaretIndex,
                            FirstTextSelectionStartIndex = ((IEquationRow)ChildEquations[startIndex]).GetFirstSelectionText().SelectionStartIndex,
                            FirstTextSelectedItems = ((IEquationRow)ChildEquations[startIndex]).GetFirstSelectionText().SelectedItems,
                            LastTextCaretIndex = ((IEquationRow)ChildEquations[endIndex]).GetLastSelectionText().CaretIndex,
                            LastTextSelectionStartIndex = ((IEquationRow)ChildEquations[endIndex]).GetLastSelectionText().SelectionStartIndex,
                            LastTextSelectedItems = ((IEquationRow)ChildEquations[endIndex]).GetLastSelectionText().SelectedItems,
                        };
                        UndoManager.AddUndoAction(ecfa);
                    }
                }
                else
                {
                    ActiveChild.ModifySelection(operation, argument, applied, addUndo);
                }
                CalculateSize();
            }
        }

        private void ProcessRowContainerFormatAction(EquationAction action)
        {
            var rcfa = action as RowContainerFormatAction;
            if (rcfa != null)
            {
                IsSelecting = true;
                ActiveChild = rcfa.ActiveChild;
                SelectedItems = rcfa.SelectedItems;
                SelectionStartIndex = rcfa.SelectionStartIndex;
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                ((IEquationRow)ChildEquations[startIndex]).ActiveChildIndex = rcfa.FirstRowActiveChildIndex;
                ChildEquations[startIndex].SelectionStartIndex = rcfa.FirstRowSelectionStartIndex;
                ChildEquations[startIndex].SelectedItems = rcfa.FirstRowSelectedItems;
                ((IEquationRow)ChildEquations[endIndex]).ActiveChildIndex = rcfa.LastRowActiveChildIndex;
                ChildEquations[endIndex].SelectionStartIndex = rcfa.LastRowSelectionStartIndex;
                ChildEquations[endIndex].SelectedItems = rcfa.LastRowSelectedItems;
                ((IEquationRow)ChildEquations[startIndex]).GetFirstTextEquation().CaretIndex = rcfa.FirstTextCaretIndex;
                ((IEquationRow)ChildEquations[startIndex]).GetFirstTextEquation().SelectionStartIndex = rcfa.FirstTextSelectionStartIndex;
                ((IEquationRow)ChildEquations[startIndex]).GetFirstTextEquation().SelectedItems = rcfa.FirstTextSelectedItems;
                ((IEquationRow)ChildEquations[endIndex]).GetLastTextEquation().CaretIndex = rcfa.LastTextCaretIndex;
                ((IEquationRow)ChildEquations[endIndex]).GetLastTextEquation().SelectionStartIndex = rcfa.LastTextSelectionStartIndex;
                ((IEquationRow)ChildEquations[endIndex]).GetLastTextEquation().SelectedItems = rcfa.LastTextSelectedItems;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    if (i > startIndex && i < endIndex)
                    {
                        ChildEquations[i].SelectAll();
                    }
                    ChildEquations[i].ModifySelection(rcfa.Operation, rcfa.Argument, rcfa.UndoFlag ? !rcfa.Applied : rcfa.Applied, false);
                }
                CalculateSize();
                ParentEquation.ChildCompletedUndo(this);
            }
        }
    }
}
