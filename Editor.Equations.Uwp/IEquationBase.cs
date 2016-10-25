using System.Collections.Generic;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;

namespace Editor
{
    public interface IEquationBase
    {
        Thickness Margin { get; }
        bool ApplySymbolGap { get; set; }
        HashSet<int> GetUsedTextFormats();
        void ResetTextFormats(Dictionary<int, int> formatMapping);
        bool ConsumeMouseClick(Point mousePoint);
        void HandleMouseDrag(Point mousePoint);
        IEquationBase Split(EquationContainer newParent);
        void ConsumeText(string text);
        void ConsumeFormattedText(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo);
        bool ConsumeKey(VirtualKey key);
        Point GetVerticalCaretLocation();
        double GetVerticalCaretLength();
        void CalculateWidth();
        void CalculateHeight();
        XElement Serialize();
        void DeSerialize(XElement xElement);
        void StartSelection();
        bool Select(VirtualKey key);
        void DeSelect();
        void RemoveSelection(bool registerUndo);
        Rect GetSelectionBounds();
        CopyDataObject Copy(bool removeSelection);
        void Paste(XElement xe);
        void SetCursorOnKeyUpDown(VirtualKey key, Point point);
        void ModifySelection(string operation, string argument, bool applied, bool addUndo);
        void CalculateSize();
        void SelectAll();
        string GetSelectedText();
        double FontFactor { get; set; }
        double FontSize { get; set; }
        double RefX { get; }
        double RefY { get; }
        double Width { get; set; }
        double Height { get; set; }
        double Left { get; set; }
        double Top { get; set; }
        double Right { get; set; }
        double Bottom { get; set; }
        EquationContainer ParentEquation { get; set; }
        bool IsStatic { get; set; }
        Rect Bounds { get; }
        double MidX { get; set; }
        double MidY { get; set; }
        int SelectionStartIndex { get; set; }
        int SelectedItems { get; set; }
        Point Location { get; set; }
    }
}
