namespace Editor
{
    public static class StatusBarHelper
    {
        static IMainWindow window = null;
        public static void Init(IMainWindow _window)
        {
            window = _window;
        }

        public static void PrintStatusMessage(string message)
        {
            window.SetStatusBarMessage(message);
        }

        public static void ShowCoordinates(string message)
        {
            window.ShowCoordinates(message);
        }
    }
}