using System;
using System.Collections.Generic;

namespace Editor
{   
    public static class UndoManager
    {
        public static bool DisableAddingActions { get; set; }
        private static Stack<EquationAction> UndoStack { get; } = new Stack<EquationAction>();
        private static Stack<EquationAction> RedoStack { get; } = new Stack<EquationAction>();

        public static event EventHandler<UndoEventArgs> CanUndo = (a, b) => { };
        public static event EventHandler<UndoEventArgs> CanRedo = (a, b) => { };

        public static void AddUndoAction(EquationAction equationAction)
        {
            if (DisableAddingActions) return;

            UndoStack.Push(equationAction);
            RedoStack.Clear();
            CanUndo(null, new UndoEventArgs(true));
            CanRedo(null, new UndoEventArgs(false));
        }
        
        public static void Undo()
        {
            if (UndoStack.Count <= 0) return;

            var temp = UndoStack.Peek();
            for (var i = 0; i <= temp.FurtherUndoCount; i++)
            {
                var action = UndoStack.Pop();
                action.Executor.ProcessUndo(action);
                action.UndoFlag = !action.UndoFlag;
                RedoStack.Push(action);
            }

            if (UndoStack.Count == 0)
            {
                CanUndo(null, new UndoEventArgs(false));
            }
            CanRedo(null, new UndoEventArgs(true));
        }

        public static void Redo()
        {
            if (RedoStack.Count <= 0) return;

            var temp = RedoStack.Peek();
            for (var i = 0; i <= temp.FurtherUndoCount; i++)
            {
                var action = RedoStack.Pop();
                action.Executor.ProcessUndo(action);
                action.UndoFlag = !action.UndoFlag;
                UndoStack.Push(action);
            }

            if (RedoStack.Count == 0)
            {
                CanRedo(null, new UndoEventArgs(false));
            }
            CanUndo(null, new UndoEventArgs(true));
        }

        public static void ClearAll()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            CanUndo(null, new UndoEventArgs(false));
            CanRedo(null, new UndoEventArgs(false));
        }

        public static int UndoCount => UndoStack.Count;

        public static void ChangeUndoCountOfLastAction(int newCount)
        {
            UndoStack.Peek().FurtherUndoCount = newCount;
            for (var i = 0; i < newCount; i++)
            {
                RedoStack.Push(UndoStack.Pop());
            }

            UndoStack.Peek().FurtherUndoCount = newCount;
            for (var i = 0; i < newCount; i++)
            {
                UndoStack.Push(RedoStack.Pop());
            }
        }
    }
}

