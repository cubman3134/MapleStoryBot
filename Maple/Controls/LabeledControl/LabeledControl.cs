using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Maple.Controls.LabeledControl
{
    public class LabeledControl : HeaderedContentControl
    {
        static LabeledControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledControl), new FrameworkPropertyMetadata(typeof(LabeledControl)));
        }

        public double LabelWidth
        {
            get { return (double)GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }

        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register("LabelWidth", typeof(double), typeof(LabeledControl), new PropertyMetadata(Double.NaN));

        public double LabelHeight
        {
            get { return (double)GetValue(LabelHeightProperty); }
            set { SetValue(LabelHeightProperty, value); }
        }

        public static readonly DependencyProperty LabelHeightProperty =
            DependencyProperty.Register("LabelHeight", typeof(double), typeof(LabeledControl), new PropertyMetadata(Double.NaN));

        public Dock LabelDock
        {
            get { return (Dock)GetValue(LabelDockProperty); }
            set
            {
                switch (value)
                {
                    case Dock.Bottom:
                        break;
                    case Dock.Left:
                        break;
                    case Dock.Right:
                        break;
                    case Dock.Top:
                        break;
                }
                SetValue(LabelDockProperty, value);
            }
        }

        public static readonly DependencyProperty LabelDockProperty =
            DependencyProperty.Register("LabelDock", typeof(Dock), typeof(LabeledControl), new PropertyMetadata(Dock.Left));

        public TextAlignment LabelTextAlignment
        {
            get { return (TextAlignment)GetValue(LabelTextAlignmentProperty); }
            set { SetValue(LabelTextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty LabelTextAlignmentProperty =
            DependencyProperty.Register("LabelTextAlignment", typeof(TextAlignment), typeof(LabeledControl), new PropertyMetadata(TextAlignment.Left));
    }
}
