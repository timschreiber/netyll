﻿using System;

namespace Netyll.Logic.Liquid
{
    public static class DateToRfc822FormatFilter
    {
        public static string date_to_rfc822(DateTime input)
        {
            var rfc822 = input.ToString("r");
            var offset = TimeZoneInfo.Local.GetUtcOffset(input).ToString();

            // if local time is behind utc time, offset should start with "-".
            // otherwise, add a plus sign to the beginning of the string.
            if (!offset.StartsWith("-"))
                offset = "+" + offset; // Add a (+) if it's a UTC+ timezone
            offset = offset.Substring(0, 6); // only want the first 6 chars.
            offset = offset.Replace(":", ""); // remove colons.
            // offset now looks something like "-0700".
            rfc822 = rfc822.Replace("GMT", offset);

            return rfc822;
        }

        public static string date_to_rfc822(string input)
        {
            DateTime inputDate;

            if (DateTime.TryParse(input, out inputDate))
            {
                return date_to_rfc822(inputDate);
            }

            return "";
        }
    }
}
