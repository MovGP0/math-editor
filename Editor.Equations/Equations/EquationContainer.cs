﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    public abstract class EquationContainer : EquationBase, IEquationContainer
    {
        public int GetIndex(IEquationBase child)
        {
            return ChildEquations.IndexOf(child);
        }

        public void ReleaseChild(IEquationBase child)
        {
            if (ChildEquations.Contains(child))
            {
                ChildEquations.Remove(child);
            }
        }

        public List<IEquationBase> ChildEquations { get; } = new List<IEquationBase>();
        IEquationBase active;
        public IEquationBase ActiveChild
        {
            get { return active; }
            set
            {
                if (value == null || !value.IsStatic)
                {
                    active = value;
                }
            }
        }

        protected EquationContainer(IEquationContainer parent) : base(parent) { }

        public virtual void ExecuteCommand(CommandType commandType, object data)
        {
            if (ActiveChild is IEquationContainer)
            {
                ((IEquationContainer)ActiveChild).ExecuteCommand(commandType, data);
            }
            CalculateSize();
        }

        public override string GetSelectedText()
        {
            var stringBulider = new StringBuilder("");
            foreach (var eb in ChildEquations)
            {
                stringBulider.Append(eb.GetSelectedText());
            }
            return stringBulider.ToString();
        }

        public override void StartSelection()
        {
            SelectedItems = 0; //=base.StartSelection();
            SelectionStartIndex = ChildEquations.IndexOf(ActiveChild);
            ActiveChild.StartSelection();
        }

        public override Rect GetSelectionBounds()
        {
            return ActiveChild.GetSelectionBounds();
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            foreach (var eb in ChildEquations)
            {
                eb.DrawEquation(dc);
            }
        }

        public override void RemoveSelection(bool registerUndo)
        {
            ActiveChild.RemoveSelection(registerUndo);
            CalculateSize();
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            var temp = ActiveChild.Copy(removeSelection);
            if (removeSelection)
            {
                CalculateSize();
            }
            return temp;
        }

        public override void Paste(XElement xe)
        {
            ActiveChild.Paste(xe);
            CalculateSize();
        }

        public override bool Select(Key key)
        {
            return ActiveChild.Select(key);
        }

        public override void DeSelect()
        {
            SelectedItems = 0; //base.Deselect()
            foreach (var eb in ChildEquations)
            {
                eb.DeSelect();
            }
        }

        public virtual void ChildCompletedUndo(IEquationBase child)
        {
            ActiveChild = child;
            CalculateSize();
            if (ParentEquation != null)
            {
                ParentEquation.ChildCompletedUndo(this);
            }
        }

        public override void ConsumeText(string text)
        {
            ActiveChild.ConsumeText(text);
            CalculateSize();
        }

        public override void ConsumeFormattedText(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo)
        {
            ActiveChild.ConsumeFormattedText(text, formats, modes, decorations, addUndo);
            CalculateSize();
        }

        public override bool ConsumeKey(Key key)
        {
            var temp = ActiveChild.ConsumeKey(key);
            CalculateSize();
            return temp;
        }

        public override Point GetVerticalCaretLocation()
        {
            return ActiveChild.GetVerticalCaretLocation();
        }

        public override double GetVerticalCaretLength()
        {
            return ActiveChild.GetVerticalCaretLength();
        }

        public virtual IEquationContainer GetInnerMostEquationContainer()
        {
            var child = ActiveChild as IEquationContainer;
            return child != null ? child.GetInnerMostEquationContainer() : this;
        }

        public Point GetHorizontalCaretLocation()
        {
            var child = ActiveChild as EquationContainer;
            return child?.GetHorizontalCaretLocation() ?? new Point(Left, Bottom);
        }

        public double GetHorizontalCaretLength()
        {
            var child = ActiveChild as EquationContainer;
            return child?.GetHorizontalCaretLength() ?? Width;
        }

        public override IEquationBase Split(IEquationContainer newParent)
        {
            var result = ActiveChild.Split(this);
            CalculateSize();
            return result;
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            foreach (var eb in ChildEquations)
            {
                if (!eb.IsStatic && eb.Bounds.Contains(mousePoint))
                {
                    ActiveChild = eb;
                    return ActiveChild.ConsumeMouseClick(mousePoint);
                }
            }
            return false;
        }

        public override void SetCursorOnKeyUpDown(Key key, Point point)
        {
            if (key == Key.Up)
            {
                for (var i = ChildEquations.Count - 1; i >= 0; i--)
                {
                    var type = ChildEquations[i].GetType();
                    if (type == typeof(IRowContainer) || type == typeof(IEquationRow))
                    {
                        ChildEquations[i].SetCursorOnKeyUpDown(key, point);
                        ActiveChild = ChildEquations[i];
                        break;
                    }
                }
            }
            else
            {
                for (var i = 0; i < ChildEquations.Count; i++)
                {
                    var type = ChildEquations[i].GetType();
                    if (type == typeof(IRowContainer) || type == typeof(IEquationRow))
                    {
                        ChildEquations[i].SetCursorOnKeyUpDown(key, point);
                        ActiveChild = ChildEquations[i];
                        break;
                    }
                }
            }
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            ActiveChild.HandleMouseDrag(mousePoint);
        }

        public override double FontSize
        {
            get { return base.FontSize; }
            set
            {
                base.FontSize = value;
                foreach (var eb in ChildEquations)
                {
                    eb.FontSize = FontSize;
                }
                CalculateSize();
            }
        }

        public override void SelectAll()
        {
            SelectionStartIndex = 0;
            SelectedItems = ChildEquations.Count - 1;
            ActiveChild = ChildEquations.Last();
            foreach (var child in ChildEquations)
            {
                child.SelectAll();
            }
        }

        public override void ModifySelection(string operation, string argument, bool applied, bool addUndo)
        {
            foreach (var eb in ChildEquations)
            {
                eb.ModifySelection(operation, argument, applied, addUndo);
            }
            CalculateSize();
        }

        public override HashSet<int> GetUsedTextFormats()
        {
            var list = new HashSet<int>();
            foreach (var eb in ChildEquations)
            {
                var listFormats = eb.GetUsedTextFormats();
                if (listFormats != null) //This check is necessary as the base returns 'null'
                {
                    foreach (var i in listFormats)
                    {
                        if (!list.Contains(i))
                        {
                            list.Add(i);
                        }
                    }
                }
            }
            return list;
        }

        public override void ResetTextFormats(Dictionary<int, int> formatMapping)
        {
            foreach (var eb in ChildEquations)
            {
                eb.ResetTextFormats(formatMapping);
            }
        }
        
        public override bool ApplySymbolGap
        {
            get
            {
                return base.ApplySymbolGap;
            }
            set
            {
                base.ApplySymbolGap = value;
                foreach (var eb in ChildEquations)
                {
                    eb.ApplySymbolGap = value;
                }
            }
        }
    }
}
