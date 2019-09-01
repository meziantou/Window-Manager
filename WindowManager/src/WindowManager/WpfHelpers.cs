using System.Windows.Threading;

namespace WindowManager
{
    internal static class WpfHelpers
    {
        private static readonly DispatcherOperationCallback ExitFrameCallback = ExitFrame;

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            DispatcherOperation dispatcherOperation =
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, ExitFrameCallback, frame);
            Dispatcher.PushFrame(frame);
            if (dispatcherOperation.Status == DispatcherOperationStatus.Completed)
                return;
            dispatcherOperation.Abort();
        }

        private static object ExitFrame(object state)
        {
            ((DispatcherFrame)state).Continue = false;
            return null;
        }
    }
}
