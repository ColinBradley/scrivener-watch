using ScrivenerWatch.App.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace ScrivenerWatch.App.Commands
{
    internal class MergeFilesCommand : ICommand
    {
        private readonly ApplicationModel mApplicationModel;

        public MergeFilesCommand(ApplicationModel applicationModel)
        {
            mApplicationModel = applicationModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? _parameter)
            => true;

        public void Execute(object? parameter)
        {
            var files = mApplicationModel.LatestDifference?.AllDifferences.Where(d => d.IsSelected).ToArray();
            if (files == null || files.Length == 0)
            {
                return;
            }

            object oMissing = System.Reflection.Missing.Value;
            object oFalse = false;
            var wordApp = new Microsoft.Office.Interop.Word.Application();
            var document = wordApp.Documents.Add();

            foreach (var file in files.Select(d => d.NewFile!))
            {
                var headerParagraph = document.Paragraphs.Add();
                headerParagraph.Range.Text = file.Title;
                headerParagraph.OutlineLevel = Microsoft.Office.Interop.Word.WdOutlineLevel.wdOutlineLevel1;
                headerParagraph.set_Style(Microsoft.Office.Interop.Word.WdBuiltinStyle.wdStyleHeading1);
                headerParagraph.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                _ = document.Paragraphs.Add();

                var path = mApplicationModel.GetFilePath(file.Id);
                _ = document.Paragraphs.Add();
                document.Range(document.Content.End - 1).InsertFile(path);
                document.Range(document.Content.End - 1).InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
            }

            wordApp.Visible = true;
        }
    }
}