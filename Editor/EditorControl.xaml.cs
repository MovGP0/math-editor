using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace Editor
{
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    public partial class EditorControl : IDisposable
    {
        private readonly System.Threading.Timer _timer;
        private readonly int _blinkPeriod = 600;

        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        public event EventHandler ZoomChanged = (x, y) => { };
        public bool Dirty { get; set; }
        private EquationRoot _equationRoot;
        private readonly Caret _vCaret = new Caret(false);
        private readonly Caret _hCaret = new Caret(true);
        private readonly double _fontSize = EditorControlGlobal.RootFontBaseSize;
        
        public EditorControl()
        {
            InitializeComponent();
            mainGrid.Children.Add(_vCaret);
            mainGrid.Children.Add(_hCaret);
            _equationRoot = new EquationRoot(_vCaret, _hCaret)
            {
                FontSize = _fontSize
            };
            _timer = new System.Threading.Timer(BlinkCaret, null, _blinkPeriod, _blinkPeriod);
        }

        public void SetFontSizePercentage(int percentage)
        {            
            _equationRoot.FontSize = _fontSize * percentage / 100;
            EditorControlGlobal.RootFontSize = _equationRoot.FontSize;
            AdjustView();
        }

        public void ShowOverbar(bool show)
        {
            if (!show)
            {
                _hCaret.Visibility = Visibility.Hidden;
            }
            else
            {
                _hCaret.Visibility = Visibility.Visible;
            }
        }

        private void BlinkCaret(object state)
        {
            _vCaret.ToggleVisibility();
            _hCaret.ToggleVisibility();
        }

        public void HandleUserCommand(CommandDetails commandDetails)
        {
            _equationRoot.HandleUserCommand(commandDetails);
            AdjustView();
            Dirty = true;
        }

        public void SaveFile(Stream stream, string fileName)
        {
            //equationRoot.SaveFile(stream);
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    _equationRoot.SaveFile(memoryStream);
                    memoryStream.Position = 0;
                    ZipStream(memoryStream, stream, Path.GetFileNameWithoutExtension(fileName) + ".xml");
                }
            }
            catch
            {
                MessageBox.Show("Could not save file. Make sure the specified path is correct.", "Error");
            }
            Dirty = false;
        }

        public void ZipStream(MemoryStream memStreamIn, Stream outputStream, string zipEntryName)
        {
            var zipStream = new ZipOutputStream(outputStream);
            zipStream.SetLevel(5); //0-9, 9 being the highest level of compression
            var newEntry = new ZipEntry(zipEntryName) {DateTime = DateTime.Now};
            zipStream.PutNextEntry(newEntry);
            StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
            zipStream.CloseEntry();
            zipStream.IsStreamOwner = false;	// False stops the Close also Closing the underlying stream.
            zipStream.Close();			// Must finish the ZipOutputStream before using outputMemStream.            
        }

        public void LoadFile(Stream stream)
        {
            //equationRoot.LoadFile(stream);
            try
            {
                var zipInputStream = new ZipInputStream(stream);
                var zipEntry = zipInputStream.GetNextEntry();
                var outputStream = new MemoryStream();
                if (zipEntry != null)
                {
                    var buffer = new byte[4096];
                    StreamUtils.Copy(zipInputStream, outputStream, buffer);
                }
                outputStream.Position = 0;
                using (outputStream)
                {
                    _equationRoot.LoadFile(outputStream);
                }
            }
            catch
            {
                stream.Position = 0;
                _equationRoot.LoadFile(stream);
                //MessageBox.Show("Cannot open the specified file. The file is not in correct format.", "Error");
            }
            AdjustView();
            Dirty = false;
        }

        private bool _isDragging;

        private void EditorControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_equationRoot.ConsumeMouseClick(Mouse.GetPosition(this)))
            {
                InvalidateVisual();
            }
            Focus();
            _lastMouseLocation = e.GetPosition(this);
            _isDragging = true;
        }

        private void EditorControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        private void EditorControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void EditorControl_MouseLeave(object sender, MouseEventArgs e)
        {
            StatusBarHelper.ShowCoordinates("");
        }

        private Point _lastMouseLocation;

        private void EditorControl_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            StatusBarHelper.ShowCoordinates((int)mousePosition.X + ", " + (int)mousePosition.Y);

            if (!_isDragging) return;
            if (IsLargeDrag(_lastMouseLocation, mousePosition)) return;

            _equationRoot.HandleMouseDrag(mousePosition);
            _lastMouseLocation = mousePosition;
            InvalidateVisual();
        }

        private static bool IsLargeDrag(Point lastMouseLocation, Point mousePosition)
        {
            return (Math.Abs(lastMouseLocation.X - mousePosition.X) <= 2) &&
                   (Math.Abs(lastMouseLocation.Y - mousePosition.Y) <= 2);
        }

        public void DeleteSelection()
        {
            _equationRoot.RemoveSelection(true);
            InvalidateVisual();
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //equationRoot.DrawEquation(drawingContext);
            var scrollViewer = Parent as ScrollViewer;
            _equationRoot.DrawVisibleRows(drawingContext, scrollViewer.VerticalOffset, scrollViewer.ViewportHeight + scrollViewer.VerticalOffset);
        }

        public void EditorControl_TextInput(object sender, TextCompositionEventArgs e)
        {
            ConsumeText(e.Text.Replace('-', '\u2212'));
        }

        public void ConsumeText(string text)
        {
            _equationRoot.ConsumeText(text);
            AdjustView();
            Dirty = true;
        }

        private void EditorControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void EditorControl_KeyDown(object sender, KeyEventArgs e)
        {
            var handled = false;
            if (e.Key == Key.Tab)
            {
                _equationRoot.ConsumeText("    ");
                handled = true;
            }
            else if (_equationRoot.ConsumeKey(e.Key))
            {
                handled = true;
            }
            if (handled)
            {
                e.Handled = true;
                AdjustView();
                Dirty = true;
            }
        }

        private void AdjustView()
        {
            DetermineSize();
            AdjustScrollViewer();
            InvalidateVisual();
        }

        private void DetermineSize()
        {
            MinWidth = _equationRoot.Width + 50;
            MinHeight = _equationRoot.Height + 20;
        }

        private void AdjustScrollViewer()
        {
            var scrollViewer = Parent as ScrollViewer;
            //Vector offsetPoint = VisualTreeHelper.GetOffset(this);           

            if (scrollViewer != null)
            {
                var left = scrollViewer.HorizontalOffset;
                var top = scrollViewer.VerticalOffset;
                var right = scrollViewer.ViewportWidth + scrollViewer.HorizontalOffset;
                var bottom = scrollViewer.ViewportHeight + scrollViewer.VerticalOffset;
                double hOffset = 0;
                double vOffset = 0;
                var rightDone = false;
                var bottomDone = false;
                while (_vCaret.Left > right - 8)
                {
                    hOffset += 8;
                    right += 8;
                    rightDone = true;
                }
                while (_vCaret.VerticalCaretBottom > bottom - 10)
                {
                    vOffset += 10;
                    bottom += 10;
                    bottomDone = true;
                }
                while (_vCaret.Left < left + 8 && !rightDone)
                {
                    hOffset -= 8;
                    left -= 8;
                }
                while (_vCaret.Top < top + 10 && !bottomDone)
                {
                    vOffset -= 10;
                    top -= 10;
                }
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + hOffset);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + vOffset);
            }
        }

        public void Undo()
        {
            UndoManager.Undo();
            AdjustView();
            Dirty = true;
            _equationRoot.AdjustCarets();
        }

        public void Redo()
        {
            UndoManager.Redo();
            AdjustView();
            Dirty = true;
            _equationRoot.AdjustCarets();
        }

        public void ExportImage(string filePath)
        {
            _equationRoot.SaveImageToFile(filePath);
            /*//clip 1
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Default);
                bmp.Render(drawingVisual);

            }
            //clip 2
            var image = Clipboard.GetImage();
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

            //clip 3

            Rect rect = new Rect(this.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(this);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes("logo.png", ms.ToArray());
            Console.WriteLine("Done");
            */
        }

        public void ZoomOut()
        {
            ZoomChanged(this, EventArgs.Empty);
            _equationRoot.ZoomOut(4);
            EditorControlGlobal.RootFontSize = _equationRoot.FontSize;
            AdjustView();
        }

        public void ZoomIn()
        {
            ZoomChanged(this, EventArgs.Empty);
            _equationRoot.ZoomIn(4);
            EditorControlGlobal.RootFontSize = _equationRoot.FontSize;
            AdjustView();
        }

        private void ZoomOutHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut();
        }

        private void ZoomInHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn();
        }

        public void Copy(bool cut)
        {
            _equationRoot.Copy(cut);
            if (cut)
            {
                AdjustView();
            }
        }

        public void Paste()
        {
            if (!_equationRoot.PasteFromClipBoard()) return;

            AdjustView();
            Dirty = true;
        }

        public void SelectAll()
        {
            _equationRoot.SelectAll();
            InvalidateVisual();
        }

        public void ChangeFont(FontType fontType)
        {
            _equationRoot.ChangeFont(fontType);
            InvalidateVisual();
        }

        public void ChangeFormat(string operation, string argument, bool applied)
        {
            _equationRoot.ModifySelection(operation, argument, applied, true);
            AdjustView();
            Dirty = true;
        }

        public void Clear()
        {
            _equationRoot = new EquationRoot(_vCaret, _hCaret) {FontSize = _fontSize};
            EditorControlGlobal.RootFontSize = _fontSize;
            Dirty = false;
            AdjustView();
        }
    }
}
