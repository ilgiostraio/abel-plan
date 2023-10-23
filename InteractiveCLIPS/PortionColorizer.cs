using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace InteractiveCLIPS
{
    public class PortionColorizer : DocumentColorizingTransformer
    {
        int lineNumber;
        int offset;
        int length;

        public PortionColorizer(int lineNumber, int offset, int length)
        {
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, "Line numbers are 1-based.");
            this.lineNumber = lineNumber;
            this.offset = offset;
            this.length = length;
        }
    
        public int LineNumber {
            get { return lineNumber; }
            set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", value, "Line numbers are 1-based.");
                lineNumber = value;
            }
        }
    
        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber) {
                ChangeLinePart(offset, length, ApplyChanges);
            }
        }
    
        void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetForegroundBrush(Brushes.Red);
        }

    }
}