using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace WindowManager.Core.Settings
{
    public class Settings : ICloneable, INotifyPropertyChanged
    {
        private const string Name = "WindowManager";
        private const string Extension = ".xml";
        public string CurrentHotKeys { get; set; }
        public string CurrentTheme { get; set; }

        private Theme _theme;

        [XmlIgnore]
        public Theme Theme
        {
            get { return _theme; }
            set
            {
                if (value == _theme)
                    return;

                _theme = value;
                OnPropertyChanged("Theme");
            }
        }

        private HotKeys _hotKeys;

        [XmlIgnore]
        public HotKeys HotKeys
        {
            get { return _hotKeys; }
            set
            {
                if (value == _hotKeys)
                    return;
                _hotKeys = value;
                OnPropertyChanged("HotKeys");
            }
        }

        public bool StartWithWindows { get; set; }
        public bool FirstStart { get; set; }
        public bool MinimizeToTrayBar { get; set; }
        public bool FullScreenMaximized { get; set; }
        public bool OverrideExistingHotKeys { get; set; }
        public bool SendAnonymData { get; set; }

        public static IEnumerable<string> GetThemeNames()
        {
            return Directory.GetFiles(GetThemeFolder(), "*" + Extension).Select(Path.GetFileNameWithoutExtension).ToList();
        }

        public static IEnumerable<string> GetHotKeysNames()
        {
            return Directory.GetFiles(GetHotKeysFolder(), "*" + Extension).Select(Path.GetFileNameWithoutExtension).ToList();
        }

        private static string GetSettingFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Meziantou", Name);
        }

        private static string GetSettingsFile()
        {
            return Path.Combine(GetSettingFolder(), "settings.xml");
        }

        private static string GetThemeFolder()
        {
            return Path.Combine(GetSettingFolder(), "Themes");
        }

        private static string GetHotKeysFolder()
        {
            return Path.Combine(GetSettingFolder(), "HotKeys");
        }

        public void SetTheme(string name)
        {
            Theme = LoadTheme(name);
            CurrentTheme = name;
        }

        public void SetHotKeys(string name)
        {
            HotKeys = LoadHotKeys(name);
            CurrentHotKeys = name;
        }

        public void CreateTheme(string name)
        {
            Theme = Theme.GetDefault();
            Theme.Name = name;
            CurrentTheme = name;
        }

        public void DeleteTheme(string name)
        {
            string pathTheme = Path.Combine(GetThemeFolder(), name + Extension);
            File.Delete(pathTheme);
            string newName = GetThemeNames().FirstOrDefault();
            if (newName != null)
            {
                SetTheme(newName);
            }
        }


        public void DeleteHotKeys(string name)
        {
            string pathHotKeys = Path.Combine(GetHotKeysFolder(), name + Extension);
            File.Delete(pathHotKeys);
            string newName = GetHotKeysNames().FirstOrDefault();
            if (newName != null)
            {
                SetHotKeys(newName);
            }
        }

        public void CreateHotKeys(string name)
        {
            HotKeys = HotKeys.GetDefault();
            HotKeys.Name = name;
            CurrentHotKeys = name;
        }

        public void Save()
        {
            string path = GetSettingsFile();
            string pathTheme = Path.Combine(GetThemeFolder(), CurrentTheme + Extension);
            string pathHotKeys = Path.Combine(GetHotKeysFolder(), CurrentHotKeys + Extension);

            Directory.CreateDirectory(GetSettingFolder());
            Directory.CreateDirectory(GetThemeFolder());
            Directory.CreateDirectory(GetHotKeysFolder());

            using (Stream stream = File.Open(path, FileMode.Create))
            {
                SaveXml(this, stream);
            }
            using (Stream stream = File.Open(pathTheme, FileMode.Create))
            {
                SaveXml(Theme, stream);
            }
            using (Stream stream = File.Open(pathHotKeys, FileMode.Create))
            {
                SaveXml(HotKeys, stream);
            }

            SetStartWithWindows(this.StartWithWindows);
        }

        public void Export(string path)
        {
            throw new NotImplementedException();
        }

        public static Settings Load()
        {
            string path = GetSettingsFile();
            Settings settings;
            using (Stream stream = File.OpenRead(path))
            {
                settings = LoadXml<Settings>(stream);
            }

            settings.SetHotKeys(settings.CurrentHotKeys);
            settings.SetTheme(settings.CurrentTheme);

            SetStartWithWindows(settings.StartWithWindows);
            return settings;
        }

        public static Settings Import(string path)
        {
            throw new NotImplementedException();
        }

        public static Settings GetDefault()
        {
            Settings settings = new Settings();
            settings.CurrentHotKeys = "Default";
            settings.CurrentTheme = "Default";
            settings.FirstStart = true;
            settings.FullScreenMaximized = true;
            settings.OverrideExistingHotKeys = true;
            settings.SendAnonymData = true;
            settings.HotKeys = HotKeys.GetDefault();
            settings.MinimizeToTrayBar = true;
            settings.StartWithWindows = true;
            settings.Theme = Theme.GetDefault();

            return settings;
        }

        /// <summary>
        ///   Loads an object from an xml file.
        /// </summary>
        /// <typeparam name="T"> Type of the object to load. </typeparam>
        /// <param name="stream"> The input xml file. </param>
        /// <returns> The loaded object. </returns>
        private static T LoadXml<T>(Stream stream)
        {
            Contract.Requires(stream != null);

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));

                T temp = (T)xs.Deserialize(stream);
                return temp;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        ///   Saves an object to an xml file. Close the stream at the end.
        /// </summary>
        /// <typeparam name="T"> Type of the object to save </typeparam>
        /// <param name="obj"> The object to save. </param>
        /// <param name="stream"> The xml output file. </param>
        /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the System.Exception.InnerException property.</exception>
        private static void SaveXml<T>(T obj, Stream stream)
        {
            Contract.Requires(stream != null);

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(stream, obj);
            }
            finally
            {
                stream.Close();
            }
        }

        private static void SetStartWithWindows(bool startWithWindows)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startWithWindows)
                rkApp.SetValue(Name, Assembly.GetEntryAssembly().Location);
            else
            {
                if (rkApp.GetValue(Name) != null)
                    rkApp.DeleteValue(Name);
            }
        }

        private static Theme LoadTheme(string name)
        {
            string path = Path.Combine(GetThemeFolder(), name + Extension);
            using (Stream stream = File.OpenRead(path))
            {
                Theme theme = LoadXml<Theme>(stream);
                theme.Name = name;
                return theme;
            }
        }

        private static HotKeys LoadHotKeys(string name)
        {
            string path = Path.Combine(GetHotKeysFolder(), name + Extension);
            using (Stream stream = File.OpenRead(path))
            {
                HotKeys hotKeys = LoadXml<HotKeys>(stream);
                hotKeys.Name = name;
                return hotKeys;
            }
        }

        private static void SaveTheme(Theme theme)
        {
            string path = Path.Combine(GetThemeFolder(), theme.Name + Extension);
            Directory.CreateDirectory(path);
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                SaveXml(theme, stream);
            }
        }

        private static void SaveHotKeys(HotKeys hotKeys)
        {
            string path = Path.Combine(GetHotKeysFolder(), hotKeys.Name + Extension);
            Directory.CreateDirectory(path);
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                SaveXml(hotKeys, stream);
            }
        }

        public Settings Clone()
        {
            Settings settings = new Settings();
            settings.HotKeys = new HotKeys();
            settings.Theme = new Theme();

            CopyTo(settings);

            return settings;
        }
        private void CopyTo(Settings settings)
        {
            settings.StartWithWindows = StartWithWindows;
            settings.OverrideExistingHotKeys = OverrideExistingHotKeys;
            settings.FirstStart = FirstStart;
            settings.MinimizeToTrayBar = MinimizeToTrayBar;
            settings.FullScreenMaximized = FullScreenMaximized;
            settings.SendAnonymData = SendAnonymData;

            settings.HotKeys.CenterHotKey = SafeClone(HotKeys.CenterHotKey);
            settings.HotKeys.HorizontalCenterHotKey = SafeClone(HotKeys.HorizontalCenterHotKey);
            settings.HotKeys.VerticalCenterHotKey = SafeClone(HotKeys.VerticalCenterHotKey);
            settings.HotKeys.BottomMostHotKey = SafeClone(HotKeys.BottomMostHotKey);
            settings.HotKeys.TopMostHotKey = SafeClone(HotKeys.TopMostHotKey);
            settings.HotKeys.ShowSizeSelectionWindowHotKey = SafeClone(HotKeys.ShowSizeSelectionWindowHotKey);
            settings.HotKeys.SwitchScreenHotKey = SafeClone(HotKeys.SwitchScreenHotKey);
            settings.HotKeys.TopLeftHotKey = SafeClone(HotKeys.TopLeftHotKey);
            settings.HotKeys.TopHotKey = SafeClone(HotKeys.TopHotKey);
            settings.HotKeys.TopRightHotKey = SafeClone(HotKeys.TopRightHotKey);
            settings.HotKeys.LeftHotKey = SafeClone(HotKeys.LeftHotKey);
            settings.HotKeys.RightHotKey = SafeClone(HotKeys.RightHotKey);
            settings.HotKeys.BottomLeftHotKey = SafeClone(HotKeys.BottomLeftHotKey);
            settings.HotKeys.BottomRightHotKey = SafeClone(HotKeys.BottomRightHotKey);
            settings.HotKeys.FullScreenHotKey = SafeClone(HotKeys.FullScreenHotKey);
            settings.HotKeys.BottomHotKey = SafeClone(HotKeys.BottomHotKey);
            settings.HotKeys.MinimizeWindowHotKey = SafeClone(HotKeys.MinimizeWindowHotKey);
            settings.HotKeys.ExtendBottomHotKey = SafeClone(HotKeys.ExtendBottomHotKey);
            settings.HotKeys.ExtendTopHotKey = SafeClone(HotKeys.ExtendTopHotKey);
            settings.HotKeys.ExtendLeftHotKey = SafeClone(HotKeys.ExtendLeftHotKey);
            settings.HotKeys.ExtendRightHotKey = SafeClone(HotKeys.ExtendRightHotKey);
            settings.HotKeys.ReduceBottomHotKey = SafeClone(HotKeys.ReduceBottomHotKey);
            settings.HotKeys.ReduceTopHotKey = SafeClone(HotKeys.ReduceTopHotKey);
            settings.HotKeys.ReduceLeftHotKey = SafeClone(HotKeys.ReduceLeftHotKey);
            settings.HotKeys.ReduceRightHotKey = SafeClone(HotKeys.ReduceRightHotKey);

            settings.Theme.NbItemsPerRow = Theme.NbItemsPerRow;
            settings.Theme.NbItemsPerColumn = Theme.NbItemsPerColumn;
            settings.Theme.SwitchWindowFullScreen = Theme.SwitchWindowFullScreen;
            settings.Theme.BackgroundColor = Theme.BackgroundColor;
            settings.Theme.ForegroundColor = Theme.ForegroundColor;
            settings.Theme.SelectionFillColor = Theme.SelectionFillColor;
            settings.Theme.SelectionBorderColor = Theme.SelectionBorderColor;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        private static TIn SafeClone<TIn>(TIn obj)
            where TIn : class, ICloneable
        {
            if (obj == null)
                return null;

            var clone = (TIn)obj.Clone();
            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}