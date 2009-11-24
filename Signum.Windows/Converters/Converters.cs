﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using Signum.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Signum.Utilities.DataStructures;
using Signum.Utilities.ExpressionTrees;
using System.Windows;
using System.Windows.Media;
using Signum.Entities;
using Signum.Entities.Basics;
using Signum.Entities.DynamicQuery;
using Signum.Windows.Properties;
using Signum.Entities.Reflection;

namespace Signum.Windows
{
    public static class Converters
    {
        public static readonly IValueConverter Identity =
            ConverterFactory.New((object v) => v, (object v) => v);

        public static readonly IValueConverter ToLite =
           ConverterFactory.New((IIdentifiable ei) => ei.TryCC(e => Lite.Create(e.GetType(), (IdentifiableEntity)e)));

        public static readonly IValueConverter NullableEnumConverter =
            ConverterFactory.New((object v) => v == null ? "-" : v, (object v) => (v as string) == "-" ? null : v);

        public static readonly IValueConverter EnumDescriptionConverter =
            ConverterFactory.New((object v) => v is Enum ? EnumExtensions.NiceToString((Enum)v) : (string)v);

        public static readonly IValueConverter ErrorListToToolTipString =
            ConverterFactory.New((IEnumerable<ValidationError> err) => err.Select(e => e.ErrorContent).FirstOrDefault());

        public static readonly IValueConverter ErrorListToErrorCount =
            ConverterFactory.New((string[] str) => str == null ? null :
                                                 new Switch<int, string>(str.Length)
                                                 .Case(0, Properties.Resources.NoDirectErrors)
                                                 .Case(1, v => Properties.Resources._1Error.Formato(str[0]))
                                                 .Default(v => Properties.Resources._0Errors1.Formato(v, str[0])));

        public static readonly IValueConverter ErrorListToBool =
            ConverterFactory.New((string[] str) => str != null && str.Length > 0);

        public static readonly IValueConverter ErrorToInt =
            ConverterFactory.New((string str) => str.HasText() ? 1 : 0);

        public static readonly IValueConverter BoolToInt =
            ConverterFactory.New((bool b) => b ? 1 : 0);

        public static readonly IValueConverter BoolToBold =
            ConverterFactory.New((bool b) => b ? FontWeights.Bold : FontWeights.Normal);

        public static readonly IValueConverter CollapseStringEmpty =
            ConverterFactory.New((string s) => s == "" ? null : s);

        public static readonly IValueConverter BoolToVisibility =
            ConverterFactory.New((bool b) => b ? Visibility.Visible : Visibility.Collapsed);

        public static readonly IValueConverter NotBoolToVisibility =
            ConverterFactory.New((bool b) => b ? Visibility.Collapsed : Visibility.Visible);

        public static readonly IValueConverter NullToVisibility =
            ConverterFactory.New((object o) => o != null ? Visibility.Visible : Visibility.Collapsed);

        public static readonly IValueConverter NotNullToVisibility =
           ConverterFactory.New((object o) => o != null ? Visibility.Collapsed : Visibility.Visible);

        public static readonly IValueConverter IsNull =
            ConverterFactory.New((object o) => o == null);

        public static readonly IValueConverter IsNotNull =
            ConverterFactory.New((object o) => o != null);

        public static readonly IValueConverter BoolToSelectionMode =
            ConverterFactory.New((bool b) => b ? SelectionMode.Extended : SelectionMode.Single);

        public static readonly IValueConverter Not = ConverterFactory.New((bool b) => !b, (bool b) => !b);

        public static readonly IValueConverter TypeContextName =
            ConverterFactory.New((FrameworkElement b) => b.TryCC(fe => Common.GetTypeContext(fe)).TryCC(c => c.Type).TryCC(t => t.NiceName()) ?? "??");

        public static readonly IValueConverter TypeName =
            ConverterFactory.New((Type type) => type.TryCC(t => t.NiceName()) ?? "??");

        public static readonly IValueConverter TypeImage =
            ConverterFactory.New((Type type) => type.TryCC(t => Navigator.Manager.GetEntityIcon(type, true)));

        public static readonly IValueConverter ThicknessToCornerRadius =
            ConverterFactory.New((Thickness b) => new CornerRadius
            {
                BottomLeft = 2 * Math.Max(b.Bottom, b.Left),
                BottomRight = 2 * Math.Max(b.Bottom, b.Right),
                TopLeft = 2 * Math.Max(b.Top, b.Left),
                TopRight = 2 * Math.Max(b.Top, b.Right)
            });

        public static readonly IValueConverter ToStringConverter = ConverterFactory.New(
            (object d) => d.TryCC(a => a.ToString()));

        static readonly ColorConverter cc = new ColorConverter();
        public static readonly IValueConverter ColorConverter = ConverterFactory.New(
            (ColorDN c) => c == null ? null : (Color?)(System.Windows.Media.ColorConverter.ConvertFromString(c.Hex)),
            (Color? c) => c == null ? null : new ColorDN { Hex = cc.ConvertToString(c) });

        public static readonly IValueConverter FilterOperation = ConverterFactory.New((FilterOperation fo) => fo.NiceToString());
    }

    public static class ColorExtensions
    {
        public static Color Lerp(Color a, float coef, Color b)
        {
            return Color.FromScRgb(
                (1 - coef) * a.ScA + coef * b.ScA,
                (1 - coef) * a.ScR + coef * b.ScR,
                (1 - coef) * a.ScG + coef * b.ScG,
                (1 - coef) * a.ScB + coef * b.ScB);

        }

        public static Color Lerp(Color a, float coef, Color b, float alpha)
        {
            return Color.FromScRgb(
                alpha,
                (1 - coef) * a.ScR + coef * b.ScR,
                (1 - coef) * a.ScG + coef * b.ScG,
                (1 - coef) * a.ScB + coef * b.ScB);
        }
    }

}
