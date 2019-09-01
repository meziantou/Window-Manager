using System.Diagnostics.Contracts;
using System.Windows.Media;

namespace WindowManager.Core.Settings
{
    public sealed class Theme
    {
        public int NbItemsPerRow { get; set; }
        public int NbItemsPerColumn { get; set; }
        public bool SwitchWindowFullScreen { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color SelectionBorderColor { get; set; }
        public Color SelectionFillColor { get; set; }

        public static Theme GetDefault()
        {
            Contract.Ensures(Contract.Result<Theme>() != null);

            var theme = new Theme
            {
                NbItemsPerRow = 10,
                NbItemsPerColumn = 10,
                SwitchWindowFullScreen = false,
                BackgroundColor = Color.FromArgb(1, 255, 255, 255),
                ForegroundColor = Color.FromArgb(255, 0, 0, 0),
                SelectionBorderColor = Color.FromArgb(175, 0, 0, 255),
                SelectionFillColor = Color.FromArgb(0xA0, 0, 89, 255),
            };

            return theme;
        }
    }
}