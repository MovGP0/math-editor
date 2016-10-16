namespace Editor
{
    public static class StatusBarHelper
    {
        private static IMainWindow _window;
        public static void Init(IMainWindow window)
        {
            _window = window;
        }

        public static void PrintStatusMessage(string message)
        {
            _window.SetStatusBarMessage(message);
        }

        public static void ShowCoordinates(string message)
        {
            _window.ShowCoordinates(message);
        }
    }
}