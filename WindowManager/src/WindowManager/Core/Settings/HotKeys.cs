using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager.Core.Settings
{
    public sealed class HotKeys
    {
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
        public HotKey IncreaseTransparencyHotKey { get; set; }
        public HotKey DecreaseTransparencyHotKey { get; set; }

        public static HotKeys GetDefault()
        {
            var hotKeys = new HotKeys
            {
                BottomLeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad1),
                BottomHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad2),
                BottomRightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad3),
                LeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad4),
                RightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad6),
                TopLeftHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad7),
                TopHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad8),
                TopRightHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad9),
                CenterHotKey = new HotKey(ModifierKeys.Windows, Keys.NumPad5),
                HorizontalCenterHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.NumPad4),
                VerticalCenterHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.NumPad8),
                TopMostHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.NumPad0),
                BottomMostHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.NumPad1),
                ShowSizeSelectionWindowHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.Space),
                IncreaseTransparencyHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.Subtract),
                DecreaseTransparencyHotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, Keys.Add),

                ExtendBottomHotKey = HotKey.None(),
                ExtendLeftHotKey = HotKey.None(),
                ExtendRightHotKey = HotKey.None(),
                ExtendTopHotKey = HotKey.None(),
                FullScreenHotKey = HotKey.None(),
                MinimizeWindowHotKey = HotKey.None(),
                ReduceBottomHotKey = HotKey.None(),
                ReduceLeftHotKey = HotKey.None(),
                ReduceRightHotKey = HotKey.None(),
                ReduceTopHotKey = HotKey.None(),
                SwitchScreenHotKey = HotKey.None(),
            };

            return hotKeys;
        }

        public IEnumerable<HotKey> All()
        {
            return typeof(HotKeys)
                .GetProperties()
                .Where(p => typeof(HotKey).IsAssignableFrom(p.PropertyType))
                .Select(p => (HotKey)p.GetValue(this));
        }
    }
}