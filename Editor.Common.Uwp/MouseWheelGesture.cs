using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Editor
{
    // https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.uielement.pointerwheelchanged
    public sealed class MouseWheelGesture
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public MouseWheelGesture(UIElement element)
        {
            element.PointerWheelChanged += ElementOnPointerWheelChanged;
        }
        
        private void ElementOnPointerWheelChanged(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            var modifiers = pointerRoutedEventArgs.KeyModifiers;

            var element = sender as UIElement;
            var point = pointerRoutedEventArgs.GetCurrentPoint(element);
            var delta = point.Properties.MouseWheelDelta;
        }
    }
}
