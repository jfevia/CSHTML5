﻿
//===============================================================================
//
//  IMPORTANT NOTICE, PLEASE READ CAREFULLY:
//
//  ● This code is dual-licensed (GPLv3 + Commercial). Commercial licenses can be obtained from: http://cshtml5.com
//
//  ● You are NOT allowed to:
//       – Use this code in a proprietary or closed-source project (unless you have obtained a commercial license)
//       – Mix this code with non-GPL-licensed code (such as MIT-licensed code), or distribute it under a different license
//       – Remove or modify this notice
//
//  ● Copyright 2019 Userware/CSHTML5. This code is part of the CSHTML5 product.
//
//===============================================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if MIGRATION
using System.Windows;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a System.Windows.Controls.DataGrid column that hosts template-specified
    /// content in its cells.
    /// </summary>
    public class DataGridTemplateColumn : DataGridBoundColumn
    {
        /// <summary>
        /// Gets or sets the template to use to display the contents of a cell that is
        /// not in editing mode.
        /// </summary>
        public DataTemplate CellTemplate
        {
            get { return (DataTemplate)GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }
        /// <summary>
        /// Identifies the System.Windows.Controls.DataGridTemplateColumn.CellTemplate
        /// dependency property.
        /// </summary>
        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register("CellTemplate", typeof(DataTemplate), typeof(DataGridTemplateColumn), new PropertyMetadata(null, CellTemplateProperty_Changed));

        private static void CellTemplateProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //todo: tell the DataGrid to redraw this column for all the children.
        }

        /// <summary>
        /// Gets or sets the template to use to display the contents of a cell that is
        /// in editing mode.</summary>
        public DataTemplate CellEditingTemplate
        {
            get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
            set { SetValue(CellEditingTemplateProperty, value); }
        }
        /// <summary>
        /// Identifies the System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate
        /// dependency property.
        /// </summary>
        public static readonly DependencyProperty CellEditingTemplateProperty =
            DependencyProperty.Register("CellEditingTemplate", typeof(DataTemplate), typeof(DataGridTemplateColumn), new PropertyMetadata(null, CellEditingTemplateProperty_Changed));

        private static void CellEditingTemplateProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //probably nothing to do here
        }

        internal override FrameworkElement GenerateElement(object childData)
        {
            if (CellTemplate != null)
            {
                FrameworkElement element = CellTemplate.INTERNAL_InstantiateFrameworkTemplate();
                element.DataContext = childData;
                Binding b = this.INTERNAL_GetBinding(DataGridBoundColumn.BindingProperty); //we get the Binding in the Binding property set by the user.
                if (b != null)
                {
                    if (b.Mode == BindingMode.OneWay)
                    {
                        if (!b.INTERNAL_WasModeSetByUserRatherThanDefaultValue())
                        {
                            b.Mode = BindingMode.TwoWay;
                        }
                    }
                    element.SetBinding(FrameworkElement.DataContextProperty, b);
                }
                return element;
            }
            else //we are going to make a ToString() of the element the user tries to show:
            {
                TextBlock textBlock = new TextBlock();
                textBlock.DataContext = childData;
                Binding b = this.INTERNAL_GetBinding(DataGridBoundColumn.BindingProperty); //we get the Binding in the Binding property set by the user.
                if (b != null)
                {
                    if (b.Mode == BindingMode.OneWay)
                    {
                        if (!b.INTERNAL_WasModeSetByUserRatherThanDefaultValue())
                        {
                            b.Mode = BindingMode.TwoWay;
                        }
                    }
                    textBlock.SetBinding(TextBlock.TextProperty, b);
                }
                else
                {
                    textBlock.Text = childData.ToString();
                }
                return textBlock;
            }
        }

        internal override FrameworkElement GenerateEditingElement(object childData)
        {
            if (CellEditingTemplate != null)
            {
                FrameworkElement element = CellEditingTemplate.INTERNAL_InstantiateFrameworkTemplate();
                element.DataContext = childData;
                Binding b = this.INTERNAL_GetBinding(DataGridBoundColumn.BindingProperty); //we get the Binding in the Binding property set by the user.
                if (b != null)
                    element.SetBinding(FrameworkElement.DataContextProperty, b);
                return element;
            }
            else
            {
                return GenerateElement(childData);
            }
        }
    }
}
