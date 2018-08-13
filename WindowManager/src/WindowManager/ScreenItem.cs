using System.ComponentModel;
using System.Windows.Forms;

namespace WindowManager
{
    public class ScreenItem : INotifyPropertyChanged
    {
        public ScreenItem(Screen screen, int position)
        {
            Screen = screen;
            Position = position;
        }

        public Screen Screen { get; private set; }

        public int Position { get; private set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}