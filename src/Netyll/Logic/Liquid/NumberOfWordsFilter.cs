﻿using System.Text.RegularExpressions;

namespace Netyll.Logic.Liquid
{
    public static class NumberOfWordsFilter
    {
        public static string number_of_words(string input)
        {
            return CountWords(input).ToString();
        }

        private static int CountWords(string input)
        {
            var collection = Regex.Matches(input, @"[\S]+");
            return collection.Count;
        }
    }
}
