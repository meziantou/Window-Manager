using System.Diagnostics.Contracts;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager.Core
{
    public static class HotKeyFluent
    {
        public static HotKey Register(this HotKey hotKey)
        {
            Contract.Ensures(Contract.Result<HotKey>() == hotKey);

            if (hotKey == null)
                return null;

            hotKey.RegisterHotKey();
            return hotKey;
        }

        public static HotKey Unregister(this HotKey hotKey)
        {
            Contract.Ensures(Contract.Result<HotKey>() == hotKey);

            if (hotKey == null)
                return null;

            hotKey.UnregisterHotKey();
            return hotKey;
        }

        public static HotKey Reassign(this HotKey hotKey, ModifierKeys modifier, Keys key)
        {
            Contract.Ensures(Contract.Result<HotKey>() == hotKey);

            if (hotKey == null)
                return null;

            hotKey.ReassignHotKey(modifier, key);
            return hotKey;
        }

        public static HotKey AddHandler(this HotKey hotKey, HotKeyPressedEventHandler handler)
        {
            Contract.Ensures(Contract.Result<HotKey>() == hotKey);

            if (hotKey == null)
                return null;

            hotKey.HotKeyPressed += handler;
            return hotKey;
        }

        public static HotKey RemoveHandler(this HotKey hotKey, HotKeyPressedEventHandler handler)
        {
            Contract.Ensures(Contract.Result<HotKey>() == hotKey);

            if (hotKey == null)
                return null;

            hotKey.HotKeyPressed -= handler;
            return hotKey;
        }
    }
}