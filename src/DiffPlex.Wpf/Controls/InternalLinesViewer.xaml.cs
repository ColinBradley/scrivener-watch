using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace DiffPlex.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for InternalLinesViewer.xaml
    /// </summary>
    internal partial class InternalLinesViewer : UserControl
    {
        private readonly Dictionary<string, Binding> bindings = new Dictionary<string, Binding>();

        public InternalLinesViewer()
        {
            this.InitializeComponent();
        }

        public event ScrollChangedEventHandler ScrollChanged
        {
            add => ValueScrollViewer.ScrollChanged += value;
            remove => ValueScrollViewer.ScrollChanged -= value;
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => ValueScrollViewer.VerticalScrollBarVisibility;
            set => ValueScrollViewer.VerticalScrollBarVisibility = value;
        }

        public Guid TrackingId { get; set; }

        public double VerticalOffset => ValueScrollViewer.VerticalOffset;

        public int LineNumberWidth
        {
            get
            {
                return (int)(NumberColumn.ActualWidth + OperationColumn.ActualWidth);
            }

            set
            {
                var aThird = value / 3;
                OperationColumn.Width = new GridLength(aThird);
                NumberColumn.Width = new GridLength(value - aThird);
            }
        }

        public void Clear()
        {
            NumberPanel.Children.Clear();
            OperationPanel.Children.Clear();
            ValuePanel.Inlines.Clear();
        }

        public Span Add(string value, string changeType, UIElement source)
        {
            var span = new Span();
            var lines = value.Split('\n');
            for (var index = 0; index < lines.Length; index++) 
            {
                span.Inlines.Add(new Run(lines[index]));
                span.Inlines.Add(new LineBreak());
            }

            span.SetBinding(BackgroundProperty, this.GetBindings(changeType + "Background", source));

            if (!string.IsNullOrEmpty(value))
            {
                span.SetBinding(TextElement.ForegroundProperty, this.GetBindings(changeType + "Foreground", source, this.Foreground));
                span.SetBinding(TextElement.BackgroundProperty, this.GetBindings(changeType + "Background", source));
                this.ApplyTextBlockProperties(span, source);
            }

            ValuePanel.Inlines.Add(span);
            return span;
        }

        public Span Add(List<KeyValuePair<string, string>>? values, string changeType, UIElement source)
        {
            var span = new Span();
            span.SetBinding(BackgroundProperty, this.GetBindings(changeType + "Background", source));

            if (values != null)
            {
                foreach (var ele in values)
                {
                    if (string.IsNullOrEmpty(ele.Key))
                        continue;

                    var textSpan = new Span();
                    var lines = ele.Key.Split('\n');
                    for (var index = 0; index < lines.Length; index++)
                    {
                        var run = new Run(lines[index]);
                        span.Inlines.Add(run);

                        run.SetBinding(TextElement.BackgroundProperty, this.GetBindings(ele.Value + "Background", source));
                        run.SetBinding(TextElement.ForegroundProperty, this.GetBindings(ele.Value + "Foreground", source, this.Foreground));
                        
                        if (index < lines.Length - 1)
                            span.Inlines.Add(new LineBreak());
                    }

                    this.ApplyTextBlockProperties(textSpan, source);
                    span.Inlines.Add(textSpan);
                }
                span.Inlines.Add(new LineBreak());
            }

            ValuePanel.Inlines.Add(span);
            return span;
        }

        private Binding GetBindings(string key, UIElement source)
        {
            if (bindings.TryGetValue(key, out var r) && r.Source == source) return r;
            return bindings[key] = new Binding(key) { Source = source, Mode = BindingMode.OneWay };
        }

        private Binding GetBindings(string key, UIElement source, object defaultValue)
        {
            if (bindings.TryGetValue(key, out var r) && r.Source == source) return r;
            return bindings[key] = new Binding(key) { Source = source, Mode = BindingMode.OneWay, TargetNullValue = defaultValue };
        }

        public void ScrollToVerticalOffset(double offset)
        {
            ValueScrollViewer.ScrollToVerticalOffset(offset);
        }

        internal void AdjustScrollView()
        {
            var isV = ValueScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
            var hasV = ValuePanel.Margin.Bottom > 10;
            if (isV)
            {
                if (!hasV) ValuePanel.Margin = NumberPanel.Margin = OperationPanel.Margin = new Thickness(0, 0, 0, 20);
            }
            else
            {
                if (hasV) ValuePanel.Margin = NumberPanel.Margin = OperationPanel.Margin = new Thickness(0);
            }
        }

        private void ApplyTextBlockProperties(TextElement text, UIElement source)
        {
            text.SetBinding(TextElement.FontSizeProperty, this.GetBindings("FontSize", source));
            text.SetBinding(TextElement.FontFamilyProperty, this.GetBindings("FontFamily", source, Helper.FontFamily));
            text.SetBinding(TextElement.FontWeightProperty, this.GetBindings("FontWeight", source));
            text.SetBinding(TextElement.FontStretchProperty, this.GetBindings("FontStretch", source));
            text.SetBinding(TextElement.FontStyleProperty, this.GetBindings("FontStyle", source));
        }

        private void NumberScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = NumberScrollViewer.VerticalOffset;
            this.ScrollVertical(ValueScrollViewer, offset);
        }

        private void OperationScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = OperationScrollViewer.VerticalOffset;
            this.ScrollVertical(ValueScrollViewer, offset);
        }

        private void ValueScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = ValueScrollViewer.VerticalOffset;
            this.ScrollVertical(NumberScrollViewer, offset);
            this.ScrollVertical(OperationScrollViewer, offset);
        }

        private void ScrollVertical(ScrollViewer scrollViewer, double offset)
        {
            if (Math.Abs(scrollViewer.VerticalOffset - offset) > 1)
                scrollViewer.ScrollToVerticalOffset(offset);
        }

        private void ValueScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AdjustScrollView();
        }
    }
}
