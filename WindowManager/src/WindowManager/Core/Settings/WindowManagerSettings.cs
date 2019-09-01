using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Meziantou.Framework;
using Microsoft.Win32;

namespace WindowManager.Core.Settings
{
    public sealed class WindowManagerSettings : INotifyPropertyChanged
    {
        private const string Name = "WindowManager";

        private HotKeys _hotKeys = HotKeys.GetDefault();
        private Theme _theme = Theme.GetDefault();

        public bool StartWithWindows { get; set; }
        public bool FirstStart { get; set; }
        public bool MinimizeToTrayBar { get; set; }
        public bool FullScreenMaximized { get; set; }
        public bool OverrideExistingHotKeys { get; set; }

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

        private static string GetSettingsFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Meziantou", Name, "settings.xml");
        }

        public void Save()
        {
            var path = GetSettingsFile();
            IOUtilities.PathCreateDirectory(path);

            using (Stream stream = File.Open(path, FileMode.Create))
            {
                SaveXml(this, stream);
            }

            SetStartWithWindows(StartWithWindows);
        }

        public static WindowManagerSettings Load()
        {
            var path = GetSettingsFile();
            WindowManagerSettings settings;
            using (Stream stream = File.OpenRead(path))
            {
                settings = LoadXml<WindowManagerSettings>(stream);
            }

            SetStartWithWindows(settings.StartWithWindows);
            return settings;
        }

        public static WindowManagerSettings GetDefault()
        {
            var settings = new WindowManagerSettings
            {
                FirstStart = true,
                FullScreenMaximized = true,
                OverrideExistingHotKeys = true,
                MinimizeToTrayBar = true,
                StartWithWindows = true,
                HotKeys = HotKeys.GetDefault(),
                Theme = Theme.GetDefault(),
            };

            return settings;
        }

        private static T LoadXml<T>(Stream stream)
        {
            Contract.Requires(stream != null);

            var xs = new XmlSerializer(typeof(T));

            var temp = (T)xs.Deserialize(stream);
            return temp;
        }

        private static void SaveXml<T>(T obj, Stream stream)
        {
            Contract.Requires(stream != null);

            var xs = new XmlSerializer(typeof(T));
            xs.Serialize(stream, obj);
        }

        private static void SetStartWithWindows(bool startWithWindows)
        {
            RegistryKey rkApp = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
            if (startWithWindows)
            {
                rkApp.SetValue(Name, Assembly.GetEntryAssembly().Location);
            }
            else
            {
                if (rkApp.GetValue(Name) != null)
                {
                    rkApp.DeleteValue(Name);
                }
            }
        }

        public WindowManagerSettings Clone()
        {
            using var ms = new MemoryStream();
            SaveXml(this, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return LoadXml<WindowManagerSettings>(ms);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}