using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows.Media.Imaging;

namespace Editor
{
    public class EquationRow : EquationContainer, ISupportsUndo
    {
        protected EquationContainer deleteable = null;
        static Pen boxPen = new Pen(Brushes.Blue, 1.1) { StartLineCap = PenLineCap.Flat, EndLineCap = PenLineCap.Flat };

        static EquationRow()
        {
            boxPen.DashStyle = DashStyles.Dash;
            boxPen.Freeze();
        }

        public EquationRow(EquationContainer parent)
            : base(parent)
        {
            var textEq = new TextEquation(this);
            ActiveChild = textEq;
            AddChild(textEq);
            CalculateSize();
        }

        public sealed override void CalculateSize()
        {
            base.CalculateSize();
        }

        public TextEquation GetFirstSelectionText()
        {
            return (TextEquation)ChildEquations[SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems];
        }

        public TextEquation GetLastSelectionText()
        {
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var otherOffset = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
            return (TextEquation)ChildEquations[otherOffset];
        }

        public void AddChildren(List<IEquationBase> equations, bool insertAtStart)
        {
            if (insertAtStart)
            {
                ChildEquations.InsertRange(0, equations);
            }
            else
            {
                ChildEquations.AddRange(equations);
            }
            CalculateSize();
        }

        public TextEquation GetFirstTextEquation()
        {
            return ChildEquations.First() as TextEquation;
        }

        public TextEquation GetLastTextEquation()
        {
            return ChildEquations.Last() as TextEquation;
        }

        public List<IEquationBase> GetSelectedEquations()
        {
            var list = new List<IEquationBase>();
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
            for (var i = startIndex; i <= endIndex; i++)
            {
                list.Add(ChildEquations[i]);
            }
            return list;
        }

        public List<IEquationBase> DeleteTail()
        {
            var removedList = new List<IEquationBase>();
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            if (SelectedItems != 0)
            {
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                for (var i = endIndex; i > startIndex; i--)
                {
                    removedList.Add(ChildEquations[i]);
                    ChildEquations.RemoveAt(i);
                }
                removedList.Reverse();
            }
            ActiveChild = ChildEquations[startIndex];
            return removedList;
        }

