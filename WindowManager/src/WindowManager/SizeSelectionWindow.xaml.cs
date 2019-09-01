using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using WindowManager.Core.Settings;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace WindowManager
{
    /// <summary>
    ///   Interaction logic for SizeSelectionWindow.xaml
    /// </summary>
    public sealed partial class SizeSelectionWindow : Window
    {
        private readonly int _nbItemsPerRow = 10;
        private readonly int _nbItemsPerColumn = 10;
        private readonly WindowManagerSettings _settings;
        private readonly Screen _screen;
        public event SizeSelectedEventHandler SizeSelected;
        private Point _startPoint;
        private readonly List<ScreenItem> _screenItems;

        public SizeSelectionWindow(Screen screen, int nbItemsPerRow, int nbItemsPerColumn, WindowManagerSettings settings)
        {
            _screen = screen;
            _nbItemsPerRow = nbItemsPerRow;
            _nbItemsPerColumn = nbItemsPerColumn;
            _settings = settings;

            InitializeComponent();

            Resources["foregroundBrush"] = new SolidColorBrush(settings.Theme.ForegroundColor);
            Resources["selectionBorderBrush"] = new SolidColorBrush(settings.Theme.SelectionBorderColor);
            Resources["selectionFillBrush"] = new SolidColorBrush(settings.Theme.SelectionFillColor);
            Background = new SolidColorBrush(settings.Theme.BackgroundColor);
            var selectionViewModel = new SizeSelectionViewModel
            {
                NbColumns = NbItemsPerRow,
                NbRows = NbItemsPerColumn,
            };
            _screenItems = new List<ScreenItem>();
            for (var position = 0; position < NbItemsPerColumn * NbItemsPerRow; position++)
                _screenItems.Add(new ScreenItem(screen, position));

            selectionViewModel.ScreenItems = _screenItems;
            DataContext = selectionViewModel;
        }

        void SizeSelectionWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Theme.SwitchWindowFullScreen)
            {
                Left = _screen.WorkingArea.X;
                Top = _screen.WorkingArea.Y;
                Width = _screen.WorkingArea.Width;
                Height = _screen.WorkingArea.Height;
            }
            else
            {
                if (NbItemsPerColumn > NbItemsPerRow)
                {
                    Width = ActualWidth / NbItemsPerColumn * NbItemsPerRow;
                }
                else if (NbItemsPerColumn < NbItemsPerRow)
                {
                    Height = ActualHeight / NbItemsPerRow * NbItemsPerColumn;
                }

                Left = _screen.WorkingArea.X + (_screen.WorkingArea.Width - ActualWidth) / 2.0;
                Top = _screen.WorkingArea.Y + (_screen.WorkingArea.Height - ActualHeight) / 4.0 * 3.0;
            }
        }

        public Screen Screen => _screen;

        public int NbItemsPerRow => _nbItemsPerRow;

        public int NbItemsPerColumn => _nbItemsPerColumn;

        private void OnSizeSelected(SizeSelectedEventArgs e)
        {
            SizeSelectedEventHandler selectedEventHandler = SizeSelected;
            if (selectedEventHandler == null)
                return;
            selectedEventHandler(this, e);
        }

        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas.CaptureMouse();
            _startPoint = e.GetPosition((IInputElement)sender);

            Canvas.SetLeft(rectangle, _startPoint.X);
            Canvas.SetTop(rectangle, _startPoint.X);
        }

        private Rectangle GetSelection()
        {
            var modX = canvas.ActualWidth / NbItemsPerRow;
            var modY = canvas.ActualHeight / NbItemsPerColumn;

            var left = Canvas.GetLeft(rectangle);
            var top = Canvas.GetTop(rectangle);

            var x = (int)(left / modX);
            var y = (int)(top / modY);
            var width = (int)((left + rectangle.Width) / modX) + 1 - x;
            var height = (int)((top + rectangle.Height) / modY) + 1 - y;

            if (width + x > NbItemsPerRow)
                width = NbItemsPerRow - x;

            if (height + y > NbItemsPerColumn)
                height = NbItemsPerColumn - y;

            return new Rectangle(x, y, width, height);
        }

        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle selection = GetSelection();

            var maximized = selection.Width == NbItemsPerRow && selection.Height == NbItemsPerColumn;

            var widthPerUnit = _screen.WorkingArea.Width / (double)NbItemsPerRow;
            var heightPerUnit = _screen.WorkingArea.Height / (double)NbItemsPerColumn;

            var x = (int)(_screen.WorkingArea.Left + selection.X * widthPerUnit);
            var y = (int)(_screen.WorkingArea.Top + selection.Y * heightPerUnit);
            var width = (int)(selection.Width * widthPerUnit);
            var height = (int)(selection.Height * heightPerUnit);

            var rect = new Rectangle(x, y, width, height);
            OnSizeSelected(new SizeSelectedEventArgs(rect, maximized));
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rectangle == null)
                return;

            // Move rectangle
            Point pos = e.GetPosition((IInputElement)sender);

            var x = Math.Min(pos.X, _startPoint.X);
            var y = Math.Min(pos.Y, _startPoint.Y);
            var w = Math.Max(pos.X, _startPoint.X) - x;
            var h = Math.Max(pos.Y, _startPoint.Y) - y;

            rectangle.Width = w;
            rectangle.Height = h;

            if (x < 0)
                x = 0;

            if (y < 0)
                y = 0;

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            // Change selection
            Rectangle selection = GetSelection();

            foreach (ScreenItem t in _screenItems)
            {
                x = t.Position % NbItemsPerRow;
                // ReSharper disable PossibleLossOfFraction
                y = t.Position / NbItemsPerRow;
                // ReSharper restore PossibleLossOfFraction
                t.IsSelected = x >= selection.X && x < selection.Width + selection.X
                               && y >= selection.Y && y < selection.Y + selection.Height;
            }
        }

        private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}