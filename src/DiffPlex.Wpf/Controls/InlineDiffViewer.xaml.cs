﻿using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls
{
    /// <summary>
    /// The inline diff control for text.
    /// Interaction logic for InlineDiffViewer.xaml
    /// </summary>
    public partial class InlineDiffViewer : UserControl
    {
        /// <summary>
        /// The property of diff model.
        /// </summary>
        public static readonly DependencyProperty DiffModelProperty =
             DependencyProperty.Register("DiffModel", typeof(DiffPaneModel),
             typeof(InlineDiffViewer), new FrameworkPropertyMetadata(null, (d, e) =>
             {
                 if (!(d is InlineDiffViewer c) || e.OldValue == e.NewValue) return;
                 if (e.NewValue == null)
                 {
                     c.UpdateContent(null);
                     return;
                 }

                 if (!(e.NewValue is DiffPaneModel model)) return;
                 c.UpdateContent(model);
             }));

        /// <summary>
        /// The property of line number background brush.
        /// </summary>
        public static readonly DependencyProperty LineNumberForegroundProperty = RegisterDependencyProperty<Brush>("LineNumberForeground", new SolidColorBrush(Color.FromArgb(255, 64, 128, 160)));

        /// <summary>
        /// The property of line number.
        /// </summary>
        public static readonly DependencyProperty LineNumberWidthProperty = RegisterDependencyProperty<double>("LineNumberWidth", 60, (d, e) =>
        {
            if (!(d is InlineDiffViewer c) || e.OldValue == e.NewValue || !(e.NewValue is int n)) return;
            c.ContentPanel.LineNumberWidth = n;
        });

        /// <summary>
        /// The property of change type symbol foreground brush.
        /// </summary>
        public static readonly DependencyProperty ChangeTypeForegroundProperty = RegisterDependencyProperty<Brush>("ChangeTypeForeground", new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty InsertedForegroundProperty = RegisterDependencyProperty<Brush>("InsertedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty InsertedBackgroundProperty = RegisterDependencyProperty<Brush>("InsertedBackground", new SolidColorBrush(Color.FromArgb(64, 96, 216, 32)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty DeletedForegroundProperty = RegisterDependencyProperty<Brush>("DeletedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty DeletedBackgroundProperty = RegisterDependencyProperty<Brush>("DeletedBackground", new SolidColorBrush(Color.FromArgb(64, 216, 32, 32)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty UnchangedForegroundProperty = RegisterDependencyProperty<Brush>("UnchangedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty UnchangedBackgroundProperty = RegisterDependencyProperty<Brush>("UnchangedBackground");

        /// <summary>
        /// The property of grid splitter background brush.
        /// </summary>
        public static readonly DependencyProperty SplitterForegroundProperty = RegisterDependencyProperty<Brush>("SplitterForeground");

        /// <summary>
        /// The property of grid splitter background brush.
        /// </summary>
        public static readonly DependencyProperty SplitterBackgroundProperty = RegisterDependencyProperty<Brush>("SplitterBackground", new SolidColorBrush(Color.FromArgb(64, 128, 128, 128)));

        /// <summary>
        /// The property of grid splitter border brush.
        /// </summary>
        public static readonly DependencyProperty SplitterBorderBrushProperty = RegisterDependencyProperty<Brush>("SplitterBorderBrush");

        /// <summary>
        /// The property of grid splitter border thickness.
        /// </summary>
        public static readonly DependencyProperty SplitterBorderThicknessProperty = RegisterDependencyProperty<Thickness>("SplitterBorderThickness");

        /// <summary>
        /// The property of grid splitter width.
        /// </summary>
        public static readonly DependencyProperty SplitterWidthProperty = RegisterDependencyProperty<double>("SplitterWidth", 5);

        /// <summary>
        /// Initializes a new instance of the InlineDiffViewer class.
        /// </summary>
        public InlineDiffViewer()
        {
            this.InitializeComponent();

            ContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
        }

        /// <summary>
        /// Gets or sets the side by side diff model.
        /// </summary>
        [Category("Appearance")]
        public DiffPaneModel DiffModel
        {
            get => (DiffPaneModel)this.GetValue(DiffModelProperty);
            set => this.SetValue(DiffModelProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line number.
        /// </summary>
        [Bindable(true)]
        public Brush LineNumberForeground
        {
            get => (Brush)this.GetValue(LineNumberForegroundProperty);
            set => this.SetValue(LineNumberForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the line number width.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public int LineNumberWidth
        {
            get => (int)this.GetValue(LineNumberWidthProperty);
            set => this.SetValue(LineNumberWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the change type symbol.
        /// </summary>
        [Bindable(true)]
        public Brush ChangeTypeForeground
        {
            get => (Brush)this.GetValue(ChangeTypeForegroundProperty);
            set => this.SetValue(ChangeTypeForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush InsertedForeground
        {
            get => (Brush)this.GetValue(InsertedForegroundProperty);
            set => this.SetValue(InsertedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush InsertedBackground
        {
            get => (Brush)this.GetValue(InsertedBackgroundProperty);
            set => this.SetValue(InsertedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line deleted.
        /// </summary>
        [Bindable(true)]
        public Brush DeletedForeground
        {
            get => (Brush)this.GetValue(DeletedForegroundProperty);
            set => this.SetValue(DeletedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line deleted.
        /// </summary>
        [Bindable(true)]
        public Brush DeletedBackground
        {
            get => (Brush)this.GetValue(DeletedBackgroundProperty);
            set => this.SetValue(DeletedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line unchanged.
        /// </summary>
        [Bindable(true)]
        public Brush UnchangedForeground
        {
            get => (Brush)this.GetValue(UnchangedForegroundProperty);
            set => this.SetValue(UnchangedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line unchanged.
        /// </summary>
        [Bindable(true)]
        public Brush UnchangedBackground
        {
            get => (Brush)this.GetValue(UnchangedBackgroundProperty);
            set => this.SetValue(UnchangedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the grid splitter.
        /// </summary>
        [Bindable(true)]
        public Brush SplitterForeground
        {
            get => (Brush)this.GetValue(SplitterForegroundProperty);
            set => this.SetValue(SplitterForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the grid splitter.
        /// </summary>
        [Bindable(true)]
        public Brush SplitterBackground
        {
            get => (Brush)this.GetValue(SplitterBackgroundProperty);
            set => this.SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border brush of the grid splitter.
        /// </summary>
        [Bindable(true)]
        public Brush SplitterBorderBrush
        {
            get => (Brush)this.GetValue(SplitterBackgroundProperty);
            set => this.SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of the grid splitter.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public Thickness SplitterBorderThickness
        {
            get => (Thickness)this.GetValue(SplitterBorderThicknessProperty);
            set => this.SetValue(SplitterBorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of the grid splitter.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public double SplitterWidth
        {
            get => (double)this.GetValue(SplitterWidthProperty);
            set => this.SetValue(SplitterWidthProperty, value);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        public void SetDiffModel(string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            this.DiffModel = InlineDiffBuilder.Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        public void SetDiffModel(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            this.DiffModel = InlineDiffBuilder.Diff(differ, oldText, newText, ignoreWhiteSpace, ignoreCase);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="builder">The differ builder instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        /// <param name="chunker">The chunker.</param>
        public void SetDiffModel(IInlineDiffBuilder builder, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker chunker = null)
        {
            if (builder == null)
            {
                this.DiffModel = InlineDiffBuilder.Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);
                return;
            }

            this.DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace, ignoreCase, chunker ?? LineChunker.Instance);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldFile">The old text file to compare.</param>
        /// <param name="newFile">The new text file.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        /// <param name="chunker">The chunker.</param>
        /// <exception cref="ArgumentNullException">oldFile or newFile was null.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="IOException">Read file failed because of I/O exception.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the file.</exception>
        public void SetDiffModel(FileInfo oldFile, FileInfo newFile, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker chunker = null)
        {
            if (oldFile == null) throw new ArgumentNullException(nameof(oldFile), "oldFile should not be null.");
            if (newFile == null) throw new ArgumentNullException(nameof(newFile), "newFile should not be null.");
            var oldText = File.ReadAllText(oldFile.FullName);
            var newText = File.ReadAllText(newFile.FullName);
            var builder = new InlineDiffBuilder();
            this.DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace, ignoreCase, chunker ?? LineChunker.Instance);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldFile">The old text file to compare.</param>
        /// <param name="newFile">The new text file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        /// <param name="chunker">The chunker.</param>
        /// <exception cref="ArgumentNullException">oldFile or newFile was null.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="IOException">Read file failed because of I/O exception.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the file.</exception>
        public void SetDiffModel(FileInfo oldFile, FileInfo newFile, Encoding encoding, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker chunker = null)
        {
            if (oldFile == null) throw new ArgumentNullException(nameof(oldFile), "oldFile should not be null.");
            if (newFile == null) throw new ArgumentNullException(nameof(newFile), "newFile should not be null.");
            var oldText = File.ReadAllText(oldFile.FullName, encoding);
            var newText = File.ReadAllText(newFile.FullName, encoding);
            var builder = new InlineDiffBuilder();
            this.DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace, ignoreCase, chunker ?? LineChunker.Instance);
        }

        /// <summary>
        /// Refreshes.
        /// </summary>
        public void Refresh()
        {
            this.UpdateContent(this.DiffModel);
        }

        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="lineIndex">The index of line.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(int lineIndex)
        {
            return Helper.GoTo(ContentPanel, lineIndex);
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="m">The diff model.</param>
        private void UpdateContent(DiffPaneModel m)
        {
            Helper.RenderInlineDiffs(ContentPanel, m, this);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(InlineDiffViewer), null);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback = null)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(InlineDiffViewer), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }
    }
}
