using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace Editor
{
    public sealed class TextEquation : EquationBase, ISupportsUndo
    {
        private static readonly HashSet<char> Symbols = new HashSet<char>() 
        { 
            '+', '\u2212', '-', '=',  '>', '<',
            '\u2190', '\u2191', '\u2192', '\u2193', '\u2194', '\u2195', '\u2196', 
            '\u2197', '\u2198', '\u2199', '\u219A', '\u219B', '\u219C', '\u219D', 
            '\u219E', '\u219F', '\u21A0', '\u21A1', '\u21A2', '\u21A3', '\u21A4', 
            '\u21A5', '\u21A6', '\u21A7', '\u21A8', '\u21A9', '\u21AA', '\u21AB', 
            '\u21AC', '\u21AD', '\u21AE', '\u21AF', '\u21B0', '\u21B1', '\u21B2',
            '\u21B3', '\u21B4', '\u21B5', '\u21B6', '\u21B7', '\u21B8', '\u21B9',
            '\u21BA', '\u21BB', '\u21BC', '\u21BD', '\u21BE', '\u21BF', '\u21C0',
            '\u21C1', '\u21C2', '\u21C3', '\u21C4', '\u21C5', '\u21C6', '\u21C7',
            '\u21C8', '\u21C9', '\u21CA', '\u21CB', '\u21CC', '\u21CD', '\u21CE',
            '\u21CF', '\u21D0', '\u21D1', '\u21D2', '\u21D3', '\u21D4', '\u21D5', 
            '\u21D6', '\u21D7', '\u21D8', '\u21D8', '\u21D9', '\u21DA', '\u21DB',
            '\u21DC',  
            '\u00d7', '\u00b7', '\u00f7', '\u00b1', 
            '\u2200', '\u2208', '\u2209', '\u220B', '\u220C', 
            '\u2217', '\u2227', '\u2228', 
            '\u2229', '\u222A', '\u2234', '\u2235', '\u2237', '\u2238', '\u2264', 
            '\u2265', '\u226e', '\u226f',  
            '\u25E0', '\u25E1',
        };

        public static event PropertyChangedEventHandler InputPropertyChanged = (x, y) => { };
        private static bool _inputBold;
        private static bool _inputItalic;
        private static bool _inputUnderline;
        private static EditorMode _editorMode = EditorMode.Math;
        public static bool InputBold
        {
            get { return _inputBold; }
            set
            {
                if (_inputBold != value)
                {
                    _inputBold = value;
                    InputPropertyChanged(null, new PropertyChangedEventArgs("InputBold"));
                }
            }
        }

        public static bool InputItalic
        {
            get { return _inputItalic; }
            set
            {
                if (_inputItalic != value)
                {
                    _inputItalic = value;
                    InputPropertyChanged(null, new PropertyChangedEventArgs("InputItalic"));
                }
            }
        }
        public static bool InputUnderline
        {
            get { return _inputUnderline; }
            set
            {
                if (_inputUnderline != value)
                {
                    _inputUnderline = value;
                    InputPropertyChanged(null, new PropertyChangedEventArgs("InputUnderline"));
                }
            }
        }
        public static EditorMode EditorMode
        {
            get { return _editorMode; }
            set
            {
                if (_editorMode != value)
                {
                    _editorMode = value;
                    InputPropertyChanged(null, new PropertyChangedEventArgs("EditorMode"));
                }
            }
        }

        private readonly StringBuilder _textData = new StringBuilder();
        private int _caretIndex = 0;
        private static FontType _fontType = FontType.StixGeneral;
        private readonly List<CharacterDecorationInfo> _decorations = new List<CharacterDecorationInfo>();
        private readonly List<int> _formats = new List<int>();
        private readonly List<EditorMode> _modes = new List<EditorMode>();

        public TextEquation(IEquationContainer parent)
            : base(parent)
        {
            CalculateSize();
        }

        private void SetCaretIndex(int index)
        {
            _caretIndex = index;
            if (_formats.Count > 0)
            {
                var formatIndexToUse = index;
                if (index > 0)
                {
                    formatIndexToUse--;
                }
                InputBold = TextManager.IsBold(_formats[formatIndexToUse]);
                InputItalic = TextManager.IsItalic(_formats[formatIndexToUse]);
                InputUnderline = TextManager.IsUnderline(_formats[formatIndexToUse]);
                EditorMode = _modes[formatIndexToUse];
                FontType = TextManager.GetFontType(_formats[formatIndexToUse]);
            }
        }

        public static FontType FontType
        {
            get { return _fontType; }
            set
            {
                if (_fontType != value)
                {
                    _fontType = value;
                    InputPropertyChanged(null, new PropertyChangedEventArgs("FontType"));
                }
            }
        }

        public override void StartSelection()
        {
            SelectedItems = 0;  // = base.StartSelection();
            SelectionStartIndex = _caretIndex;
        }

        public override bool Select(Key key)
        {
            if (key == Key.Left)
            {
                if (_caretIndex > 0)
                {
                    SelectedItems--;
                    SetCaretIndex(_caretIndex - 1);
                    return true;
                }
            }
            else if (key == Key.Right)
            {
                if (_caretIndex < _textData.Length)
                {
                    SelectedItems++;
                    SetCaretIndex(_caretIndex + 1);
                    return true;
                }
            }
            return false;
        }

        public string Text
        {
            get
            {
                return _textData.ToString();
            }
        }

        public override string GetSelectedText()
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                return _textData.ToString(startIndex, count);
            }
            return "";
        }

        public int[] GetSelectedFormats()
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                return _formats.GetRange(startIndex, count).ToArray();
            }
            return new int[0];
        }

        public EditorMode[] GetSelectedModes()
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                return _modes.GetRange(startIndex, count).ToArray();
            }
            return new EditorMode[0];
        }

        public CharacterDecorationInfo[] GetSelectedDecorations()
        {
            if (SelectedItems != 0 && _decorations.Count > 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
                var selected = (from d in _decorations where d.Index >= startIndex && d.Index < endIndex select d).ToArray();
                foreach (var s in selected)
                {
                    s.Index = s.Index - startIndex;
                }
                return selected;
            }
            else
            {
                return new CharacterDecorationInfo[0];
            }
        }

        public int[] GetFormats()
        {
            return _formats.ToArray();
        }

        public EditorMode[] GetModes()
        {
            return _modes.ToArray();
        }

        public CharacterDecorationInfo[] GetDecorations()
        {
            if (_decorations.Count > 0)
            {
                return _decorations.ToArray();
            }
            else
            {
                return new CharacterDecorationInfo[0];
            }
        }

        public override void SelectAll()
        {
            SelectionStartIndex = 0;
            SelectedItems = _textData.Length;
            CaretIndex = _textData.Length;
        }

        public void ResetTextEquation(int caretIndex, int selectionStartIndex, int selectedItems, string text, int[] formats,
                                      EditorMode[] modes, CharacterDecorationInfo[] cdiList)
        {
            _textData.Clear();
            _textData.Append(text);
            this._caretIndex = caretIndex;
            this.SelectionStartIndex = selectionStartIndex;
            this.SelectedItems = selectedItems;
            this._formats.Clear();
            this._formats.AddRange(formats);
            this._modes.Clear();
            this._modes.AddRange(modes);
            this._decorations.Clear();
            _decorations.AddRange(cdiList);
            FormatText();
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                if (registerUndo)
                {
                    var textRemoveAction = new TextRemoveAction(this, startIndex, _textData.ToString(startIndex, count),
                                                                             SelectionStartIndex, SelectedItems, ParentEquation.SelectionStartIndex,
                                                                             _formats.GetRange(startIndex, count).ToArray(),
                                                                             _modes.GetRange(startIndex, count).ToArray(),
                                                                             (from d in _decorations where d.Index >= startIndex && d.Index < startIndex + count select d).ToArray()
                                                                             );
                    UndoManager.AddUndoAction(textRemoveAction);
                }
                RemoveContent(startIndex, count);
                SetCaretIndex(startIndex);
                FormatText();
                SelectedItems = 0;
            }
        }

        private void RemoveContent(int startIndex, int count)
        {
            _decorations.RemoveAll(x => x.Index >= startIndex && x.Index < startIndex + count);
            var list = from d in _decorations where d.Index >= startIndex + count select d;
            foreach (var v in list)
            {
                v.Index = v.Index - count;
            }
            _textData.Remove(startIndex, count);
            _formats.RemoveRange(startIndex, count);
            _modes.RemoveRange(startIndex, count);
        }

        public void SelectToStart()
        {
            SetCaretIndex(0);
            SelectedItems = -SelectionStartIndex;
        }

        public void SelectToEnd()
        {
            SetCaretIndex(_textData.Length);
            SelectedItems = _caretIndex - SelectionStartIndex;
        }

        public void MoveToStart()
        {
            SetCaretIndex(0);
        }

        public void MoveToEnd()
        {
            SetCaretIndex(_textData.Length);
        }

        public int CaretIndex
        {
            get { return _caretIndex; }
            set { SetCaretIndex(value); }
        }

        public int TextLength
        {
            get { return _textData.Length; }
        }

        public override Rect GetSelectionBounds()
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                if (count > 0)
                {
                    var beforeWidth = GetWidth(0, startIndex);
                    var selectedWith = GetWidth(startIndex, count);
                    return new Rect(Left + beforeWidth, Top, selectedWith, Height);
                }
            }
            return Rect.Empty;
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            var selectedText = GetSelectedText();
            var selectedFormats = GetSelectedFormats();
            if (selectedText.Length > 0)
            {
                var formattedText = TextManager.GetFormattedText(selectedText, selectedFormats.ToList());
                var bitmap = new RenderTargetBitmap((int)(Math.Ceiling(Width + 4)), (int)(Math.Ceiling(Height + 4)), 96, 96, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, bitmap.Width, bitmap.Height));
                    DrawEquation(dc);
                }
                bitmap.Render(dv);
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                var xElement = CreateXElement(startIndex, count);
                if (removeSelection)
                {
                    RemoveSelection(true);
                }
                return new CopyDataObject { Image = bitmap, Text = selectedText, XElement = xElement };
            }
            return null;
        }

        public override void Paste(XElement xElement)
        {
            if (xElement.Name.LocalName == GetType().Name)
            {
                //textData.Insert(caretIndex, xElement.Element("Text").Value);
                var text = xElement.Element("Text").Value;
                var formatStrings = xElement.Element("Formats").Value.Split(',');
                var modeStrings = xElement.Element("Modes").Value.Split(',');
                var formats = new int[text.Length];
                var modes = new EditorMode[text.Length];
                var decos = xElement.Elements().FirstOrDefault(x => x.Name == "Decorations");
                CharacterDecorationInfo[] decorations = null;
                if (decos != null)
                {
                    var children = decos.Elements("d").ToList();
                    decorations = new CharacterDecorationInfo[children.Count];
                    for (var i = 0; i < children.Count; i++)
                    {
                        var list = children[i].Value.Split(',');
                        decorations[i] = new CharacterDecorationInfo();
                        decorations[i].DecorationType = (CharacterDecorationType)Enum.Parse(typeof(CharacterDecorationType), list[0]);
                        decorations[i].Index = int.Parse(list[1]);
                        decorations[i].Position = (Position)Enum.Parse(typeof(Position), list[2]);
                        decorations[i].UnicodeString = list[3];
                    }
                }
                try
                {
                    for (var i = 0; i < text.Length; i++)
                    {
                        formats[i] = int.Parse(formatStrings[i]);
                        modes[i] = (EditorMode)Enum.Parse(typeof(EditorMode), modeStrings[i]);
                    }
                }
                catch
                {
                    var formatId = TextManager.GetFormatId(FontSize, _fontType, FontStyles.Normal, FontWeights.Normal, Brushes.Black, false);
                    for (var i = 0; i < text.Length; i++)
                    {
                        formats[i] = formatId;
                        modes[i] = Editor.EditorMode.Math;
                    }
                }
                ConsumeFormattedText(text, formats, modes, decorations, true);
            }
        }

        public override XElement Serialize()
        {
            return CreateXElement(0, _textData.Length);
        }

        private XElement CreateXElement(int start, int count)
        {
            var thisElement = new XElement(GetType().Name);
            var text = new XElement("Text", _textData.ToString(start, count));
            var strBuilder = new StringBuilder();
            var modeStr = new StringBuilder();
            for (var i = start; i < start + count; i++)
            {
                strBuilder.Append(_formats[i] + ",");
                modeStr.Append((int)_modes[i] + ",");
            }
            if (strBuilder.Length > 0)
            {
                strBuilder.Remove(strBuilder.Length - 1, 1);
                modeStr.Remove(modeStr.Length - 1, 1);
            }
            var formatsElement = new XElement("Formats", strBuilder.ToString());
            var modesElement = new XElement("Modes", modeStr.ToString());
            thisElement.Add(text);
            thisElement.Add(formatsElement);
            thisElement.Add(modesElement);
            var d = (from x in _decorations where x.Index >= start && x.Index < start + count select x).ToList();
            if (d.Count > 0)
            {
                var decorationsElement = new XElement("Decorations");
                foreach (var x in d)
                {
                    var xe = new XElement("d", x.DecorationType + "," + (x.Index - start) + "," + x.Position + "," + x.UnicodeString);
                    decorationsElement.Add(xe);
                }
                thisElement.Add(decorationsElement);
            }
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _textData.Append(xElement.Element("Text").Value);
            try
            {
                var formatStrings = xElement.Element("Formats").Value.Split(',');
                var modeStrings = xElement.Element("Modes").Value.Split(',');
                for (var i = 0; i < formatStrings.Length; i++)
                {
                    _formats.Add(int.Parse(formatStrings[i]));
                    _modes.Add((EditorMode)Enum.Parse(typeof(EditorMode), modeStrings[i]));
                }
                var decos = xElement.Elements().FirstOrDefault(x => x.Name == "Decorations");
                if (decos != null)
                {
                    var children = decos.Elements("d").ToList();
                    for (var i = 0; i < children.Count; i++)
                    {
                        var list = children[i].Value.Split(',');
                        var d = new CharacterDecorationInfo();
                        d.DecorationType = (CharacterDecorationType)Enum.Parse(typeof(CharacterDecorationType), list[0]);
                        d.Index = int.Parse(list[1]);
                        d.Position = (Position)Enum.Parse(typeof(Position), list[2]);
                        d.UnicodeString = list[3];
                        _decorations.Add(d);
                    }
                }
            }
            catch
            {
                _formats.Clear();
                _modes.Clear();
                _decorations.Clear();
                var formatId = TextManager.GetFormatId(FontSize, _fontType, FontStyles.Normal, FontWeights.Normal, Brushes.Black, false);
                for (var i = 0; i < _textData.Length; i++)
                {
                    _formats.Add(formatId);
                    _modes.Add(Editor.EditorMode.Math);
                }
            }
            FormatText();
        }

        public override double FontSize
        {
            get { return base.FontSize; }
            set
            {
                base.FontSize = value;
                for (var i = 0; i < _formats.Count; i++)
                {
                    _formats[i] = TextManager.GetFormatIdForNewSize(_formats[i], value);
                }
                FormatText();
            }
        }

        public override IEquationBase Split(IEquationContainer newParent)
        {
            var newText = new TextEquation(newParent);
            var itemCount = _textData.Length - _caretIndex;
            newText._textData.Append(_textData.ToString(_caretIndex, itemCount));
            _textData.Remove(_caretIndex, itemCount);
            newText._formats.AddRange(_formats.GetRange(_caretIndex, itemCount));
            _formats.RemoveRange(_caretIndex, itemCount);
            newText._modes.AddRange(_modes.GetRange(_caretIndex, itemCount));
            _modes.RemoveRange(_caretIndex, itemCount);
            var list = (from d in _decorations where d.Index >= _caretIndex select d).ToList();
            foreach (var v in list)
            {
                _decorations.Remove(v);
                v.Index = v.Index - _caretIndex;
                newText._decorations.Add(v);
            }
            SetCaretIndex(_textData.Length);
            FormatText();
            newText.FormatText();
            return newText;
        }

        public void Merge(TextEquation secondText)
        {
            SetCaretIndex(_textData.Length);
            foreach (var v in secondText._decorations)
            {
                v.Index = v.Index + _textData.Length;
                _decorations.Add(v);
            }
            //secondText.decorations.Clear();
            _textData.Append(secondText._textData.ToString());
            _formats.AddRange(secondText._formats);
            _modes.AddRange(secondText._modes);
            FormatText();
        }

        public override void ConsumeText(string text)
        {
            //text = "\U0001D400";
            //string someText = char.ConvertFromUtf32(0x1D7D9);
            var list = from d in _decorations where d.Index >= _caretIndex select d;
            foreach (var v in list)
            {
                v.Index = v.Index + text.Length;
            }
            var style = InputItalic ? FontStyles.Italic : FontStyles.Normal;
            _textData.Insert(_caretIndex, text);
            var name = FunctionNames.CheckForFunctionName(_textData.ToString(0, _caretIndex + text.Length));
            if (name != null && EditorMode == Editor.EditorMode.Math && _caretIndex - (name.Length - text.Length) >= 0)
            {
                for (var i = _caretIndex - (name.Length - text.Length); i < _caretIndex; i++)
                {
                    _formats[i] = TextManager.GetFormatIdForNewStyle(_formats[i], FontStyles.Normal);
                    _modes[i] = Editor.EditorMode.Math;
                }
                style = FontStyles.Normal;
            }
            else if (text.Length == 1 && EditorMode == Editor.EditorMode.Math)
            {
                if (((int)text[0] >= 65 && (int)text[0] <= 90 || (int)text[0] >= 97 && (int)text[0] <= 122) || char.IsWhiteSpace(text[0]))
                {
                    style = FontStyles.Italic;
                }
                else
                {
                    style = FontStyles.Normal;
                }
            }

            var formatId = TextManager.GetFormatId(FontSize, _fontType, style, InputBold ? FontWeights.Bold : FontWeights.Normal,
                                                   Brushes.Black, InputUnderline);
            var tempFormats = new int[text.Length];
            var tempModes = new EditorMode[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                _formats.Insert(i + _caretIndex, formatId);
                _modes.Insert(i + _caretIndex, EditorMode);
                tempFormats[i] = formatId;
                tempModes[i] = EditorMode;
            }
            UndoManager.AddUndoAction(new TextAction(this, _caretIndex, text, tempFormats, tempModes, new CharacterDecorationInfo[0]) { UndoFlag = false });
            SetCaretIndex(_caretIndex + text.Length);
            FormatText();
        }

        public override void ConsumeFormattedText(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo)
        {
            //this.decorations.AddRange(decorations);
            _textData.Insert(_caretIndex, text);
            this._formats.InsertRange(_caretIndex, formats);
            this._modes.InsertRange(_caretIndex, modes);
            if (decorations != null)
            {
                foreach (var d in decorations)
                {
                    d.Index = d.Index + _caretIndex;
                }
                this._decorations.AddRange(decorations);
            }
            if (addUndo)
            {
                UndoManager.AddUndoAction(new TextAction(this, _caretIndex, text, formats, modes, decorations ?? new CharacterDecorationInfo[0]) { UndoFlag = false });
            }
            SetCaretIndex(_caretIndex + text.Length);
            FormatText();
        }

        public void SetFormattedText(string text, int[] formats, EditorMode[] modes)
        {
            _textData.Clear();
            this._formats.Clear();
            this._modes.Clear();
            _textData.Append(text);
            this._formats.AddRange(formats);
            this._modes.AddRange(modes);
            SetCaretIndex(text.Length);
            FormatText();
        }

        public override bool ConsumeKey(Key key)
        {
            var consumed = false;
            switch (key)
            {
                case Key.Home:
                    SetCaretIndex(0);
                    consumed = true;
                    break;
                case Key.End:
                    SetCaretIndex(_textData.Length);
                    consumed = true;
                    break;
                case Key.Delete:
                    if (_textData.Length > 0 && _caretIndex < _textData.Length)
                    {
                        var cdi = (from d in _decorations where d.Index == _caretIndex select d).LastOrDefault();
                        if (cdi != null)
                        {
                            RemoveDecoration(cdi);
                        }
                        else
                        {
                            RemoveChar(_caretIndex);
                        }
                        FormatText();
                        consumed = true;
                    }
                    break;
                case Key.Back:
                    if (_caretIndex > 0)
                    {
                        var cdi = (from d in _decorations where d.Index == _caretIndex - 1 select d).LastOrDefault();
                        if (cdi != null)
                        {
                            RemoveDecoration(cdi);
                        }
                        else
                        {
                            RemoveChar(_caretIndex - 1);
                            SetCaretIndex(_caretIndex - 1);
                        }
                        FormatText();
                        consumed = true;
                    }
                    break;
                case Key.Right:
                    if (_caretIndex < _textData.Length)
                    {
                        SetCaretIndex(_caretIndex + 1);
                        consumed = true;
                    }
                    break;
                case Key.Left:
                    if (_caretIndex > 0)
                    {
                        SetCaretIndex(_caretIndex - 1);
                        consumed = true;
                    }
                    break;
            }
            return consumed;
        }

        private void RemoveDecoration(CharacterDecorationInfo cdi)
        {
            _decorations.Remove(cdi);
            UndoManager.AddUndoAction(new DecorationAction(this, new[] { cdi }) { UndoFlag = false });
        }

        public override void ModifySelection(string operation, string argument, bool applied, bool addUndo)
        {
            if (SelectedItems != 0)
            {
                if (operation == "format")
                {
                    ModifyFormat(argument, applied, addUndo);
                }
                else if (operation == "font")
                {
                    ModifyFont((FontType)Enum.Parse(typeof(FontType), argument), applied, addUndo);
                }
                else if (operation == "mode")
                {
                    ModifyMode(argument, addUndo);
                }
                FormatText();
            }
        }

        private void ModifyFormat(string format, bool applied, bool addUndo)
        {
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
            var oldFormats = _formats.GetRange(startIndex, count).ToArray();
            for (var i = startIndex; i < startIndex + count; i++)
            {
                switch (format.ToLower())
                {
                    case "underline":
                        _formats[i] = TextManager.GetFormatIdForNewUnderline(_formats[i], applied);
                        break;
                    case "bold":
                        _formats[i] = TextManager.GetFormatIdForNewWeight(_formats[i], applied ? FontWeights.Bold : FontWeights.Normal);
                        break;
                    case "italic":
                        _formats[i] = TextManager.GetFormatIdForNewStyle(_formats[i], applied ? FontStyles.Italic : FontStyles.Normal);
                        break;
                    default:
                        throw new ArgumentException("Incorrect value passed with font format change request."); ;
                }
            }
            if (addUndo)
            {
                var tfa = new TextFormatAction(this, startIndex, oldFormats, _formats.GetRange(startIndex, count).ToArray());
                UndoManager.AddUndoAction(tfa);
            }
        }

        private void ModifyMode(string newMode, bool addUndo)
        {
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
            var oldModes = _modes.GetRange(startIndex, count).ToArray();
            for (var i = startIndex; i < startIndex + count; i++)
            {
                switch (newMode.ToLower())
                {
                    case "math":
                        _modes[i] = EditorMode.Math;
                        break;
                    case "text":
                        _modes[i] = EditorMode.Text;
                        break;
                    default:
                        throw new ArgumentException("Incorrect value passed with mode change request.");
                }
            }
            if (addUndo)
            {
                var mca = new ModeChangeAction(this, startIndex, oldModes, _modes.GetRange(startIndex, count).ToArray());
                UndoManager.AddUndoAction(mca);
            }
        }

        private void ModifyFont(FontType fontType, bool applied, bool addUndo)
        {
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
            var oldFormats = _formats.GetRange(startIndex, count).ToArray();
            for (var i = startIndex; i < startIndex + count; i++)
            {
                _formats[i] = TextManager.GetFormatIdForNewFont(_formats[i], fontType);
            }
            if (addUndo)
            {
                var tfa = new TextFormatAction(this, startIndex, oldFormats, _formats.GetRange(startIndex, count).ToArray());
                UndoManager.AddUndoAction(tfa);
            }
        }

        private void FormatText()
        {
            CalculateSize();
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (Left <= mousePoint.X && Right >= mousePoint.X)
            {
                SetCaretPosition(mousePoint);
                return true;
            }
            return false;
        }

        private void SetCaretPosition(Point mousePoint)
        {
            var left = Left;
            _caretIndex = _textData.Length;
            for (; _caretIndex > 0; _caretIndex--)
            {
                var lastChar = TextManager.GetFormattedText(_textData.ToString(_caretIndex - 1, 1), _formats.GetRange(_caretIndex - 1, 1));
                //FormattedText textPart = textManager.GetFormattedText(textData.ToString(0, caretIndex), formats.GetRange(0, caretIndex));
                left = GetWidth(0, _caretIndex) + Left;
                //left = textPart.GetFullWidth() + Left;
                if (left <= mousePoint.X + lastChar.GetFullWidth() / 2)
                    break;
            }
            SetCaretIndex(_caretIndex);
        }

        public override void SetCursorOnKeyUpDown(Key key, Point point)
        {
            SetCaretPosition(point);
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (Left <= mousePoint.X && Right >= mousePoint.X)
            {
                SetCaretPosition(mousePoint);
                SelectedItems = _caretIndex - SelectionStartIndex;
            }
        }

        public override Point GetVerticalCaretLocation()
        {
            if (_caretIndex > 0)
            {
                return new Point(Left + GetWidth(0, _caretIndex), Top);
            }
            else
            {
                return Location;
            }
        }

        public override void CalculateWidth()
        {
            Width = GetWidth(0, _textData.Length);
        }

        private double GetWidth(int start, int count)
        {
            double width = 0;
            if (count > 0 && start + count <= _textData.Length)
            {
                var symIndexes = FindSymbolIndexes(start, start + count);
                var groups = (from d in _decorations orderby d.Index where d.Index >= start && d.Index < start + count group d by d.Index).ToList();
                var done = start;
                for (var i = 0; i <= symIndexes.Count; i++)
                {
                    var limit = i < symIndexes.Count ? symIndexes[i] : start + count;
                    if (limit - done > 0)
                    {
                        var subGroups = (from g in groups where g.Key >= done && g.Key < limit select g).ToList();
                        for (var j = 0; j <= subGroups.Count; j++)
                        {
                            var subLimit = j < subGroups.Count ? subGroups[j].Key : limit;
                            var text = _textData.ToString(done, subLimit - done);
                            if (text.Length > 0)
                            {
                                var ft = TextManager.GetFormattedText(text, _formats.Skip(done).Take(text.Length).ToList());
                                width += ft.GetFullWidth();
                                done += ft.Text.Length;
                            }
                            if (j < subGroups.Count)
                            {
                                var charFt = TextManager.GetFormattedText(_textData[subGroups[j].Key].ToString(), _formats[subGroups[j].Key]);
                                double hCenter;
                                double charLeft;
                                var decoWidth = GetDecoratedCharWidth(charFt, subGroups[j].ToList(), done, out charLeft, out hCenter);
                                width += decoWidth;
                                done++;
                                if (done < start + count)
                                {
                                    width += charFt.OverhangTrailing;
                                }
                            }
                        }
                    }
                    if (i < symIndexes.Count)
                    {
                        var addSpaceBefore = symIndexes[i] > 0 ? !Symbols.Contains(_textData[symIndexes[i] - 1]) : true;
                        width += addSpaceBefore ? SymSpace : 0;
                        var ft = TextManager.GetFormattedText(_textData[symIndexes[i]].ToString(), _formats[symIndexes[i]]);
                        var group = groups.FirstOrDefault(x => x.Key == symIndexes[i]);
                        width += ft.GetFullWidth() + SymSpace;
                        //dc.DrawLine(new Pen(Brushes.Purple, 1), new Point(left, Top), new Point(left, Bottom));
                        done++;
                    }
                }
            }
            return width;
        }

        private double _refY = 0;
        public override double RefY
        {
            get { return _refY; }
        }

        private double _topExtra = 0;
        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            //dc.DrawRectangle(Brushes.Yellow, null, Bounds);
            //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, refY + Top), new Point(Left + Width, refY + Top));
            //dc.DrawLine(new Pen(Brushes.Blue, 1), new Point(0, Top), new Point(10000, Top));
            if (_textData.Length > 0)
            {
                var groups = (from d in _decorations orderby d.Index group d by d.Index).ToList();
                var symIndexes = FindSymbolIndexes();
                var done = 0;
                var left = Left;
                for (var i = 0; i <= symIndexes.Count; i++)
                {
                    var limit = i < symIndexes.Count ? symIndexes[i] : _textData.Length;
                    if (limit - done > 0)
                    {
                        var subGroups = (from g in groups where g.Key >= done && g.Key < limit select g).ToList();
                        for (var j = 0; j <= subGroups.Count; j++)
                        {
                            var subLimit = j < subGroups.Count ? subGroups[j].Key : limit;
                            var text = _textData.ToString(done, subLimit - done);
                            if (text.Length > 0)
                            {
                                var ft = TextManager.GetFormattedText(text, _formats.Skip(done).Take(text.Length).ToList());
                                ft.DrawTextLeftAligned(dc, new Point(left, Top - _topExtra));
                                left += ft.GetFullWidth();
                                //dc.DrawLine(new Pen(Brushes.Blue, 1), new Point(left, Top), new Point(left, Bottom));
                                if (done == 0 && !char.IsWhiteSpace(_textData[0]))
                                {
                                    //left -= ft.OverhangLeading;
                                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(left, Top), new Point(left, Bottom));
                                }
                                if (!char.IsWhiteSpace(text[text.Length - 1]))
                                {
                                    //left -= ft.OverhangTrailing;
                                    //dc.DrawLine(new Pen(Brushes.Green, 1), new Point(left, Top), new Point(left, Bottom));
                                }
                                done += ft.Text.Length;
                            }
                            if (j < subGroups.Count)
                            {
                                var charFt = TextManager.GetFormattedText(_textData[subGroups[j].Key].ToString(), _formats[subGroups[j].Key]);
                                double hCenter;
                                double charLeft;
                                var decoWidth = GetDecoratedCharWidth(charFt, subGroups[j].ToList(), done, out charLeft, out hCenter);
                                charFt.DrawTextCenterAligned(dc, new Point(left + hCenter, Top - _topExtra));
                                DrawAllDecorations(dc, left, hCenter, Top - _topExtra, charLeft, charFt, done, subGroups[j].ToList());
                                left += decoWidth + charFt.OverhangLeading;// +(diff > 0 ? diff : 0);
                                done++;
                                //if (done < limit)
                                //{
                                //    left += charFt.OverhangTrailing;
                                //}
                            }
                        }
                    }
                    if (i < symIndexes.Count)
                    {
                        var addSpaceBefore = symIndexes[i] > 0 ? !Symbols.Contains(_textData[symIndexes[i] - 1]) : true;
                        left += addSpaceBefore ? SymSpace : 0;
                        var ft = TextManager.GetFormattedText(_textData[symIndexes[i]].ToString(), _formats[symIndexes[i]]);
                        ft.DrawTextLeftAligned(dc, new Point(left, Top - _topExtra));
                        //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, refY + Top), new Point(Right, refY + Top));
                        //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, Top - topExtra), new Point(Right, Top - topExtra));
                        var group = groups.FirstOrDefault(x => x.Key == symIndexes[i]);
                        if (group != null)
                        {
                            DrawAllDecorations(dc, left, ft.GetFullWidth() / 2, Top - _topExtra, 0, ft, group.Key, group.ToList());
                        }
                        left += ft.GetFullWidth() + SymSpace;
                        //dc.DrawLine(new Pen(Brushes.Purple, 1), new Point(left, Top), new Point(left, Bottom));
                        done++;
                    }
                }
            }
        }

        private double SymSpace
        {
            get { return FontSize * 0.22; }
        }

        public override void CalculateHeight()
        {
            if (_textData.ToString().Trim().Length > 0)
            {
                var groups = (from d in _decorations orderby d.Index group d by d.Index).ToList();
                double upperHalf = 0;
                double lowerHalf = 0;
                var extra = double.MaxValue;
                var done = 0;
                for (var i = 0; i <= groups.Count; i++)
                {
                    List<CharacterDecorationInfo> list = null;
                    var end = i < groups.Count ? groups[i].Key : _textData.Length;
                    if (end - done > 0)
                    {
                        var hm = GetChunkHeightMetrics(done, end, list);
                        upperHalf = Math.Max(hm.UpperHalf, upperHalf);
                        lowerHalf = Math.Max(hm.LowerHalf, lowerHalf);
                        extra = Math.Min(extra, hm.TopExtra);
                        done += end - done;
                    }
                    if (i < groups.Count)
                    {
                        list = groups[i].ToList();
                        var hm = GetChunkHeightMetrics(end, end + 1, list);
                        upperHalf = Math.Max(hm.UpperHalf, upperHalf);
                        lowerHalf = Math.Max(hm.LowerHalf, lowerHalf);
                        extra = Math.Min(extra, hm.TopExtra);
                        done++;
                    }
                }
                _topExtra = extra;
                _refY = upperHalf;
                Height = upperHalf + lowerHalf;
            }
            else
            {
                var formatId = TextManager.GetFormatId(FontSize, _fontType, InputItalic ? FontStyles.Italic : FontStyles.Normal, InputBold ? FontWeights.Bold : FontWeights.Normal,
                                                   Brushes.Black, InputUnderline);
                var ft = TextManager.GetFormattedText("d", formatId);
                //hm.TopExtra = Math.Min(ft.Baseline * .30, ft.TopExtra());
                _topExtra = ft.Baseline * .26;
                _refY = ft.Baseline * .5;
                Height = _refY + ft.Descent() + ft.Baseline * .24;
            }
        }

        private HeightMetrics GetChunkHeightMetrics(int start, int limit, List<CharacterDecorationInfo> list)
        {
            var hm = new HeightMetrics();
            if (list == null)
            {
                var text = _textData.ToString(start, limit - start);
                var ft = TextManager.GetFormattedText(text, _formats.Skip(start).Take(text.Length).ToList());
                //hm.TopExtra = Math.Min(ft.Baseline * .30, ft.TopExtra());
                hm.TopExtra = ft.Baseline * .26;
                hm.UpperHalf = ft.Baseline * .5;
                hm.LowerHalf = ft.Descent() + ft.Baseline * .24;
            }
            else
            {
                var charFt = TextManager.GetFormattedText(_textData[limit - 1].ToString(), _formats[limit - 1]);
                var bottomGroup = (from x in list where x.Position == Position.Bottom select x).ToList();
                var topGroup = (from x in list where x.Position == Position.Top select x).ToList();
                var lowerHalf = charFt.Descent() + charFt.Baseline * .24;
                var upperHalf = charFt.Baseline * .50;
                double topHeight = 0;
                double bottomHeight = 0;
                foreach (var v in topGroup)
                {
                    var sign = TextManager.GetFormattedText(v.UnicodeString, _formats[v.Index]);
                    topHeight += sign.Extent + FontSize * .1;
                }
                foreach (var v in bottomGroup)
                {
                    var sign = TextManager.GetFormattedText(v.UnicodeString, _formats[v.Index]);
                    bottomHeight += sign.Extent + FontSize * .1;
                }
                var diff = upperHalf - (charFt.Extent - lowerHalf + topHeight);
                hm.TopExtra = Math.Min(charFt.Baseline * .26, charFt.TopExtra() - topHeight);
                hm.UpperHalf = upperHalf - (diff > 0 ? 0 : diff);
                hm.LowerHalf = lowerHalf + bottomHeight;
            }
            return hm;
        }

        private void RemoveChar(int index)
        {
            var decos = (from d in _decorations where d.Index == index select d).ToArray();
            UndoManager.AddUndoAction(new TextAction(this, index, _textData.ToString(index, 1), _formats.GetRange(index, 1).ToArray(), _modes.GetRange(index, 1).ToArray(), decos));
            _decorations.RemoveAll(x => x.Index == index);
            var list = from d in _decorations where d.Index > index select d;
            foreach (var v in list)
            {
                v.Index -= 1;
            }
            _textData.Remove(index, 1);
            _formats.RemoveAt(index);
            _modes.RemoveAt(index);
        }

        public void ProcessUndo(EquationAction action)
        {
            if (action.GetType() == typeof(TextAction))
            {
                ProcessTextEquation(action);
            }
            else if (action.GetType() == typeof(TextFormatAction))
            {
                var tfa = action as TextFormatAction;
                _caretIndex = tfa.Index;
                _formats.RemoveRange(tfa.Index, tfa.NewFormats.Length);
                IsSelecting = true;
                if (tfa.UndoFlag)
                {
                    for (var i = 0; i < tfa.OldFormats.Length; i++)
                    {
                        tfa.OldFormats[i] = TextManager.GetFormatIdForNewSize(tfa.OldFormats[i], FontSize);
                    }
                    _formats.InsertRange(tfa.Index, tfa.OldFormats);
                }
                else
                {
                    for (var i = 0; i < tfa.NewFormats.Length; i++)
                    {
                        tfa.NewFormats[i] = TextManager.GetFormatIdForNewSize(tfa.NewFormats[i], FontSize);
                    }
                    _formats.InsertRange(tfa.Index, tfa.NewFormats);
                }
            }
            else if (action.GetType() == typeof(ModeChangeAction))
            {
                var mca = action as ModeChangeAction;
                _caretIndex = mca.Index;
                _modes.RemoveRange(mca.Index, mca.NewModes.Length);
                if (mca.UndoFlag)
                {
                    _modes.InsertRange(mca.Index, mca.OldModes);
                }
                else
                {
                    _modes.InsertRange(mca.Index, mca.NewModes);
                }
            }
            else if (action.GetType() == typeof(DecorationAction))
            {
                var da = action as DecorationAction;
                if (da.UndoFlag)
                {
                    foreach (var cdi in da.CharacterDecorations)
                    {
                        _decorations.Remove(cdi);
                    }
                }
                else
                {
                    _decorations.AddRange(da.CharacterDecorations);
                }
            }
            else
            {
                ProcessTextRemoveAction(action);
            }
            FormatText();
            ParentEquation.ChildCompletedUndo(this);
        }

        private void ProcessTextRemoveAction(EquationAction action)
        {
            var textAction = action as TextRemoveAction;
            var count = textAction.Text.Length;
            if (textAction.UndoFlag)
            {
                _textData.Insert(textAction.Index, textAction.Text);
                for (var i = 0; i < count; i++)
                {
                    textAction.Formats[i] = TextManager.GetFormatIdForNewSize(textAction.Formats[i], FontSize);
                }
                _formats.InsertRange(textAction.Index, textAction.Formats);
                _modes.InsertRange(textAction.Index, textAction.Modes);
                _decorations.AddRange(textAction.Decorations);
                var list = from d in _decorations where d.Index >= textAction.Index select d;
                foreach (var v in list)
                {
                    v.Index += count;
                }
                SelectedItems = textAction.SelectionCount;
                SelectionStartIndex = textAction.SelectionStartIndex;
                ParentEquation.SelectionStartIndex = textAction.ParentSelectionStartIndex;
                ParentEquation.SelectedItems = 0;
                IsSelecting = true;
            }
            else
            {
                _textData.Remove(textAction.Index, count);
                _formats.RemoveRange(textAction.Index, count);
                _modes.RemoveRange(textAction.Index, count);
                _decorations.RemoveAll(x => x.Index >= textAction.Index && x.Index < textAction.Index + count);
                var list = from d in _decorations where d.Index >= textAction.Index + count select d;
                foreach (var v in list)
                {
                    v.Index -= count;
                }
                SelectedItems = 0;
                SelectionStartIndex = textAction.Index;
                IsSelecting = false;
            }
            SetCaretIndex(textAction.Index);
        }

        private void ProcessTextEquation(EquationAction action)
        {
            var textAction = action as TextAction;
            var count = textAction.Text.Length;
            if (textAction.UndoFlag)
            {
                _textData.Insert(textAction.Index, textAction.Text);
                for (var i = 0; i < count; i++)
                {
                    textAction.Formats[i] = TextManager.GetFormatIdForNewSize(textAction.Formats[i], FontSize);
                }
                _formats.InsertRange(textAction.Index, textAction.Formats);
                _modes.InsertRange(textAction.Index, textAction.Modes);
                _decorations.AddRange(textAction.Decorations);
                var list = from d in _decorations where d.Index >= textAction.Index select d;
                foreach (var v in list)
                {
                    v.Index += textAction.Modes.Length;
                }
                SetCaretIndex(textAction.Index + count);
            }
            else
            {
                _textData.Remove(textAction.Index, count);
                _formats.RemoveRange(textAction.Index, count);
                _modes.RemoveRange(textAction.Index, count);
                _decorations.RemoveAll(x => x.Index >= textAction.Index && x.Index < textAction.Index + count);
                var list = from d in _decorations where d.Index >= textAction.Index + count select d;
                foreach (var v in list)
                {
                    v.Index -= count;
                }
                SetCaretIndex(textAction.Index);
            }
            IsSelecting = false;
        }

        public void Truncate(int keepCount)
        {
            var list = (from d in _decorations where d.Index >= keepCount select d).ToList();
            foreach (var v in list)
            {
                _decorations.Remove(v);
                v.Index = v.Index - keepCount;
            }
            //decorations.RemoveAll(x => x.Index >= keepCount);            
            _textData.Length = keepCount;
            _formats.RemoveRange(keepCount, _formats.Count - keepCount);
            _modes.RemoveRange(keepCount, _modes.Count - keepCount);
            SetCaretIndex(Math.Min(_caretIndex, _textData.Length));
            FormatText();
        }

        public void Truncate()
        {
            var list = (from d in _decorations where d.Index >= _caretIndex select d).ToList();
            foreach (var v in list)
            {
                _decorations.Remove(v);
                v.Index = v.Index - _caretIndex;
            }
            //decorations.RemoveAll(x => x.Index >= caretIndex);            
            _textData.Length = _caretIndex;
            _formats.RemoveRange(_caretIndex, _formats.Count - _caretIndex);
            _modes.RemoveRange(_caretIndex, _modes.Count - _caretIndex);
            FormatText();
        }

        public void AddDecoration(CharacterDecorationType cdt, Position position, string sign)
        {
            if (cdt == CharacterDecorationType.None)
            {
                var decoArray = (from d in _decorations where d.Index == _caretIndex - 1 select d).ToArray();
                UndoManager.AddUndoAction(new DecorationAction(this, decoArray) { UndoFlag = false });
                _decorations.RemoveAll(x => x.Index == _caretIndex - 1);
            }
            else if (!char.IsWhiteSpace(_textData[_caretIndex - 1]))
            {
                var cdi = new CharacterDecorationInfo
                {
                    DecorationType = cdt,
                    Position = position,
                    UnicodeString = sign,
                    Index = _caretIndex - 1
                };
                _decorations.Add(cdi);
                UndoManager.AddUndoAction(new DecorationAction(this, new[] { cdi }));
            }
            FormatText();
        }

        private List<int> FindSymbolIndexes(int start = 0, int limit = 0) // limit is 1 past the last to be checked
        {
            var symIndexes = new List<int>();
            if (ApplySymbolGap)
            {
                if (ParentEquation.GetIndex(this) == 0 && start == 0)
                {
                    start = 1;
                }
                limit = limit > 0 ? limit : _textData.Length;
                for (var i = start; i < limit; i++)
                {
                    var isSymbol = true;
                    if (i > 0 && Symbols.Contains(_textData[i - 1]))
                    {
                        if (i + 1 == _textData.Length || (i + 1 < _textData.Length && !Symbols.Contains(_textData[i + 1])))
                        {
                            isSymbol = false;
                        }
                    }
                    if (Symbols.Contains(_textData[i]) && isSymbol)
                    {
                        symIndexes.Add(i);
                    }
                }
            }
            return symIndexes;
        }

        private void DrawAllDecorations(DrawingContext dc, double left, double hCenter, double top, double charLeft,
                                        FormattedText charFt, int index, List<CharacterDecorationInfo> decorationList)
        {
            DrawRightDecorations(dc, decorationList, top, left + charLeft + charFt.GetFullWidth(), _formats[index]);
            DrawLeftDecorations(dc, decorationList, top, left + charLeft, _formats[index]);
            DrawFaceDecorations(dc, charFt, decorationList, top, left + hCenter);
            DrawTopDecorations(dc, charFt, decorationList, top, left + hCenter, _formats[index]);
            DrawBottomDecorations(dc, charFt, decorationList, top, left + hCenter, _formats[index]);
        }

        /*
        void DrawDecorations(DrawingContext dc, List<CharacterDecorationInfo> decorationList, FormattedText ft, int index, double hCenter)
        {
            double offset = FontSize * .05;
            //character metrics    
            double topPixel = ft.Height + ft.OverhangAfter - ft.Extent; //ft.Baseline - ft.Extent + descent;
            double descent = ft.Height - ft.Baseline + ft.OverhangAfter;
            double halfCharWidth = ft.GetFullWidth() / 2;
            double right = hCenter + halfCharWidth + offset;
            double left = hCenter - halfCharWidth - offset;
            double top = Top + topPixel - offset;
            double bottom = Top + ft.Baseline + descent + offset;
        }
        */
        private void DrawTopDecorations(DrawingContext dc, FormattedText ft, List<CharacterDecorationInfo> cdiList, double top, double center, int formatId)
        {
            var topDecorations = (from x in cdiList where x.Position == Position.Top select x).ToList();
            if (topDecorations.Count > 0)
            {
                top += ft.Height + ft.OverhangAfter - ft.Extent - FontSize * .1;
                foreach (var d in topDecorations)
                {
                    var text = d.UnicodeString;
                    var sign = TextManager.GetFormattedText(text, TextManager.GetFormatIdForNewStyle(formatId, FontStyles.Normal));
                    sign.DrawTextBottomCenterAligned(dc, new Point(center, top));
                    top -= sign.Extent + FontSize * .1;
                }
            }
        }

        private void DrawBottomDecorations(DrawingContext dc, FormattedText ft, List<CharacterDecorationInfo> cdiList, double top, double hCenter, int formatId)
        {
            var bottomDecorations = (from x in cdiList where x.Position == Position.Bottom select x).ToList();
            if (bottomDecorations.Count > 0)
            {
                top += ft.Height + ft.OverhangAfter + FontSize * .1;
                foreach (var d in bottomDecorations)
                {
                    var text = d.UnicodeString;
                    var sign = TextManager.GetFormattedText(text, TextManager.GetFormatIdForNewStyle(formatId, FontStyles.Normal));
                    sign.DrawTextTopCenterAligned(dc, new Point(hCenter, top));
                    top += sign.Extent + FontSize * .1;
                }
            }
        }

        private void DrawLeftDecorations(DrawingContext dc, List<CharacterDecorationInfo> cdiList, double top, double left, int formatId)
        {
            var leftDecorations = (from x in cdiList where x.Position == Position.TopLeft select x).ToList();
            if (leftDecorations.Count > 0)
            {
                var s = "";
                foreach (var d in leftDecorations)
                {
                    s = s + d.UnicodeString;
                }
                var formattedText = TextManager.GetFormattedText(s, TextManager.GetFormatIdForNewStyle(formatId, FontStyles.Normal));
                formattedText.DrawTextRightAligned(dc, new Point(left - FontSize * .05, top));
            }
        }

        private void DrawRightDecorations(DrawingContext dc, List<CharacterDecorationInfo> cdiList, double top, double right, int formatId)
        {
            var rightDecorations = (from x in cdiList where x.Position == Position.TopRight select x).ToList();
            if (rightDecorations.Count > 0)
            {
                var s = "";
                foreach (var d in rightDecorations)
                {
                    s = s + d.UnicodeString;
                }
                var var = TextManager.GetFormattedText(s, TextManager.GetFormatIdForNewStyle(formatId, FontStyles.Normal));
                var.DrawTextLeftAligned(dc, new Point(right, top));
            }
        }

        //index = index of the decorated character in this.textData
        private void DrawFaceDecorations(DrawingContext dc, FormattedText charText, List<CharacterDecorationInfo> cdiList, double top, double hCenter)
        {
            var decorations = (from x in cdiList where x.Position == Position.Over select x).ToList();
            if (decorations.Count > 0)
            {
                double offset = 0;
                top += charText.Height + charText.OverhangAfter - charText.Extent; //ft.Baseline - ft.Extent + descent;
                var bottom = top + charText.Extent; //charText.Height - charText.Baseline + charText.OverhangAfter;
                var vCenter = top + charText.Extent / 2;
                var left = hCenter - charText.GetFullWidth() / 2 - offset;
                var right = hCenter + charText.GetFullWidth() / 2 + offset;

                var pen = PenManager.GetPen(FontSize * .035);
                foreach (var d in decorations)
                {
                    switch (d.DecorationType)
                    {
                        case CharacterDecorationType.Cross:
                            dc.DrawLine(pen, new Point(left, top), new Point(right, bottom));
                            dc.DrawLine(pen, new Point(left, bottom), new Point(right, top));
                            break;
                        case CharacterDecorationType.LeftCross:
                            dc.DrawLine(pen, new Point(left, top), new Point(right, bottom));
                            break;
                        case CharacterDecorationType.RightCross:
                            dc.DrawLine(pen, new Point(left, bottom), new Point(right, top));
                            break;
                        case CharacterDecorationType.LeftUprightCross:
                            dc.DrawLine(pen, new Point(hCenter - FontSize * .08, top - FontSize * 0.04), new Point(hCenter + FontSize * .08, bottom + FontSize * 0.04));
                            break;
                        case CharacterDecorationType.RightUprightCross:
                            dc.DrawLine(pen, new Point(hCenter + FontSize * .08, top - FontSize * 0.04), new Point(hCenter - FontSize * .08, bottom + FontSize * 0.04));
                            break;
                        case CharacterDecorationType.StrikeThrough:
                            dc.DrawLine(pen, new Point(left, vCenter), new Point(right, vCenter));
                            break;
                        case CharacterDecorationType.DoubleStrikeThrough:
                            dc.DrawLine(pen, new Point(left, vCenter - FontSize * .05), new Point(right, vCenter - FontSize * .05));
                            dc.DrawLine(pen, new Point(left, vCenter + FontSize * .05), new Point(right, vCenter + FontSize * .05));
                            break;
                        case CharacterDecorationType.VStrikeThrough:
                            dc.DrawLine(pen, new Point(hCenter, top - FontSize * .05), new Point(hCenter, bottom + FontSize * .05));
                            break;
                        case CharacterDecorationType.VDoubleStrikeThrough:
                            dc.DrawLine(pen, new Point(hCenter - FontSize * .05, top - FontSize * .05), new Point(hCenter - FontSize * .05, bottom + FontSize * .05));
                            dc.DrawLine(pen, new Point(hCenter + FontSize * .05, top - FontSize * .05), new Point(hCenter + FontSize * .05, bottom + FontSize * .05));
                            break;
                    }
                }
            }
        }

        private double GetDecoratedCharWidth(FormattedText ft, List<CharacterDecorationInfo> decorationList,
                                             int index, out double charLeft, out double hCenter)
        {
            charLeft = 0;
            var width = ft.GetFullWidth();
            hCenter = width / 2;
            var charWidth = width;
            var lhList = from d in decorationList where d.Position == Position.TopLeft select d;
            var rhList = from d in decorationList where d.Position == Position.TopRight select d;
            var topList = from d in decorationList where d.Position == Position.Top select d;
            var bottomList = from d in decorationList where d.Position == Position.Bottom select d;
            //var vList = from d in decorationList where d.Position == Position.Top || d.Position == Position.Bottom select d;
            var oList = from d in decorationList where d.Position == Position.Over select d;
            var text = "";
            foreach (var v in lhList)
            {
                text += v.UnicodeString;
            }
            if (text.Length > 0)
            {
                var t = TextManager.GetFormattedText(text, TextManager.GetFormatIdForNewStyle(_formats[index], FontStyles.Normal));
                width += t.GetFullWidth();
                charLeft = t.GetFullWidth();
                hCenter += charLeft;
            }
            foreach (var v in topList)
            {
                var f = TextManager.GetFormattedText("f", _formats[index]);
                var fTop = f.TopExtra();
                var ftTop = ft.TopExtra();
                if (fTop < ftTop)
                {
                    var t = TextManager.GetFormattedText(v.UnicodeString, _formats[index]);
                    var diff = t.GetFullWidth() - charWidth;
                    if (diff > 0)
                    {
                        charWidth += diff;
                        charLeft += diff / 2;
                        hCenter += diff / 2;
                    }
                }
            }
            foreach (var v in bottomList)
            {
                var t = TextManager.GetFormattedText(v.UnicodeString, _formats[index]);
                var diff = t.GetFullWidth() - charWidth;
                if (diff > 0)
                {
                    charWidth += diff;
                    charLeft += diff / 2;
                    hCenter += diff / 2;
                }
            }
            width = Math.Max(width, charWidth);
            foreach (var v in oList)
            {
                if (v.DecorationType == CharacterDecorationType.Cross || v.DecorationType == CharacterDecorationType.DoubleStrikeThrough ||
                                v.DecorationType == CharacterDecorationType.LeftCross || v.DecorationType == CharacterDecorationType.RightCross ||
                                v.DecorationType == CharacterDecorationType.StrikeThrough)
                {
                    var diff = (ft.GetFullWidth() + FontSize * .1) - width;
                    if (diff > 0)
                    {
                        width = ft.GetFullWidth() + FontSize * .1;
                        charLeft += diff / 2;
                        hCenter += diff / 2;
                    }
                }
            }
            text = "";
            foreach (var v in rhList)
            {
                text += v.UnicodeString;
            }
            if (text.Length > 0)
            {
                var t = TextManager.GetFormattedText(text, _formats[index]);
                width += t.GetFullWidth();
            }
            return width;
        }

        public double OverhangTrailing
        {
            get
            {
                if (_textData.Length > 0 && !char.IsWhiteSpace(_textData[_textData.Length - 1]))
                {
                    var ft = TextManager.GetFormattedText(_textData[_textData.Length - 1].ToString(), _formats[_formats.Count - 1]);
                    return ft.OverhangTrailing;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double OverhangAfter
        {
            get
            {
                if (_textData.Length > 0)
                {
                    var ft = TextManager.GetFormattedText(_textData.ToString(), _formats);
                    return ft.OverhangAfter;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double GetCornerDescent(Position position)
        {
            if (_textData.Length > 0)
            {
                if (position == Position.Right)
                {
                    if (!char.IsWhiteSpace(_textData[_textData.Length - 1]))
                    {
                        var ft = TextManager.GetFormattedText(_textData[_textData.Length - 1].ToString(), _formats[_formats.Count - 1]);
                        return ft.Descent();
                    }
                }
                else
                {
                    if (!char.IsWhiteSpace(_textData[0]))
                    {
                        var ft = TextManager.GetFormattedText(_textData[0].ToString(), _formats[0]);
                        return ft.Descent();
                    }
                }
            }
            return 0;
        }

        //lolz.. I had to delete and recreate this method, as VS was raising an error that "not all code paths return a value"!!
        public override HashSet<int> GetUsedTextFormats()
        {
            return new HashSet<int>(_formats);
        }

        public override void ResetTextFormats(Dictionary<int, int> formatMapping)
        {
            for (var i = 0; i < _formats.Count; i++)
            {
                _formats[i] = formatMapping[_formats[i]];
            }
        }
    }
}
