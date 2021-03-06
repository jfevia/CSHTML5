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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if MIGRATION
namespace System.Windows.Media
#else
namespace Windows.UI.Xaml.Media
#endif
{
    #if FOR_DESIGN_TIME
    /// <summary>
    /// Used to convert a System.Windows.Media.Matrix object to or from another object
    /// type.
    /// </summary>
    public sealed class MatrixConverter : TypeConverter
    {
        /// <summary>
        /// Determines whether this class can convert an object of a given type to a
        /// System.Windows.Media.Matrix object.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="sourceType">The type from which to convert.</param>
        /// <returns>
        /// true if the type converter can create an instance of System.Windows.Matrix
        /// from the specified type; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
        
        /// <summary>
        /// Determines whether this class can convert an object of a given type to the
        /// specified destination type.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="destinationType">The destination type.</param>
        /// <returns>Returns true if conversion is possible; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }
        
        // Exceptions:
        //   System.NotSupportedException:
        //     value is NULL or cannot be converted to a System.Windows.Media.Matrix.
        /// <summary>
        /// Converts from an object of a given type to a System.Windows.Media.Matrix object.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="culture">The culture information that applies to the conversion.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>
        /// Returns a new System.Windows.Media.Matrix object if successful; otherwise,
        /// NULL.
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                throw GetConvertFromException(value);

            if (value is string)
                return Matrix.INTERNAL_ConvertFromString((string)value);

            return base.ConvertFrom(context, culture, value);
        }
   
        // Exceptions:
        //   System.NotSupportedException:
        //     value is NULL or it is not a System.Windows.Media.Matrix-or-destinationType
        //     is not a valid destination type.
        /// <summary>
        /// Converts a System.Windows.Media.Matrix object to a specified type, using the
        /// specified context and culture information.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="culture">The current culture information.</param>
        /// <param name="value">The System.Windows.Media.Matrix to convert.</param>
        /// <param name="destinationType">The destination type that the value object is converted to.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            throw new NotImplementedException();
        }
    }
#endif
}