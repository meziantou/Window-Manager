using System.Diagnostics.Contracts;
using System.Windows.Media;
using System.Xml.Serialization;

namespace WindowManager.Core.Settings
{
    public class Theme
    {
        [XmlIgnore]
        public string Name { get; set; }
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

            var theme = new Theme();
            theme.Name = "Default";
            theme.NbItemsPerRow = 10;
            theme.NbItemsPerColumn = 10;
            theme.SwitchWindowFullScreen = false;
            theme.BackgroundColor = Color.FromArgb(1, 255, 255, 255);
            theme.ForegroundColor = Color.FromArgb(255, 0, 0, 0);
            theme.SelectionBorderColor = Color.FromArgb(175, 0, 0, 255);
            theme.SelectionFillColor = Color.FromArgb(0xA0, 0, 89, 255);

            return theme;
        }
    }
}