        public List<IEquationBase> DeleteHead()
        {
            var removedList = new List<IEquationBase>();
            var startIndex = (SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems);
            if (SelectedItems != 0)
            {
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                for (var i = endIndex - 1; i >= startIndex; i--)
                {
                    removedList.Add(ChildEquations[i]);
                    ChildEquations.RemoveAt(i);
                }
                removedList.Reverse();
            }
            ActiveChild = ChildEquations[startIndex];
            return removedList;
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var otherIndex = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
                var firstEquation = (TextEquation)ChildEquations[startIndex];
                var lastEquation = (TextEquation)ChildEquations[otherIndex];
                var equations = new List<IEquationBase>();
                var action = new RowRemoveAction(this)
                                         {
                                             ActiveEquation = ActiveChild,
                                             HeadTextEquation = firstEquation,
                                             TailTextEquation = lastEquation,
                                             SelectionStartIndex = SelectionStartIndex,
                                             SelectedItems = SelectedItems,
                                             FirstTextCaretIndex = firstEquation.CaretIndex,
                                             LastTextCaretIndex = lastEquation.CaretIndex,
                                             FirstTextSelectionIndex = firstEquation.SelectionStartIndex,
                                             LastTextSelectionIndex = lastEquation.SelectionStartIndex,
                                             FirstTextSelectedItems = firstEquation.SelectedItems,
                                             LastTextSelectedItems = lastEquation.SelectedItems,
                                             FirstText = firstEquation.Text,
                                             LastText = lastEquation.Text,
                                             FirstFormats = firstEquation.GetFormats(),
                                             LastFormats = lastEquation.GetFormats(),
                                             FirstModes = firstEquation.GetModes(),
                                             LastModes = lastEquation.GetModes(),
                                             FirstDecorations = firstEquation.GetDecorations(),
                                             LastDecorations = lastEquation.GetDecorations(),
                                             Equations = equations
                                         };
                firstEquation.RemoveSelection(false);
                lastEquation.RemoveSelection(false);
                firstEquation.Merge(lastEquation);
                for (var i = otherIndex; i > startIndex; i--)
                {
                    equations.Add(ChildEquations[i]);
                    ChildEquations.RemoveAt(i);
                }
                SelectedItems = 0;
                equations.Reverse();
                ActiveChild = firstEquation;
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

        public override bool Select(Key key)
        {
            if (key == Key.Left)
            {
                return HandleLeftSelect(key);
            }
            else if (key == Key.Right)
            {
                return HandleRightSelect(key);
            }
            return false;
        }
        private bool HandleRightSelect(Key key)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild == ChildEquations.Last())
                {
                    return false;
                }
                else
                {
                    SelectedItems += 2;
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 2];
                    ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1].DeSelect();
                    if (SelectedItems > 0)
                    {
                        ((TextEquation)ActiveChild).MoveToStart();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else
            {
                if (!ActiveChild.Select(key))
                {
                    var previsouText = (TextEquation)ChildEquations[SelectionStartIndex - 1];
                    var nextText = (TextEquation)ChildEquations[SelectionStartIndex + 1];
                    previsouText.MoveToEnd();
                    previsouText.StartSelection();
                    nextText.MoveToStart();
                    nextText.StartSelection();
                    SelectionStartIndex--;
                    SelectedItems += 2;
                    ActiveChild = nextText;
                }
                return true;
            }
        }
        private bool HandleLeftSelect(Key key)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild == ChildEquations.First())
                {
                    return false;
                }
                else
                {
                    SelectedItems -= 2;
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 2];
                    ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1].DeSelect();
                    if (SelectedItems < 0)
                    {
                        ((TextEquation)ActiveChild).MoveToEnd();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else
            {
                if (!ActiveChild.Select(key))
                {
                    var previsouText = (TextEquation)ChildEquations[SelectionStartIndex - 1];
                    var nextText = (TextEquation)ChildEquations[SelectionStartIndex + 1];
                    previsouText.MoveToEnd();
                    previsouText.StartSelection();
                    nextText.MoveToStart();
                    nextText.StartSelection();
                    SelectionStartIndex++;
                    SelectedItems -= 2;
                    ActiveChild = previsouText;
                }
                return true;
            }
        }

        public override Rect GetSelectionBounds()
        {
            try
            {
                if (IsSelecting)
                {
                    var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                    var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                    var firstRect = ChildEquations[startIndex].GetSelectionBounds();
                    if (firstRect == Rect.Empty)
                    {
                        firstRect = new Rect(ChildEquations[startIndex].Right, ChildEquations[startIndex].Top, 0, 0);
                    }
                    if (count > 0)
                    {
                        var lastRect = ChildEquations[count + startIndex].GetSelectionBounds();
                        if (lastRect == Rect.Empty)
                        {
                            lastRect = new Rect(ChildEquations[count + startIndex].Left, ChildEquations[count + startIndex].Top, 0, ChildEquations[count + startIndex].Height);
                        }
                        for (var i = startIndex + 1; i < startIndex + count; i++)
                        {
                            var equation = ChildEquations[i];
                            lastRect.Union(equation.Bounds);
                        }
                        firstRect.Union(lastRect);
                    }
                    return new Rect(firstRect.TopLeft, firstRect.BottomRight);
                }
            }
            catch
            {

            }
            return Rect.Empty;
        }

        public override void Paste(XElement xe)
        {
            if (ActiveChild.GetType() == typeof(TextEquation) && xe.Name.LocalName == GetType().Name)
            {
                var children = xe.Element("ChildEquations");
                var newChildren = new List<IEquationBase>();
                foreach (var xElement in children.Elements())
                {
                    newChildren.Add(CreateChild(xElement));
                }
                if (newChildren.Count > 0)
                {
                    var action = new EquationRowPasteAction(this)
                    {
                        ActiveTextEquation = (TextEquation)ActiveChild,
                        ActiveChildCaretIndex = ((TextEquation)ActiveChild).CaretIndex,
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        ActiveChildSelectedItems = ActiveChild.SelectedItems,
                        ActiveChildSelectionStartIndex = ActiveChild.SelectionStartIndex,
                        ActiveChildText = ((TextEquation)ActiveChild).Text,
                        ActiveChildFormats = ((TextEquation)ActiveChild).GetFormats(),
                        ActiveChildModes = ((TextEquation)ActiveChild).GetModes(),
                        ActiveChildDecorations = ((TextEquation)ActiveChild).GetDecorations(),
                        FirstNewText = ((TextEquation)newChildren.First()).Text,
                        LastNewText = ((TextEquation)newChildren.Last()).Text,
                        FirstNewFormats = ((TextEquation)newChildren.First()).GetFormats(),
                        LastNewFormats = ((TextEquation)newChildren.Last()).GetFormats(),
                        FirstNewModes = ((TextEquation)newChildren.First()).GetModes(),
                        LastNewModes = ((TextEquation)newChildren.Last()).GetModes(),                                                
                        FirstNewDecorations = ((TextEquation)newChildren.First()).GetDecorations(),
                        LastNewDecorations = ((TextEquation)newChildren.Last()).GetDecorations(),
                        Equations = newChildren
                    };
                    var newChild = ActiveChild.Split(this);
                    var index = ChildEquations.IndexOf(ActiveChild) + 1;
                    newChildren.RemoveAt(0);
                    ChildEquations.InsertRange(index, newChildren);
                    ((TextEquation)ActiveChild).ConsumeFormattedText(action.FirstNewText, action.FirstNewFormats, action.FirstNewModes, action.FirstNewDecorations, false);
                    ((TextEquation)newChildren.Last()).Merge((TextEquation)newChild);
                    ActiveChild = newChildren.Last();
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.Paste(xe);
            }
        }

        public override void DeSelect()
        {
            base.DeSelect();
            deleteable = null;
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                var firstText = ((TextEquation)ChildEquations[startIndex]).GetSelectedText();
                var lastText = ((TextEquation)ChildEquations[startIndex + count]).GetSelectedText();
                var firstFormats = ((TextEquation)ChildEquations[startIndex]).GetSelectedFormats();
                var firstModes = ((TextEquation)ChildEquations[startIndex]).GetSelectedModes();
                var firstDecorations = ((TextEquation)ChildEquations[startIndex]).GetSelectedDecorations();
                var lastFormats = ((TextEquation)ChildEquations[startIndex + count]).GetSelectedFormats();
                var lastModes = ((TextEquation)ChildEquations[startIndex + count]).GetSelectedModes();
                var lastDecorations = ((TextEquation)ChildEquations[startIndex + count]).GetSelectedDecorations();
                var firstEquation = new TextEquation(this);
                var lastEquation = new TextEquation(this);
                firstEquation.ConsumeFormattedText(firstText, firstFormats, firstModes, firstDecorations, false);
                lastEquation.ConsumeFormattedText(lastText, lastFormats, lastModes, lastDecorations, false);

                var equations = new List<IEquationBase>();
                equations.Add(firstEquation);
                for (var i = startIndex + 1; i < startIndex + count; i++)
                {
                    equations.Add(ChildEquations[i]);
                }
                equations.Add(lastEquation);

                double left = 0;
                foreach (var eb in equations)
                {
                    eb.Left = 1 + left;
                    left += eb.Width;
                }
                double maxUpperHalf = 0;
                double maxBottomHalf = 0;
                foreach (EquationBase eb in ChildEquations)
                {
                    if (eb.RefY > maxUpperHalf) { maxUpperHalf = eb.RefY; }
                    if (eb.Height - eb.RefY > maxBottomHalf) { maxBottomHalf = eb.Height - eb.RefY; }
                }
                double width = 0;
                foreach (var eb in equations)
                {
                    eb.Top = 1 + maxUpperHalf - eb.RefY;
                    width += eb.Width;
                }
                var rect = GetSelectionBounds();
                var bitmap = new RenderTargetBitmap((int)(Math.Ceiling(width + 2)), (int)(Math.Ceiling(maxUpperHalf + maxBottomHalf + 2)), 96, 96, PixelFormats.Default);
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
                var children = new XElement("ChildEquations");
                foreach (var eb in equations)
                {
                    eb.SelectAll();
                    children.Add(eb.Serialize());
                }
                thisElement.Add(children);
                //data.SetText(GetSelectedText());
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

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            if (deleteable != null)
            {
                Brush brush = new SolidColorBrush(Colors.Gray);
                brush.Opacity = 0.5;
                dc.DrawRectangle(brush, null, new Rect(deleteable.Location, deleteable.Size));
            }
            if (ChildEquations.Count == 1)
            {
                var firstEquation = (TextEquation)ChildEquations.First();
                if (firstEquation.TextLength == 0)
                {
                    if (IsSelecting)
                    {
                        //dc.DrawRectangle(Brushes.LightGray, null, new Rect(new Point(Left - 1, Top), new Size(FontSize / 2.5, Height)));
                    }
                    dc.DrawRectangle(null, boxPen, new Rect(Left, Top, Width, Height + ThinLineThickness));//new Rect(new Point(Left - 1, Top), new Size(FontSize / 2.5, Height)));
                }
            }
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("ChildEquations");
            foreach (EquationBase childEquation in ChildEquations)
            {
                children.Add(childEquation.Serialize());
            }
            thisElement.Add(children);
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var children = xElement.Element("ChildEquations");
            ChildEquations.Clear();
            foreach (var xe in children.Elements())
            {
                ChildEquations.Add(CreateChild(xe));
            }
            if (ChildEquations.Count == 0)
            {
                ChildEquations.Add(new TextEquation(this));
            }
            ActiveChild = ChildEquations.First();
            CalculateSize();
        }

        EquationBase CreateChild(XElement xElement)
        {
            var type = Type.GetType(GetType().Namespace + "." + xElement.Name);
            var paramz = new List<object>();
            paramz.Add(this);
            var parameters = xElement.Element("parameters");
            if (parameters != null)
            {
                foreach (var xe in parameters.Elements())
                {
                    var paramType = Type.GetType(GetType().Namespace + "." + xe.Name);
                    if (paramType == null)
                    {
                        paramType = Type.GetType(xe.Name.ToString());
                    }
                    if (paramType.IsEnum)
                    {
                        paramz.Add((Enum.Parse(paramType, xe.Value)));
                    }
                    else if (paramType == typeof(bool))
                    {
                        paramz.Add(bool.Parse(xe.Value));
                    }
                    else if (paramType == typeof(int))
                    {
                        paramz.Add(int.Parse(xe.Value));
                    }
                    else
                    {
                        paramz.Add(xe.Value);
                    }
                }
            }
            var child = (EquationBase)Activator.CreateInstance(type, paramz.ToArray());
            child.DeSerialize(xElement);
            child.FontSize = FontSize;
            return child;
        }

        public static bool UseItalicIntergalOnNew { get; set; }

        public override void ExecuteCommand(CommandType commandType, object data)
        {
            deleteable = null;
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                IEquationBase newEquation = null;
                switch (commandType)
                {
                    case CommandType.Composite:
                        newEquation = CompositeFactory.CreateEquation(this, (Position)data);
                        break;
                    case CommandType.CompositeBig:
                        newEquation = BigCompositeFactory.CreateEquation(this, (Position)data);
                        break;
                    case CommandType.Division:
                        newEquation = DivisionFactory.CreateEquation(this, (DivisionType)data);
                        break;
                    case CommandType.SquareRoot:
                        newEquation = new SquareRoot(this);
                        break;
                    case CommandType.NRoot:
                        newEquation = new nRoot(this);
                        break;
                    case CommandType.LeftBracket:
                        newEquation = new LeftBracket(this, (BracketSignType)data);
                        break;
                    case CommandType.RightBracket:
                        newEquation = new RightBracket(this, (BracketSignType)data);
                        break;
                    case CommandType.LeftRightBracket:
                        newEquation = new LeftRightBracket(this, ((BracketSignType[])data)[0], ((BracketSignType[])data)[1]);
                        break;
                    case CommandType.Sub:
                        newEquation = new Sub(this, (Position)data);
                        break;
                    case CommandType.Super:
                        newEquation = new Super(this, (Position)data);
                        break;
                    case CommandType.SubAndSuper:
                        newEquation = new SubAndSuper(this, (Position)data);
                        break;
                    case CommandType.TopBracket:
                        newEquation = new TopBracket(this, (HorizontalBracketSignType)data);
                        break;
                    case CommandType.BottomBracket:
                        newEquation = new BottomBracket(this, (HorizontalBracketSignType)data);
                        break;
                    case CommandType.DoubleArrowBarBracket:
                        newEquation = new DoubleArrowBarBracket(this);
                        break;
                    case CommandType.SignComposite:
                        newEquation = SignCompositeFactory.CreateEquation(this, (Position)(((object[])data)[0]), (SignCompositeSymbol)(((object[])data)[1]), UseItalicIntergalOnNew);
                        break;
                    case CommandType.Decorated:
                        newEquation = new Decorated(this, (DecorationType)(((object[])data)[0]), (Position)(((object[])data)[1]));
                        break;
                    case CommandType.Arrow:
                        newEquation = new Arrow(this, (ArrowType)(((object[])data)[0]), (Position)(((object[])data)[1]));
                        break;
                    case CommandType.Box:
                        newEquation = new Box(this, (BoxType)data);
                        break;
                    case CommandType.Matrix:
                        newEquation = new MatrixEquation(this, ((int[])data)[0], ((int[])data)[1]);
                        break;
                    case CommandType.DecoratedCharacter:
                        if (((TextEquation)ActiveChild).CaretIndex > 0)
                        {
                            ((TextEquation)ActiveChild).AddDecoration((CharacterDecorationType)((object[])data)[0],
                                                                      (Position)((object[])data)[1],
                                                                      (string)((object[])data)[2]);
                            CalculateSize();
                        }
                        break;
                }
                if (newEquation != null)
                {
                    var newText = ActiveChild.Split(this);
                    var caretIndex = ((TextEquation)ActiveChild).TextLength;
                    AddChild(newEquation);
                    AddChild(newText);                    
                    newEquation.CalculateSize();
                    ActiveChild = newEquation;
                    CalculateSize();                    
                    UndoManager.AddUndoAction(new RowAction(this, ActiveChild, (TextEquation)newText, ChildEquations.IndexOf(ActiveChild), caretIndex));
                }
            }
            else if (ActiveChild != null)
            {
                ((EquationContainer)ActiveChild).ExecuteCommand(commandType, data);
                CalculateSize();
            }
        }

        private void AddChild(IEquationBase newChild)
        {
            var index = 0;
            if (ChildEquations.Count > 0)
            {
                index = ChildEquations.IndexOf(ActiveChild) + 1;
            }
            ChildEquations.Insert(index, newChild);
            newChild.ParentEquation = this;
            ActiveChild = newChild;
        }

        private void RemoveChild(IEquationBase child)
        {
            ChildEquations.Remove(child);
            CalculateSize();
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (mousePoint.X < ActiveChild.Left)
            {
                HandleLeftDrag(mousePoint);
            }
            else if (mousePoint.X > ActiveChild.Right)
            {
                HandleRightDrag(mousePoint);
            }
            else
            {
                ActiveChild.HandleMouseDrag(mousePoint);
            }
            SelectedItems = ChildEquations.IndexOf(ActiveChild) - SelectionStartIndex;
        }

        private void HandleRightDrag(Point mousePoint)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                ((TextEquation)ActiveChild).SelectToEnd();
                if (ActiveChild != ChildEquations.Last())
                {
                    if (mousePoint.X > ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1].MidX)
                    {
                        ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1].SelectAll();
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 2];
                        //childEquations[childEquations.IndexOf(ActiveChild) - 1].DeSelect();
                        if (ChildEquations.IndexOf(ActiveChild) > SelectionStartIndex) // old-> (SelectedItems > 0)
                        {
                            ((TextEquation)ActiveChild).MoveToStart();
                            ActiveChild.StartSelection();
                            ActiveChild.HandleMouseDrag(mousePoint);
                        }
                    }
                }
            }
            else
            {
                var previsouText = (TextEquation)ChildEquations[SelectionStartIndex - 1];
                var nextText = (TextEquation)ChildEquations[SelectionStartIndex + 1];
                previsouText.MoveToEnd();
                previsouText.StartSelection();
                nextText.MoveToStart();
                nextText.StartSelection();
                SelectionStartIndex--;
                ActiveChild = nextText;
                ActiveChild.HandleMouseDrag(mousePoint);
            }
        }

        private void HandleLeftDrag(Point mousePoint)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                ((TextEquation)ActiveChild).SelectToStart();
                if (ActiveChild != ChildEquations.First())
                {
                    if (mousePoint.X < ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1].MidX)
                    {
                        ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1].SelectAll();
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 2];
                        //childEquations[childEquations.IndexOf(ActiveChild) + 1].DeSelect();
                        if (ChildEquations.IndexOf(ActiveChild) < SelectionStartIndex)      // old -> (SelectedItems < 0)
                        {
                            ((TextEquation)ActiveChild).MoveToEnd();
                            ActiveChild.StartSelection();
                            ActiveChild.HandleMouseDrag(mousePoint);
                        }
                    }
                }
            }
            else
            {
                var previsouText = (TextEquation)ChildEquations[SelectionStartIndex - 1];
                var nextText = (TextEquation)ChildEquations[SelectionStartIndex + 1];
                previsouText.MoveToEnd();
                previsouText.StartSelection();
                nextText.MoveToStart();
                nextText.StartSelection();
                SelectionStartIndex++;
                ActiveChild = previsouText;
                ActiveChild.HandleMouseDrag(mousePoint);
            }
        }

        public override void SetCursorOnKeyUpDown(Key key, Point point)
        {
            foreach (EquationBase eb in ChildEquations)
            {
                if (eb.Right >= point.X)
                {
                    eb.SetCursorOnKeyUpDown(key, point);
                    ActiveChild = eb;
                    break;
                }
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            deleteable = null;
            ActiveChild = null;
            foreach (EquationBase eb in ChildEquations)
            {
                if (eb.Right >= mousePoint.X && eb.Left <= mousePoint.X)
                {
                    ActiveChild = eb;
                    break;
                }
            }
            if (ActiveChild == null)
            {
                if (mousePoint.X <= MidX)
                    ActiveChild = ChildEquations.First();
                else
                    ActiveChild = ChildEquations.Last();
            }
            if (!ActiveChild.ConsumeMouseClick(mousePoint))
            {
                var moveToStart = true;
                if (ChildEquations.Count == 1)
                {
                    if (ActiveChild.MidX < mousePoint.X)
                    {
                        moveToStart = false;
                    }
                }
                else if (mousePoint.X < ActiveChild.MidX)
                {
                    if (ActiveChild != ChildEquations.First())
                    {
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                        moveToStart = false;
                    }
                }
                else if (ActiveChild != ChildEquations.Last())
                {
                    ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                }
                else
                {
                    moveToStart = false;
                }
                var equation = ActiveChild as TextEquation;
                if (equation != null)
                {
                    if (moveToStart)
                    {
                        equation.MoveToStart();
                    }
                    else
                    {
                        equation.MoveToEnd();
                    }
                }
            }
            return true;
        }

        public override bool ConsumeKey(Key key)
        {
            var result = false;
            if (key == Key.Home)
            {
                ActiveChild = ChildEquations.First();
            }
            else if (key == Key.End)
            {
                ActiveChild = ChildEquations.Last();
            }
            if (ActiveChild.ConsumeKey(key))
            {
                deleteable = null;
                result = true;
            }
            else if (key == Key.Delete)
            {
                if (ActiveChild.GetType() == typeof(TextEquation) && ActiveChild != ChildEquations.Last())
                {
                    if (ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1] == deleteable)
                    {
                        UndoManager.AddUndoAction(new RowAction(this, deleteable, (TextEquation)ChildEquations[ChildEquations.IndexOf(deleteable) + 1], 
                                                                ChildEquations.IndexOf(deleteable), TextLength) { UndoFlag = false});
                        ChildEquations.Remove(deleteable);
                        deleteable = null;
                        ((TextEquation)ActiveChild).Merge((TextEquation)ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1]);
                        ChildEquations.Remove(ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1]);
                    }
                    else
                    {
                        deleteable = (EquationContainer)ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                    }
                    result = true;
                }
            }
            else if (key == Key.Back)
            {
                if (ActiveChild.GetType() == typeof(TextEquation))
                {
                    if (ActiveChild != ChildEquations.First())
                    {
                        if ((EquationContainer)ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1] == deleteable)
                        {
                            var equationAfter = (TextEquation)ActiveChild;
                            ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 2];
                            UndoManager.AddUndoAction(new RowAction(this, deleteable, equationAfter, ChildEquations.IndexOf(deleteable), TextLength) { UndoFlag = false });
                            ChildEquations.Remove(deleteable);
                            ((TextEquation)ActiveChild).Merge(equationAfter);
                            ChildEquations.Remove(equationAfter);
                            deleteable = null;
                        }
                        else
                        {
                            deleteable = (EquationContainer)ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                        }
                        result = true;
                    }
                }
                else
                {
                    if (deleteable == ActiveChild)
                    {
                        var equationAfter = (TextEquation)ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                        UndoManager.AddUndoAction(new RowAction(this, deleteable, equationAfter, ChildEquations.IndexOf(deleteable), TextLength) { UndoFlag = false });
                        ChildEquations.Remove(deleteable);
                        ((TextEquation)ActiveChild).Merge(equationAfter);
                        ChildEquations.Remove(equationAfter);
                        deleteable = null;
                    }
                    else
                    {
                        deleteable = (EquationContainer)ActiveChild;
                    }
                    result = true;
                }
            }
            if (!result)
            {
                deleteable = null;
                if (key == Key.Right)
                {
                    if (ActiveChild != ChildEquations.Last())
                    {
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) + 1];
                        result = true;
                    }
                }
                else if (key == Key.Left)
                {
                    if (ActiveChild != ChildEquations.First())
                    {
                        ActiveChild = ChildEquations[ChildEquations.IndexOf(ActiveChild) - 1];
                        result = true;
                    }
                }
            }
            CalculateSize();
            return result;
        }

        public void Merge(EquationRow secondLine)
        {
            ((TextEquation)ChildEquations.Last()).Merge((TextEquation)secondLine.ChildEquations.First()); //first and last are always of tyep TextEquation
            for (var i = 1; i < secondLine.ChildEquations.Count; i++)
            {
                AddChild(secondLine.ChildEquations[i]);
            }
            CalculateSize();
        }

        void SplitRow(EquationRow newRow)
        {
            var index = ChildEquations.IndexOf(ActiveChild) + 1;
            var newChild = ActiveChild.Split(newRow);

            if (newChild != null)
            {
                newRow.RemoveChild(newRow.ActiveChild);
                newRow.AddChild(newChild);
                var i = index;
                for (; i < ChildEquations.Count; i++)
                {
                    newRow.AddChild(ChildEquations[i]);
                }
                for (i = ChildEquations.Count - 1; i >= index; i--)
                {
                    RemoveChild(ChildEquations[i]);
                }
            }
        }

        public override IEquationBase Split(EquationContainer newParent)
        {
            deleteable = null;
            EquationRow newRow = null;
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                newRow = new EquationRow(newParent);
                SplitRow(newRow);
                newRow.CalculateSize();
            }
            else
            {
                ActiveChild.Split(this);
            }
            CalculateSize();
            return newRow;
        }

        public void Truncate()
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                deleteable = null;
                ((TextEquation)ActiveChild).Truncate();
                var index = ChildEquations.IndexOf(ActiveChild) + 1;
                var i = index;
                for (i = ChildEquations.Count - 1; i >= index; i--)
                {
                    RemoveChild(ChildEquations[i]);
                }
            }
            CalculateSize();
        }

        public override void CalculateWidth()
        {
            double width = 0;
            foreach (var eb in ChildEquations)
            {
                width += eb.Width + eb.Margin.Left + eb.Margin.Right;
            }
            if (ChildEquations.Count > 1)
            {
                width -= ChildEquations.Last().Width == 0 ? ChildEquations[ChildEquations.Count - 2].Margin.Right : 0;
                width -= ChildEquations.First().Width == 0 ? ChildEquations[1].Margin.Left : 0;
            }
            Width = width;
        }

        public override void CalculateHeight()
        {
            double maxUpperHalf = 0;
            double maxBottomHalf = 0;
            foreach (var eb in ChildEquations)
            {
                if (eb.GetType() == typeof(Super) || eb.GetType() == typeof(Sub) || eb.GetType() == typeof(SubAndSuper))
                {
                    var subSuperBase = (SubSuperBase)eb;
                    subSuperBase.SetBuddy(subSuperBase.Position == Position.Right
                        ? PreviousNonEmptyChild(subSuperBase)
                        : NextNonEmptyChild(subSuperBase));
                }
                var childRefY = eb.RefY;
                var childHeight = eb.Height;
                if (childRefY > maxUpperHalf)
                {
                    maxUpperHalf = childRefY;
                }
                if (childHeight - childRefY > maxBottomHalf)
                {
                    maxBottomHalf = childHeight - childRefY;
                }
            }
            Height = maxUpperHalf + maxBottomHalf;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                double left = 0;
                for (var i = 0; i < ChildEquations.Count; i++)
                {
                    ChildEquations[i].Left = left + value + (left == 0 && i == 1 ? 0 : ChildEquations[i].Margin.Left);
                    left += ChildEquations[i].Width + ChildEquations[i].Margin.Right + (left == 0 && i == 1 ? 0 : ChildEquations[i].Margin.Left);
                }
            }
        }

        public override double RefY => ChildEquations.First().MidY - Top;

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                double maxUpperHalf = 0;
                foreach (var equationBase in ChildEquations)
                {
                    var eb = (EquationBase) equationBase;
                    maxUpperHalf = Math.Max(maxUpperHalf, eb.RefY);
                }
                foreach (var eb in ChildEquations)
                {
                    eb.Top = (Top + maxUpperHalf) - eb.RefY;
                }
            }
        }

        private void AdjustChildrenVertical(double maxUpperHalf)
        {
            foreach (var eb in ChildEquations)
            {
                eb.Top = (Top + maxUpperHalf) - eb.RefY;
            }
        }

        public override double Width
        {
            get { return base.Width; }
            set
            {
                if (value > 0)
                {
                    base.Width = value;
                }
                else
                {
                    base.Width = FontSize / 2;
                }
            }
        }

        public void ProcessUndo(EquationAction action)
        {
            deleteable = null;
            if (action.GetType() == typeof(RowAction))
            {
                ProcessRowAction(action);
                IsSelecting = false;
            }
            else if (action.GetType() == typeof(EquationRowPasteAction))
            {
                ProcessRowPasteAction(action);
            }
            else if (action.GetType() == typeof(EquationRowFormatAction))
            {
                ProcessEquationRowFormatAction(action);
            }
            else
            {
                ProcessRowRemoveAction(action);
            }
            CalculateSize();
            if (ParentEquation != null)
            {
                ParentEquation.ChildCompletedUndo(this);
            }
        }

        public void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems, List<IEquationBase> items, bool appendAtEnd)
        {
            SelectionStartIndex = selectionStartIndex;
            SelectedItems = selectedItems;
            var index = 0;
            if (appendAtEnd)
            {
                index = ChildEquations.Count;
            }
            for (var i = 0; i < items.Count; i++)
            {
                ChildEquations.Insert(i + index, items[i]);
            }
            ActiveChild = ChildEquations[activeChildIndex];
        }

        public void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems)
        {
            SelectionStartIndex = selectionStartIndex;
            SelectedItems = selectedItems;
            ActiveChild = ChildEquations[activeChildIndex];
        }

        public void ResetRowEquation(EquationBase activeChild, int selectionStartIndex, int selectedItems)
        {
            SelectionStartIndex = selectionStartIndex;
            SelectedItems = selectedItems;
            ActiveChild = activeChild;
        }

        private void ProcessRowRemoveAction(EquationAction action)
        {
            var rowAction = action as RowRemoveAction;
            rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex,
                                                         rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats, 
                                                         rowAction.FirstModes, rowAction.FirstDecorations);
            rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex,
                                                         rowAction.LastTextSelectedItems, rowAction.LastText, 
                                                         rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
            if (rowAction.UndoFlag)
            {
                ChildEquations.InsertRange(ChildEquations.IndexOf(rowAction.HeadTextEquation) + 1, rowAction.Equations);
                ActiveChild = rowAction.ActiveEquation;
                foreach (EquationBase eb in rowAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                SelectedItems = rowAction.SelectedItems;
                SelectionStartIndex = rowAction.SelectionStartIndex;
                IsSelecting = true;                               
            }
            else
            {
                rowAction.HeadTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.TailTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.HeadTextEquation.Merge(rowAction.TailTextEquation);
                var index = ChildEquations.IndexOf(rowAction.HeadTextEquation);
                for (var i = index + rowAction.Equations.Count; i > index; i--)
                {
                    ChildEquations.RemoveAt(i);
                }
                ActiveChild = rowAction.HeadTextEquation;
                SelectedItems = 0;
                IsSelecting = false; 
            }
        }

        private void ProcessRowPasteAction(EquationAction action)
        {
            var pasteAction = action as EquationRowPasteAction;
            var activeText = pasteAction.ActiveTextEquation;
            activeText.ResetTextEquation(pasteAction.ActiveChildCaretIndex, pasteAction.ActiveChildSelectionStartIndex, pasteAction.ActiveChildSelectedItems,
                                         pasteAction.ActiveChildText, pasteAction.ActiveChildFormats, pasteAction.ActiveChildModes, pasteAction.ActiveChildDecorations);
            ActiveChild = activeText;
            if (pasteAction.UndoFlag)
            {
                SelectedItems = pasteAction.SelectedItems;
                SelectionStartIndex = pasteAction.SelectionStartIndex;
                foreach (var eb in pasteAction.Equations)
                {
                    ChildEquations.Remove(eb);
                }
            }
            else
            {
                ((TextEquation)pasteAction.Equations.Last()).ResetTextEquation(0, 0, 0, pasteAction.LastNewText, pasteAction.LastNewFormats, pasteAction.LastNewModes, pasteAction.LastNewDecorations);
                var newChild = ActiveChild.Split(this);
                var index = ChildEquations.IndexOf(ActiveChild) + 1;
                ChildEquations.InsertRange(index, pasteAction.Equations);
                ((TextEquation)ActiveChild).ConsumeFormattedText(pasteAction.FirstNewText, pasteAction.FirstNewFormats, pasteAction.FirstNewModes, pasteAction.FirstNewDecorations, false);
                ((TextEquation)pasteAction.Equations.Last()).Merge((TextEquation)newChild);
                ActiveChild = ChildEquations[index + pasteAction.Equations.Count - 1];
                foreach (var eb in pasteAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                SelectedItems = 0;
            }
        }

        private void ProcessRowAction(EquationAction action)
        {
            var rowAction = action as RowAction;
            if (rowAction.UndoFlag)
            {
                ChildEquations.Remove(rowAction.Equation);
                ActiveChild = ChildEquations.ElementAt(rowAction.Index - 1);
                ((TextEquation)ActiveChild).Merge(rowAction.EquationAfter);
                ChildEquations.RemoveAt(rowAction.Index);
            }
            else
            {
                ActiveChild = ChildEquations[rowAction.Index - 1];
                ((TextEquation)ActiveChild).Truncate(rowAction.CaretIndex);
                ChildEquations.Insert(rowAction.Index, rowAction.Equation);
                ChildEquations.Insert(rowAction.Index + 1, rowAction.EquationAfter);
                ActiveChild = rowAction.Equation;
                rowAction.Equation.FontSize = FontSize;
                rowAction.EquationAfter.FontSize = FontSize;
            }
        }

        private void ProcessEquationRowFormatAction(EquationAction action)
        {
            var ecfa = action as EquationRowFormatAction;
            if (ecfa != null)
            {
                IsSelecting = true;
                SelectedItems = ecfa.SelectedItems;
                SelectionStartIndex = ecfa.SelectionStartIndex;
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                ChildEquations[startIndex].SelectionStartIndex = ecfa.FirstChildSelectionStartIndex;
                ChildEquations[startIndex].SelectedItems = ecfa.FirstChildSelectedItems;
                ChildEquations[endIndex].SelectionStartIndex = ecfa.LastChildSelectionStartIndex;
                ChildEquations[endIndex].SelectedItems = ecfa.LastChildSelectedItems;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    if (i > startIndex && i < endIndex)
                    {
                        ChildEquations[i].SelectAll();
                    }
                    ChildEquations[i].ModifySelection(ecfa.Operation, ecfa.Argument, ecfa.UndoFlag ? !ecfa.Applied : ecfa.Applied, false);
                }
                CalculateSize();
                ParentEquation.ChildCompletedUndo(this);
            }
        }

        public void Truncate(int indexFrom, int keepCount)
        {
            ChildEquations.RemoveRange(indexFrom, ChildEquations.Count - indexFrom);
            ((TextEquation)ChildEquations[indexFrom - 1]).Truncate(keepCount);
            CalculateSize();
        }

        public void SetCurrentChild(int childIndex, int caretIndex)
        {
            var textEquation = ChildEquations[childIndex] as TextEquation;
            textEquation.CaretIndex = caretIndex;
            ActiveChild = textEquation;
        }

        public bool IsEmpty => 
            ChildEquations.Count == 1 
            && string.IsNullOrEmpty(((TextEquation)ActiveChild).Text);

        public int ActiveChildIndex
        {
            get
            {
                return ChildEquations.IndexOf(ActiveChild);
            }
            set
            {
                ActiveChild = ChildEquations[value];
            }
        }

        public int TextLength => ((TextEquation)ActiveChild).TextLength;

        public void SelectToStart()
        {
            if (ChildEquations[SelectionStartIndex].GetType() == typeof(TextEquation))
            {
                ((TextEquation)ChildEquations[SelectionStartIndex]).SelectToStart();
            }
            else
            {
                SelectionStartIndex++;
                ((TextEquation)ChildEquations[SelectionStartIndex]).MoveToStart();
                ChildEquations[SelectionStartIndex].StartSelection();
            }
            for (var i = SelectionStartIndex - 2; i >= 0; i -= 2)
            {
                ((TextEquation)ChildEquations[i]).MoveToEnd();
                ChildEquations[i].StartSelection();
                ((TextEquation)ChildEquations[i]).SelectToStart();
                ChildEquations[i + 1].SelectAll();
            }
            SelectedItems = -SelectionStartIndex;
            ActiveChild = ChildEquations[0];
        }

        public void SelectToEnd()
        {
            if (ChildEquations[SelectionStartIndex].GetType() == typeof(TextEquation))
            {
                ((TextEquation)ChildEquations[SelectionStartIndex]).SelectToEnd();
            }
            else
            {
                SelectionStartIndex--;
                ((TextEquation)ChildEquations[SelectionStartIndex]).MoveToEnd();
                ChildEquations[SelectionStartIndex].StartSelection();
            }
            for (var i = SelectionStartIndex + 2; i < ChildEquations.Count; i += 2)
            {
                ((TextEquation)ChildEquations[i]).MoveToStart();
                ChildEquations[i].StartSelection();
                ((TextEquation)ChildEquations[i]).SelectToEnd();
                ChildEquations[i - 1].SelectAll();
            }
            SelectedItems = ChildEquations.Count - SelectionStartIndex - 1;
            ActiveChild = ChildEquations.Last();
        }

        public IEquationBase PreviousNonEmptyChild(EquationContainer equation)
        {
            var index = ChildEquations.IndexOf(equation) - 1;
            if (index >= 0)
            {
                if (index >= 1 && ((TextEquation)ChildEquations[index]).TextLength == 0)
                {
                    index--;
                }
                return ChildEquations[index];
            }
            else
            {
                return null;
            }
        }

        public IEquationBase NextNonEmptyChild(EquationContainer equation)
        {
            var index = ChildEquations.IndexOf(equation) + 1;
            if (index >= ChildEquations.Count) return null;

            if (index < ChildEquations.Count - 1 && ((TextEquation)ChildEquations[index]).TextLength == 0)
            {
                index++;
            }

            return ChildEquations[index];
        }

        public override void SelectAll()
        {
            base.SelectAll();
            ((TextEquation)ChildEquations.Last()).MoveToEnd();
        }

        public void MoveToStart()
        {
            ActiveChild = ChildEquations[0];
            ((TextEquation)ActiveChild).MoveToStart();
        }

        public void MoveToEnd()
        {
            ActiveChild = ChildEquations.Last();
            ((TextEquation)ActiveChild).MoveToEnd();
        }

        public override double GetVerticalCaretLength()
        {
            return ActiveChild.GetType() == typeof(TextEquation) 
                ? Height 
                : ActiveChild.GetVerticalCaretLength();
        }

        public override Point GetVerticalCaretLocation()
        {
            return ActiveChild.GetType() == typeof(TextEquation) 
                ? new Point(ActiveChild.GetVerticalCaretLocation().X, Top) 
                : ActiveChild.GetVerticalCaretLocation();
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
                        var ecfa = new EquationRowFormatAction(this)
                        {
                            Operation = operation,
                            Argument = argument,
                            Applied = applied,
                            SelectionStartIndex = SelectionStartIndex,
                            SelectedItems = SelectedItems,
                            FirstChildSelectionStartIndex = ChildEquations[startIndex].SelectionStartIndex,
                            FirstChildSelectedItems = ChildEquations[startIndex].SelectedItems,
                            LastChildSelectionStartIndex = ChildEquations[endIndex].SelectionStartIndex,
                            LastChildSelectedItems = ChildEquations[endIndex].SelectedItems,
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
    }
}
