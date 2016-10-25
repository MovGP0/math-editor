﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;

namespace Editor
{
    public sealed class EquationRoot : EquationContainer
    {
        Caret vCaret;
        Caret hCaret;
        string fileVersion = "1.4";
        string sessionString = Guid.NewGuid().ToString();

        public EquationRoot(Caret vCaret, Caret hCaret)
            : base(null)
        {
            ApplySymbolGap = true;
            this.vCaret = vCaret;
            this.hCaret = hCaret;
            ActiveChild = new RowContainer(this, 0.3);
            ChildEquations.Add(ActiveChild);            
            ActiveChild.Location = Location = new Point(15, 15);
            AdjustCarets();
        }

        public override void ChildCompletedUndo(IEquationBase child)
        {
            CalculateSize();
            AdjustCarets();
        }

        public void SaveFile(Stream stream)
        {
            var xDoc = new XDocument();
            var root = new XElement(GetType().Name); //ActiveChild.Serialize();
            root.Add(new XAttribute("fileVersion", fileVersion));
            root.Add(new XAttribute("appVersion", Assembly.GetEntryAssembly().GetName().Version));
            TextManager.OptimizeForSave(this);
            root.Add(TextManager.Serialize());
            root.Add(ActiveChild.Serialize());
            xDoc.Add(root);
            xDoc.Save(stream);
            TextManager.RestoreAfterSave(this);
        }

        public void LoadFile(Stream stream)
        {
            UndoManager.ClearAll();
            DeSelect();
            var xDoc = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            var root = xDoc.Root;
            XAttribute fileVersionAttribute;
            XAttribute appVersionAttribute;

            if (root.Name == GetType().Name)
            {
                var formattingElement = root.Element("TextManager");
                TextManager.DeSerialize(formattingElement);
                fileVersionAttribute = root.Attributes("fileVersion").FirstOrDefault();
                appVersionAttribute = root.Attributes("appVersion").FirstOrDefault();
                root = root.Element("RowContainer");
            }
            else
            {
                fileVersionAttribute = root.Attributes("fileVersion").FirstOrDefault();
                appVersionAttribute = root.Attributes("appVersion").FirstOrDefault();
            }            
            var appVersion = appVersionAttribute != null ? appVersionAttribute.Value : "Unknown";
            if (fileVersionAttribute == null || fileVersionAttribute.Value != fileVersion)
            {
                MessageBox.Show("The file was created by a different version (v." + appVersion + ") of Math Editor and uses a different format." + Environment.NewLine + Environment.NewLine +
                                "Math Editor will still try to open and convert the file to the current version. The operation may fail. " + Environment.NewLine + Environment.NewLine +
                                "Please create a backup if you want to keep the original file intact.", "Message");
            }
            ActiveChild.DeSerialize(root);
            CalculateSize();
            AdjustCarets();
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (!IsSelecting)
            {
                ActiveChild.StartSelection();
                IsSelecting = true;
            }
            ActiveChild.HandleMouseDrag(mousePoint);
            AdjustCarets();
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                IsSelecting = true;
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(mousePoint);                
            }
            else
            {
                ActiveChild.ConsumeMouseClick(mousePoint); //never forget, EquationRoot has only one child at all times!!                
                IsSelecting = true; //else DeSelect() might not work!
                DeSelect();
            }
            AdjustCarets();
            return true;
        }

        public override void SelectAll()
        {
            DeSelect();
            ActiveChild.SelectAll();
            if (!IsSelecting)
            {
                IsSelecting = true;
            }
        }
                
        public void HandleUserCommand(CommandDetails commandDetails)
        {            
            if (commandDetails.CommandType == CommandType.Text)
            {
                ConsumeText(commandDetails.UnicodeString); //ConsumeText() will call DeSelect() itself. No worries here
            }
            else
            {
                var undoCount = UndoManager.UndoCount + 1;
                if (IsSelecting)
                {
                    ActiveChild.RemoveSelection(true);
                }
                ((IEquationContainer)ActiveChild).ExecuteCommand(commandDetails.CommandType, commandDetails.CommandParam);
                if (IsSelecting && undoCount < UndoManager.UndoCount)
                {
                    UndoManager.ChangeUndoCountOfLastAction(1);
                }
                CalculateSize();
                AdjustCarets();
                DeSelect();                
            }
        }

        public void AdjustCarets()
        {
            vCaret.Location = ActiveChild.GetVerticalCaretLocation();
            vCaret.CaretLength = ActiveChild.GetVerticalCaretLength();
            var innerMost = ((IRowContainer)ActiveChild).GetInnerMostEquationContainer();
            hCaret.Location = innerMost.GetHorizontalCaretLocation();
            hCaret.CaretLength = innerMost.GetHorizontalCaretLength();
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            var temp = base.Copy(removeSelection);
            var data = new DataObject();
            data.SetImage(temp.Image);
            var rootElement = new XElement(this.GetType().Name);
            rootElement.Add(new XElement("SessionId", sessionString));
            rootElement.Add(TextManager.Serialize());
            rootElement.Add(new XElement("payload", temp.XElement));
            var med = new MathEditorData { XmlString = rootElement.ToString() };
            data.SetData(med);
            if (temp.Text != null)
            {
                data.SetText(temp.Text);
            }
            Clipboard.SetDataObject(data, true);
            if (removeSelection)
            {
                DeSelect();
                AdjustCarets();
            }
            return temp;
        }

