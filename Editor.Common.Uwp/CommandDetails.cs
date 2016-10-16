﻿using Windows.UI.Xaml.Controls;

namespace Editor
{
    public sealed class CommandDetails
    {
        public Image Image { get; set; }
        public string UnicodeString { get; set; }
        public CommandType CommandType { get; set; }
        public object CommandParam { get; set; }
    }
}
