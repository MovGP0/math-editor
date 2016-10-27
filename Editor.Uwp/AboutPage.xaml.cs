using System.Reflection;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Editor
{
    public partial class AboutWindow
    {
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string Version
        {
            get { return (string) GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public AboutWindow()
        {
            InitializeComponent();
            Version = GetVersionString();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private static string GetVersionString()
        {
            var versionAsString = typeof(AboutWindow).GetTypeInfo().Assembly.GetName().Version.ToString(4);
            return $"Math Editor v.{versionAsString}";
        }
    }
}
