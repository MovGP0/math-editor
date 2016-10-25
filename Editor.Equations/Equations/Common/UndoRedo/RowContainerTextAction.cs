﻿using System.Collections.Generic;

namespace Editor
{   
    public class RowContainerTextAction : EquationAction
    {        
        public int SelectionStartIndex { get; set; }
        public int SelectedItems { get; set; }

        public IEquationBase ActiveEquation { get; set; }
        public IEquationBase ActiveEquationAfterChange { get; set; }
        public int ActiveEquationSelectionIndex { get; set; }
        public int ActiveEquationSelectedItems { get; set; }
        
        public TextEquation ActiveTextInRow { get; set; }
        public int CaretIndexOfActiveText { get; set; }
        public int SelectionStartIndexOfTextEquation { get; set; }
        public int SelectedItemsOfTextEquation { get; set; }
        public string TextEquationContents { get; set; }
        public int[] TextEquationFormats { get; set; }
        public EditorMode[] TextEquationModes { get; set; }
        public CharacterDecorationInfo[] TextEquationDecoration { get; set; }

        public string FirstLineOfInsertedText { get; set; }
        public int[] FirstFormatsOfInsertedText { get; set; }
        public EditorMode[] FirstModesOfInsertedText { get; set; }
        public CharacterDecorationInfo[] FirstDecorationsOfInsertedText { get; set; }

        public List<IEquationRow> Equations { get; set; }

        public RowContainerTextAction(ISupportsUndo executor)
            : base(executor)
        {   
        }
    }    
}

