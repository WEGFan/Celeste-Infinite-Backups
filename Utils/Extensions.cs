using System;
using System.Globalization;

namespace Celeste.Mod.InfiniteBackups.Utils {
    public static class Extensions {
        public static string DialogGet(this string input, Language language = null) {
            return Dialog.Get(input, language);
        }

        /// <summary>
        /// Split a string into same length of text elements<br/>
        /// https://codereview.stackexchange.com/a/112018
        /// </summary>
        public static string[] SplitIntoFixedLength(this string value, int length, bool strict = false) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length == 0 && length != 0) {
                throw new ArgumentException($"The passed {nameof(value)} may not be empty if the {nameof(length)} != 0");
            }

            StringInfo stringInfo = new StringInfo(value);
            int valueLength = stringInfo.LengthInTextElements;
            if (valueLength != 0 && length < 1) {
                throw new ArgumentException($"The value of {nameof(length)} needs to be > 0");
            }
            if (strict && valueLength % length != 0) {
                throw new ArgumentException($"The passed {nameof(value)}'s length can't be split by the {nameof(length)}");
            }

            int currentLength = stringInfo.LengthInTextElements;
            if (currentLength == 0) {
                return new string[0];
            }

            int numberOfItems = currentLength / length;
            int remaining = currentLength > numberOfItems * length ? 1 : 0;
            string[] chunks = new string[numberOfItems + remaining];

            for (int i = 0; i < numberOfItems; i++) {
                chunks[i] = stringInfo.SubstringByTextElements(i * length, length);
            }
            if (remaining != 0) {
                chunks[numberOfItems] = stringInfo.SubstringByTextElements(numberOfItems * length);
            }
            return chunks;
        }
    }
}