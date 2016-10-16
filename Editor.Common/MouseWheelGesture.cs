using System.Windows.Input;

namespace Editor
{
    public sealed class MouseWheelGesture : MouseGesture
    {
        public WheelDirection Direction { get; set; }

        public static MouseWheelGesture CtrlDown => new MouseWheelGesture(ModifierKeys.Control)
        {
            Direction = WheelDirection.Down
        };

        public static MouseWheelGesture CtrlUp => new MouseWheelGesture(ModifierKeys.Control)
        {
            Direction = WheelDirection.Up
        };

        public MouseWheelGesture()
            : base(MouseAction.WheelClick)
        {
        }

        public MouseWheelGesture(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        {
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs)) return false;

            var mouseWheelEventArgs = inputEventArgs as MouseWheelEventArgs;
            if (mouseWheelEventArgs == null) return false;

            switch (Direction)
            {
                case WheelDirection.None:
                    return mouseWheelEventArgs.Delta == 0;
                case WheelDirection.Up:
                    return mouseWheelEventArgs.Delta > 0;
                case WheelDirection.Down:
                    return mouseWheelEventArgs.Delta < 0;
                default:
                    return false;
            }
        }
    }
}
