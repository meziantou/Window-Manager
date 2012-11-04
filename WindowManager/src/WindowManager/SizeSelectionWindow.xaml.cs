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
    public partial class SizeSelectionWindow : Window
    {
        private readonly int _nbItemsPerRow = 10;
        private readonly int _nbItemsPerColumn = 10;
        private readonly Settings _settings;
        private readonly Screen _screen;
        public event SizeSelectedEventHandler SizeSelected;
        private Point _startPoint;
        private readonly List<ScreenItem> _screenItems;

        public SizeSelectionWindow(Screen screen, int nbItemsPerRow, int nbItemsPerColumn, Settings settings)
        {
            _screen = screen;
            _nbItemsPerRow = nbItemsPerRow;
            _nbItemsPerColumn = nbItemsPerColumn;
            _settings = settings;

            InitializeComponent();

            this.Resources["foregroundBrush"] = new SolidColorBrush(settings.Theme.ForegroundColor);
            this.Resources["selectionBorderBrush"] = new SolidColorBrush(settings.Theme.SelectionBorderColor);
            this.Resources["selectionFillBrush"] = new SolidColorBrush(settings.Theme.SelectionFillColor);
            this.Background = new SolidColorBrush(settings.Theme.BackgroundColor);
            SizeSelectionViewModel selectionViewModel = new SizeSelectionViewModel();
            selectionViewModel.NbColumns = NbItemsPerRow;
            selectionViewModel.NbRows = NbItemsPerColumn;
            _screenItems = new List<ScreenItem>();
            for (int position = 0; position < NbItemsPerColumn * NbItemsPerRow; position++)
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

        public Screen Screen
        {
            get { return _screen; }
        }

        public int NbItemsPerRow
        {
            get { return _nbItemsPerRow; }
        }

        public int NbItemsPerColumn
        {
            get { return _nbItemsPerColumn; }
        }

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
            double modX = canvas.ActualWidth / NbItemsPerRow;
            double modY = canvas.ActualHeight / NbItemsPerColumn;

            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);

            int x = (int)(left / modX);
            int y = (int)(top / modY);
            int width = (int)((left + rectangle.Width) / modX) + 1 - x;
            int height = (int)((top + rectangle.Height) / modY) + 1 - y;

            if (width + x > NbItemsPerRow)
                width = NbItemsPerRow - x;

            if (height + y > NbItemsPerColumn)
                height = NbItemsPerColumn - y;

            return new Rectangle(x, y, width, height);
        }

        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle selection = GetSelection();

            bool maximized = selection.Width == NbItemsPerRow && selection.Height == NbItemsPerColumn;

            double widthPerUnit = _screen.WorkingArea.Width / (double)NbItemsPerRow;
            double heightPerUnit = _screen.WorkingArea.Height / (double)NbItemsPerColumn;

            int x = (int)(_screen.WorkingArea.Left + selection.X * widthPerUnit);
            int y = (int)(_screen.WorkingArea.Top + selection.Y * heightPerUnit);
            int width = (int)((selection.Width) * widthPerUnit);
            int height = (int)(selection.Height * heightPerUnit);

            Rectangle rect = new Rectangle(x, y, width, height);
            OnSizeSelected(new SizeSelectedEventArgs(rect, maximized));
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rectangle == null)
                return;

            // Move rectangle
            Point pos = e.GetPosition((IInputElement)sender);

            double x = Math.Min(pos.X, _startPoint.X);
            double y = Math.Min(pos.Y, _startPoint.Y);
            double w = Math.Max(pos.X, _startPoint.X) - x;
            double h = Math.Max(pos.Y, _startPoint.Y) - y;

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
                this.Close();
            }
        }
    }
}