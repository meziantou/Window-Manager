using System.Diagnostics.Contracts;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;

namespace WindowManager.Core.Settings
{
    public class HotKeys
    {
        [XmlIgnore]
        public string Name { get; set; }
        public HotKey CenterHotKey { get; set; }
        public HotKey HorizontalCenterHotKey { get; set; }
        public HotKey VerticalCenterHotKey { get; set; }
        public HotKey BottomMostHotKey { get; set; }
        public HotKey TopMostHotKey { get; set; }
        public HotKey ShowSizeSelectionWindowHotKey { get; set; }
        public HotKey SwitchScreenHotKey { get; set; }
        public HotKey TopLeftHotKey { get; set; }
        public HotKey TopHotKey { get; set; }
        public HotKey TopRightHotKey { get; set; }
        public HotKey LeftHotKey { get; set; }
        public HotKey RightHotKey { get; set; }
        public HotKey BottomLeftHotKey { get; set; }
        public HotKey BottomRightHotKey { get; set; }
        public HotKey FullScreenHotKey { get; set; }
        public HotKey BottomHotKey { get; set; }
        public HotKey MinimizeWindowHotKey { get; set; }
        public HotKey ExtendBottomHotKey { get; set; }
        public HotKey ExtendTopHotKey { get; set; }
        public HotKey ExtendLeftHotKey { get; set; }
        public HotKey ExtendRightHotKey { get; set; }
        public HotKey ReduceBottomHotKey { get; set; }
        public HotKey ReduceTopHotKey { get; set; }
        public HotKey ReduceLeftHotKey { get; set; }
        public HotKey ReduceRightHotKey { get; set; }

        public static HotKeys GetDefault()
        {
            Contract.Ensures(Contract.Result<HotKeys>() != null);

            HotKeys hotKeys = new HotKeys();
            hotKeys.Name = "Default";
            hotKeys.BottomLeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad1);
            hotKeys.BottomHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad2);
            hotKeys.BottomRightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad3);
            hotKeys.LeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad4);
            hotKeys.FullScreenHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad5);
            hotKeys.RightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad6);
            hotKeys.TopLeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad7);
            hotKeys.TopHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad8);
            hotKeys.TopRightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad9);
            hotKeys.SwitchScreenHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad0);
            hotKeys.ShowSizeSelectionWindowHotKey = new HotKey(ModifierKeys.Alt, Keys.Space);
            hotKeys.HorizontalCenterHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad4);
            hotKeys.VerticalCenterHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad8);
            hotKeys.CenterHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad5);
            hotKeys.TopMostHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad0);
            hotKeys.BottomMostHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad1);
            hotKeys.MinimizeWindowHotKey = new HotKey(ModifierKeys.Alt, Keys.NumPad2);
            //hotKeys.ExtendBottomHotKey = new HotKey(ModifierKeys.Windows, Keys.Down);
            //hotKeys.ExtendTopHotKey = new HotKey(ModifierKeys.Windows, Keys.Up);
            //hotKeys.ExtendLeftHotKey = new HotKey(ModifierKeys.Windows, Keys.Left);
            //hotKeys.ExtendRightHotKey = new HotKey(ModifierKeys.Windows, Keys.Right);
            //hotKeys.ReduceBottomHotKey = new HotKey(ModifierKeys.Alt, Keys.Up);
            //hotKeys.ReduceTopHotKey = new HotKey(ModifierKeys.Alt, Keys.Down);
            //hotKeys.ReduceLeftHotKey = new HotKey(ModifierKeys.Alt, Keys.Right);
            //hotKeys.ReduceRightHotKey = new HotKey(ModifierKeys.Alt, Keys.Left);

            return hotKeys;
        }
    }
}