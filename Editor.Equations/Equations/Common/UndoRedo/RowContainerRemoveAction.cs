using System.Collections.Generic;

namespace Editor
{   
    public class RowContainerRemoveAction : RowRemoveAction
    {
        public IEquationRow HeadEquationRow { get; set; }
        public IEquationRow TailEquationRow { get; set; }
        public int FirstRowActiveIndex { get; set; }
        public int LastRowActiveIndex { get; set; }
        public int FirstRowSelectionIndex { get; set; }
        public int LastRowSelectionIndex { get; set; }
        public int FirstRowSelectedItems { get; set; }
        public int LastRowSelectedItems { get; set; }
        public int FirstRowActiveIndexAfterRemoval { get; set; }
        public List<IEquationBase> FirstRowDeletedContent { get; set; }
        public List<IEquationBase> LastRowDeletedContent { get; set; }

        public RowContainerRemoveAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }    
}

