using ScrivenerWatch.App.Model;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace ScrivenerWatch.App.Commands
{
    internal class BrowseForScrivenerLocationCommand : ICommand
    {
        private readonly ApplicationModel mApplicationModel;

        public BrowseForScrivenerLocationCommand(ApplicationModel applicationModel)
        {
            mApplicationModel = applicationModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => true;

        public void Execute(object? parameter)
        {
            using var dialog = new OpenFileDialog()
            {
                Filter = "Scrivener File(*.scrivx)|*.scrivx",
                FileName = mApplicationModel.FilePath,
            };

            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            mApplicationModel.FilePath = dialog.FileName;
        }
    }
}
