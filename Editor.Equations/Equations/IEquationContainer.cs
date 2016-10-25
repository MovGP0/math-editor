using System.Windows;

namespace Editor
{
    public interface IEquationContainer : IEquationBase, IColor
    {
        int GetIndex(IEquationBase child);
        void ChildCompletedUndo(IEquationBase child);
        int SubLevel { get; set; }
        IEquationBase ActiveChild { get; set; }
        IEquationContainer GetInnerMostEquationContainer();
        void ExecuteCommand(CommandType commandType, object data);
        Point GetHorizontalCaretLocation();
        double GetHorizontalCaretLength();
    }
}