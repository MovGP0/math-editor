using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Editor
{
    [TemplatePart(Name=ContentHostName, Type=typeof(Canvas))]
    public sealed partial class EquationContainer
    {
        private const string ContentHostName = "PART_ContentHost";
        private Canvas ContentHost { get; set; }

        public EquationContainer()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate()
        {
            ContentHost = (Canvas)GetTemplateChild(ContentHostName);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}
