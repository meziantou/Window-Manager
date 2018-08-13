using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WindowManager.Core;

namespace WindowManager
{
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "WindowManager";
        public const string MutexName = "{ff1b2b33-9a71-4e8d-9ace-dec72894c4d7}";
        private Mutex _instanceMutex;

        public static void Restart()
        {
            ProcessStartInfo currentStartInfo = Process.GetCurrentProcess().StartInfo;

            if (ApplicationDeployment.IsNetworkDeployed)
                currentStartInfo.FileName =
                    ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;
            else
                currentStartInfo.FileName = System.Windows.Forms.Application.ExecutablePath;

            currentStartInfo.Arguments = "/Updated";
            Process.Start(currentStartInfo);
            Environment.Exit(0);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _instanceMutex = new Mutex(true, MutexName);

            if (CommandLine.Default.Updated)
            {
                if (!_instanceMutex.WaitOne(60000)) // 60s to close the previous instance
                {
                    MessageBox.Show("WindowManager was unabled to restart", "WindowManager", MessageBoxButton.OK);
                    Environment.Exit(0);
                }
            }
            else
            {
                if (!_instanceMutex.WaitOne(0))
                {
                    // Another instance is already started.
                    // Send a message to show the main window
                    try
                    {
                        var channel = new IpcChannel("Client");
                        ChannelServices.RegisterChannel(channel, false);
                        var app =
                            (SingleInstance)
                            Activator.GetObject(typeof(SingleInstance), string.Format("ipc://{0}/RemotingServer", Name));

                        if (e.Args.Length == 0)
                            app.Execute(null);
                        else
                            app.Execute(e.Args);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }

                    Environment.Exit(0);
                }
            }

            try
            {
                RegisterIpcServer();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
            base.OnExit(e);
        }

        private static void RegisterIpcServer()
        {
            var channel = new IpcChannel(Name);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(SingleInstance), "RemotingServer",
                                                               WellKnownObjectMode.Singleton);
        }

        #region Nested type: SingleInstance

        private class SingleInstance : MarshalByRefObject
        {
            public void Execute(string[] args)
            {
                Current.Dispatcher.BeginInvoke((Action)(SetVisibility), DispatcherPriority.Render);
            }

            private void SetVisibility()
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion
    }
}