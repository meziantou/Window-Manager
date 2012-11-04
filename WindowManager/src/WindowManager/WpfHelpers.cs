using System.Windows.Threading;

namespace WindowManager
{
    public static class WpfHelpers
    {
        private static readonly DispatcherOperationCallback ExitFrameCallback = ExitFrame;

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
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
