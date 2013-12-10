using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TomatoTimerWPF
{
    public static class Extensions
    {
        public static Visibility ToVisibility(this bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string ToFormat(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static T Chain<T>(this T target, Action<T> action)
        {
            action(target);
            return target;
        }

        //public static BitmapFrame GetBitmapFrame(this Icon icon)
        //{
        //    return BitmapFrame.Create(icon.GetStream());
        //}

        //public static Stream GetStream(this Icon icon)
        //{
        //    var stream = new MemoryStream();
        //    icon.Save(stream);
        //    return stream;
        //}

        public static TimeSpan Milliseconds(this int ms)
        {
            return new TimeSpan(0, 0, 0, 0, ms);
        }

        public static TimeSpan Seconds(this int seconds)
        {
            return new TimeSpan(0, 0, seconds);
        }

        public static TimeSpan Minutes(this int minutes)
        {
            return new TimeSpan(0, minutes, 0);
        }

        public static bool IsNegativeOrZero(this TimeSpan span)
        {
            return span.TotalMilliseconds <= 0;
        }
    }
}
