using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Reflection;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace InteractiveCLIPS
{
    /// <summary>
    /// Interaction logic for ClipsEditor.xaml
    /// </summary>
    public partial class ClipsEditor : System.Windows.Controls.UserControl
    {
        CompletionWindow completionWindow;
        PortionColorizer pc1, pc2;

        private Boolean AutoScroll = true;

        public Object envLockObj = new Object();

        private iClips clipsEnv;
        public iClips ClipsEnv
        {
            get { return clipsEnv; }
            set { clipsEnv = value; }
        }

        public string Text
        {
            get { return testBoxInput.Text; }
            set
            {
                testBoxInput.Text = value;
            }
        }


        public ClipsEditor(Object lockObj)
        {
            InitializeComponent();

            clipsEnv = null;

            testBoxInput.TextArea.Caret.PositionChanged += caretPositionChanged;
            testBoxInput.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            testBoxInput.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            //using (Stream s = myAssembly.GetManifestResourceStream("CustomHighlighting.xshd"))
            using (Stream s = new MemoryStream(InteractiveCLIPS.Properties.Resources.CLIPSHighlighting))
            //using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CLIPSHighlighting.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    testBoxInput.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }

            envLockObj = lockObj;

            
        }
        
        private PortionColorizer colorizeString(int line, int offset)
        {
            PortionColorizer pc = new PortionColorizer(line, offset, offset + 1);
            testBoxInput.TextArea.TextView.LineTransformers.Add(pc);
            return pc;
        }

        private void matchingBracket(int offset, bool isOpenBracket)
        {
            int offset1 = offset;
            int count = 0;
            if (isOpenBracket)
            {
                count++;
                while (count != 0 && offset1 < (testBoxInput.Text.Length - 1))
                {
                    offset1++;
                    if (testBoxInput.Text.Substring(offset1, 1).Equals(")"))
                        count--;
                    if (testBoxInput.Text.Substring(offset1, 1).Equals("("))
                        count++;
                }
            }
            else
            {
                count--;
                while (count != 0 && offset1 > 0)
                {
                    offset1--;
                    if (testBoxInput.Text.Substring(offset1, 1).Equals(")"))
                        count--;
                    if (testBoxInput.Text.Substring(offset1, 1).Equals("("))
                        count++;
                }
            }
            if (count == 0)
            {
                pc1 = colorizeString(testBoxInput.TextArea.Document.GetLineByOffset(offset).LineNumber, offset); //lo fa due volte???
                pc2 = colorizeString(testBoxInput.TextArea.Document.GetLineByOffset(offset1).LineNumber, offset1);
            }
        }

        //TODO: chiama due volte se inserisci, più evitare ricalcolo se è l'ultima coppia uguale.
        void caretPositionChanged(object sender, EventArgs e)
        {
            testBoxInput.TextArea.TextView.LineTransformers.Remove(pc1);
            testBoxInput.TextArea.TextView.LineTransformers.Remove(pc2);
            // if (testBoxInput.Text.Length == lastLength) può servire a non ricalcolare l'ultima coppia se è la stessa della precedente
            {
                int offset = testBoxInput.TextArea.Caret.Offset;
                //Console.WriteLine("Caret offset: " + offset);
                //se il char precedente è "(" oppure ")" lo prendi e basta --> vince in caso di conflitti di tipo () )( (( ))
                if (((testBoxInput.Text.Length > 0) && (offset > 0)) &&
                    ((testBoxInput.Text.Substring(offset - 1, 1).Equals("(")) || (testBoxInput.Text.Substring(offset - 1, 1).Equals(")"))))
                {
                    //calcolo degli offsets per la colorazione
                    int offset1 = offset - 1;
                    matchingBracket(offset1, testBoxInput.Text.Substring(offset1, 1).Equals("("));
                }
                //altrimenti guarda il char successivo e se è parentesi te lo prendi
                else if (((testBoxInput.Text.Length > 0) && (offset < testBoxInput.Text.Length)) &&
                    ((testBoxInput.Text.Substring(offset, 1).Equals("(")) || (testBoxInput.Text.Substring(offset, 1).Equals(")"))))
                {
                    int offset1 = offset;
                    matchingBracket(offset1, testBoxInput.Text.Substring(offset1, 1).Equals("("));
                }
            }
        }

        internal List<FunctionCompletionData> completionData = new List<FunctionCompletionData>();

        //TODO: fare in modo che le function dichiarate anche da me (sia nel file che con delegate) compaiano come completion
        //Metodi per CodeCompletion
        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "(")
            {
                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(testBoxInput.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var d in completionData) data.Add(d);

                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        //Metodi per CodeCompletion
        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void executeSelection(string selection)
        {
            lock (envLockObj)
            {
                string[]s = clipsEnv.Env.FunctionNames;
                clipsEnv.Env.SetCommandString(selection + "\n");
                    bool b = clipsEnv.Env.ExecuteIfCommandComplete();
                if (b)
                {
                    testBlockOutput.Text = clipsEnv.Router.Output.ToString();
                    scrollBar.ScrollToBottom();
                }
            }
        }

        //TODO: Nel contesto selezione, pensare meglio il lastPosition
        //TODO: Impedire di andare a cambiare linee precedenti già valutate cliccando col mouse?
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (!testBoxInput.SelectedText.Equals(string.Empty))
                {
                    executeSelection(testBoxInput.SelectedText);
                }
            }
        }

        public string openFileDialog()
        {
            FileDialog fileDial = new OpenFileDialog();
            fileDial.DefaultExt = ".clp";
            fileDial.Filter = "Clips Files (.clp)|*.clp|Text Files (.txt)|*.txt";
            fileDial.FilterIndex = 1;

            if (fileDial.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fileDial.FileName;
            }
            return string.Empty;
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            if (!testBoxInput.SelectedText.Equals(""))
            {
                executeSelection(testBoxInput.SelectedText);
            }
            else
            {
                System.Windows.MessageBox.Show("Select code to run!");
            }
        }

        private void scrollBar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (scrollBar.VerticalOffset == scrollBar.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                scrollBar.ScrollToEnd();// ScrollToVerticalOffset(scrollBar.ExtentHeight);
            }

        }

        private void cmbLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lines = cmbLines.SelectedValue.ToString();

            if(clipsEnv!=null)
                clipsEnv.Router.LimitLines = Convert.ToInt32(lines);
        }

        private void testBoxInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("CLICK");
        }


       
       

    }
}