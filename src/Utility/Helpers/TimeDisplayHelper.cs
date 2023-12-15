using System;
using System.Text;

namespace Ocuda.Utility.Helpers
{
    public static class TimeDisplayHelper
    {
        public static string FormatMinutes(int minutes)
        {
            if (minutes == 1)
            {
                return $"{minutes} minute";
            }
            if (minutes < 60)
            {
                return $"{minutes} minutes";
            }
            var ts = TimeSpan.FromMinutes(minutes);
            var sb = new StringBuilder();
            if (ts.Days > 0)
            {
                sb.Append(ts.Days);
                if (ts.Days > 1)
                {
                    sb.Append(" days ");
                }
                else
                {
                    sb.Append(" day ");
                }
            }
            if (ts.Hours > 0)
            {
                sb.Append(ts.Hours);
                if (ts.Hours > 1)
                {
                    sb.Append(" hours ");
                }
                else
                {
                    sb.Append(" hour ");
                }
            }
            if (ts.Minutes > 0)
            {
                sb.Append(ts.Minutes);
                if (ts.Minutes > 1)
                {
                    sb.Append(" minutes ");
                }
                else
                {
                    sb.Append(" minute ");
                }
            }
            return sb.ToString();
        }
    }
}