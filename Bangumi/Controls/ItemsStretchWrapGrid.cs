using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Bangumi.Controls
{
    public class ItemsStretchWrapGrid : Panel
    {
        /// <summary>
        /// Gets or sets the desired width for each column.
        /// </summary>
        /// <remarks>
        /// The width of columns can exceed the DesiredColumnWidth if the HorizontalAlignment is set to Stretch.
        /// </remarks>
        public double DesiredColumnWidth
        {
            get { return (double)GetValue(DesiredColumnWidthProperty); }
            set { SetValue(DesiredColumnWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DesiredColumnWidth"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="DesiredColumnWidth"/> dependency property.</returns>
        public static readonly DependencyProperty DesiredColumnWidthProperty = DependencyProperty.Register(
            nameof(DesiredColumnWidth),
            typeof(double),
            typeof(ItemsStretchWrapGrid),
            new PropertyMetadata(250d, OnDesiredColumnWidthChanged));

        private static void OnDesiredColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (ItemsStretchWrapGrid)d;
            panel.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var numColumns = (int)Math.Floor(availableSize.Width / Math.Min(DesiredColumnWidth, availableSize.Width));
            var columnWidth = availableSize.Width / numColumns;

            double desiredHeight = 0.0;

            int i = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(columnWidth, availableSize.Height));
                if (i % numColumns == 0)
                {
                    desiredHeight += child.DesiredSize.Height;    //Total height needs to be summed up.
                }
                i++;
            }

            desiredHeight = double.IsPositiveInfinity(availableSize.Height) ? desiredHeight : availableSize.Height;

            return new Size(availableSize.Width, desiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var numColumns = (int)Math.Floor(finalSize.Width / Math.Min(DesiredColumnWidth, finalSize.Width));
            var columnWidth = finalSize.Width / numColumns;

            double x = 0.0;
            double y = 0.0;

            int i = 0;
            foreach (UIElement child in Children)
            {
                if (i != 0 && i % numColumns == 0)
                {
                    y += child.DesiredSize.Height;
                    x = 0.0;
                }
                Rect rec = new Rect(new Point(x, y), new Size(columnWidth, child.DesiredSize.Height));
                child.Arrange(rec);
                x += columnWidth;
                i++;
            }
            return finalSize;
        }
    }
}