        public override void Paste(XElement xe)
        {
            var id = xe.Element("SessionId").Value;
            if (id != sessionString)
            {
                TextManager.ProcessPastedXML(xe);
            }
            var undoCount = UndoManager.UndoCount + 1;
            if (IsSelecting)
            {
                ActiveChild.RemoveSelection(true);
            }
            ActiveChild.Paste(xe.Element("payload").Elements().First());
            if (IsSelecting && undoCount < UndoManager.UndoCount)
            {
                UndoManager.ChangeUndoCountOfLastAction(1);
            }
            CalculateSize();
            AdjustCarets();
            DeSelect();
        }

        public bool PasteFromClipBoard()
        {
            var success = false;
            MathEditorData data = null;
            var text = "";
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    if (Clipboard.ContainsData(typeof(MathEditorData).FullName))
                    {
                        data = Clipboard.GetData(typeof(MathEditorData).FullName) as MathEditorData;                        
                        break;
                    }
                    if (Clipboard.ContainsText())
                    {
                        text = Clipboard.GetText();
                        break;
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            try
            {
                if (data != null)
                {
                    var element = XElement.Parse(data.XmlString, LoadOptions.PreserveWhitespace);
                    Paste(element);
                    success = true;
                }
                else if (!string.IsNullOrEmpty(text))
                {
                    ConsumeText(text);
                    success = true;
                }
            }
            catch 
            {
                success = false;
            }
            return success;
        }

        public override void ConsumeText(string text)
        {
            var undoCount = UndoManager.UndoCount + 1;
            if (IsSelecting)
            {
                ActiveChild.RemoveSelection(true);
            }
            ActiveChild.ConsumeText(text);
            if (IsSelecting && undoCount < UndoManager.UndoCount)
            {
                UndoManager.ChangeUndoCountOfLastAction(1);
            }
            CalculateSize();
            AdjustCarets();
            DeSelect();
        }

        public override void DeSelect()
        {
            if (IsSelecting)
            {
                base.DeSelect();
                IsSelecting = false;
            }
        }

        public void DrawVisibleRows(DrawingContext dc, double top, double bottom)
        {
            ((RowContainer)ActiveChild).DrawVisibleRows(dc, top, bottom);
        }

        public void SaveImageToFile(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                if (extension == ".bmp" || extension == "jpg")
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, Math.Ceiling(Width + Location.X * 2), Math.Ceiling(Width + Location.Y * 2)));
                }
                ActiveChild.DrawEquation(dc);                
            }
            var bitmap = new RenderTargetBitmap((int)(Math.Ceiling(Width + Location.X * 2)), (int)(Math.Ceiling(Height + Location.Y * 2)), 96, 96, PixelFormats.Default);
            bitmap.Render(dv);
            BitmapEncoder encoder = null;
            switch (extension)
            {
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".wdp":
                    encoder = new WmpBitmapEncoder();
                    break;
                case ".tif":
                    encoder = new TiffBitmapEncoder();
                    break;
            }
            try
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (Stream s = File.Create(path))
                {
                    encoder.Save(s);
                }
            }
            catch
            {
                MessageBox.Show("File could not be saved. Please make sure the path you entered is correct", "Error");
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (new[] { Key.Right, Key.Left, Key.Up, Key.Down, Key.Home, Key.End }).Contains(key))
            {
                if (!IsSelecting)
                {
                    IsSelecting = true;
                    ((RowContainer)ActiveChild).StartSelection();
                }
                ActiveChild.Select(key);
                AdjustCarets();
                return true;
            }
            Key[] handledKeys = { Key.Left, Key.Right, Key.Delete, Key.Up, Key.Down, Key.Enter, Key.Escape, Key.Back, Key.Home, Key.End };
            var result = false;
            if (handledKeys.Contains(key))
            {
                result = true;
                if (IsSelecting && (new[] { Key.Delete, Key.Enter, Key.Back }).Contains(key))
                {
                    ActiveChild.RemoveSelection(true);
                }
                else
                {
                    ActiveChild.ConsumeKey(key);                    
                }
                CalculateSize();
                AdjustCarets();
                DeSelect();                
            }
            return result;
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (IsSelecting)
            {
                ActiveChild.RemoveSelection(registerUndo);
                CalculateSize();
                AdjustCarets();
                DeSelect();
            }            
        }

        public override void CalculateWidth()
        {
            Width = ActiveChild.Width;
        }

        public override void CalculateHeight()
        {
            Height = ActiveChild.Height;
        }

        public void ZoomOut(int difference)
        {
            FontSize -= difference;            
        }

        public void ZoomIn(int difference)
        {
            FontSize += difference;
        }

        public void ChangeFont(FontType fontType)
        {
            TextEquation.FontType = fontType;
            ActiveChild.FontSize = FontSize;
            CalculateSize();
            AdjustCarets();
        }

        public override double FontSize
        {
            get { return base.FontSize; }
            set
            {
                base.FontSize = value;                
                AdjustCarets();
            }
        }
    }
}
