using System.Collections.Generic;

namespace Editor
{
    public interface IEquationRow : IEquationContainer, ISupportsUndo
    {
        void Truncate();
        void Truncate(int indexFrom, int keepCount);
        void SetCurrentChild(int childIndex, int caretIndex);
        List<IEquationBase> ChildEquations { get; }
        void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems, List<IEquationBase> items, bool appendAtEnd);
        void ResetRowEquation(IEquationBase activeChild, int selectionStartIndex, int selectedItems);
        void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems);
        void Merge(IEquationRow secondLine);
        void RemoveChild(IEquationBase child);
        void AddChild(IEquationBase newChild);
        TextEquation GetLastTextEquation();
        TextEquation GetFirstTextEquation();
        TextEquation GetFirstSelectionText();
        TextEquation GetLastSelectionText();
        List<IEquationBase> GetSelectedEquations();
        int ActiveChildIndex { get; set; }
        List<IEquationBase> DeleteTail();
        List<IEquationBase> DeleteHead();
        bool IsEmpty { get; }
        void MoveToEnd();
        void MoveToStart();
        void SelectToStart();
        void SelectToEnd();
        int TextLength { get; }

    }
}