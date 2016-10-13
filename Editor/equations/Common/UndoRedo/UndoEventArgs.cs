using System;

namespace Editor
{   
    public class UndoEventArgs : EventArgs
    {
        public bool ActionPossible { get; set; }
        public UndoEventArgs(bool actionPossible)
        {
            ActionPossible = actionPossible;
        }
    }
}

