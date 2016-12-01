using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Capella
{
    public class CSTextBlock : TextBlock
    {
        public InlineCollection InlineCollection
        {
            get
            {
                return (InlineCollection)GetValue(InlineCollectionProperty);
            }
            set
            {
                SetValue(InlineCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty InlineCollectionProperty = DependencyProperty.Register(
            "InlineCollection",
            typeof(InlineCollection),
            typeof(CSTextBlock),
                new UIPropertyMetadata((PropertyChangedCallback)((sender, args) =>
                {
                    CSTextBlock textBlock = sender as CSTextBlock;

                    if (textBlock != null)
                    {
                        textBlock.Inlines.Clear();

                        InlineCollection inlines = args.NewValue as InlineCollection;

                        if (inlines != null)
                            textBlock.Inlines.AddRange(inlines.ToList());
                    }
                })));
    }
}
