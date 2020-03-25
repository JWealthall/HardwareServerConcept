using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HardwareSvr
{
    public static class Extensions
    {
        #region String Extensions
        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string ReplaceAt(this string str, int index, char newChar)
        {
            //return new StringBuilder(str) { [index] = newChar }.ToString();
            return string.IsNullOrEmpty(str) || str.Length <= index ? str : string.Concat(str.Select((c, i) => i == index ? newChar : c));
        }

        public static string Right(this string str, int length)
        {
            if (string.IsNullOrEmpty(str) || length == 0) return string.Empty;
            // if length is greater than "size" resets "size"
            length = (str.Length < length ? str.Length : length);
            return str.Substring(str.Length - length);
        }

        public static bool IsInteger(this string str, bool allowSign = true, int? maxLength = null, int? maxValue = null, int? minValue = null)
        {
            if (!allowSign && str.Contains('-')) return false;
            str = str.Trim().Trim('-');
            if (str.Any(c => c < '0' || c > '9')) return false;
            if (maxLength.HasValue && str.Length > maxLength.Value) return false;
            if (!maxValue.HasValue && !minValue.HasValue) return true;
            // parse the integer
            var v = 0;
            if (!int.TryParse(str, out v)) return false;
            if (!maxValue.HasValue && v > maxValue.Value ) return false;
            if (!minValue.HasValue && v < minValue.Value ) return false;
            return true;
        }

        #endregion String Extensions
    }
}
