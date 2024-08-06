using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace skill_composer.Models
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringExtensions
    {
        // Extension method to fix phone numbers
        public static string FixPhoneNumber(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;  // Return the original input if it's null or only whitespace

            // Remove all spaces from the string
            var cleanedInput = input.Replace(" ", "");

            // Check if the cleaned string starts with '+', add one if it doesn't
            if (!cleanedInput.StartsWith("+")) cleanedInput = "+" + cleanedInput;

            return cleanedInput;
        }

        /// <summary>
        /// Checks to see if the string is a phone number and conforms to E.164 format.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return true;

            var rtn = false;
            try
            {
                var util = PhoneNumberUtil.GetInstance();
                var pn = util.Parse(phoneNumber, null);
                rtn = util.IsValidNumber(pn);
            }
            catch (NumberParseException ex)
            {
                Console.WriteLine($"ERROR {ex.Message} Invalid phone number.");
            }
            return rtn;
        }

        /// <summary>
        /// Phone number to words.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static string PhoneNumberToWords(this string phoneNumber)
        {
            var digitToWord = new Dictionary<char, string>()
            {
                {'0', "zero"},
                {'1', "one"},
                {'2', "two"},
                {'3', "three"},
                {'4', "four"},
                {'5', "five"},
                {'6', "six"},
                {'7', "seven"},
                {'8', "eight"},
                {'9', "nine"},
                {'+', "plus"},
                {',', ","}
            };

            var wordsList = new List<string>();
            foreach (char c in phoneNumber)
            {
                if (digitToWord.ContainsKey(c))
                {
                    wordsList.Add(digitToWord[c]);
                }
            }

            return string.Join(" ", wordsList.Where(s => !string.IsNullOrEmpty(s))); // Remove any empty entries
        }

        /// <summary>
        /// Format Phone Numbers For Speech.
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public static string FormatPhoneNumbersForSpeech(this string speech)
        {
            const string pattern = @"\+\d{11}";

            foreach (Match match in Regex.Matches(speech, pattern))
            {
                Console.WriteLine($"{match.Value}");
                var processedNumber = match.Value.Trim(); // Trim any leading/trailing whitespace
                var numberInWords = processedNumber.FixPhoneNumber().FormatPhoneNumber(',').PhoneNumberToWords();
                speech = speech.Replace(processedNumber, numberInWords);
            }

            return speech;
        }

        /// <summary>
        /// Checks the string to see if its a date time.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidTimeStamp(this string value)
        {
            var success = DateTime.TryParse(value, out _);

            return success;
        }

        /// <summary>
        /// Converts a 24 hour time into a 12 hour time.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTwelveHourFormat(this string value)
        {
            if (DateTime.TryParseExact(value, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
            {
                var outputTime = parsedTime.ToString("hh:mm tt");
                return outputTime;
            }

            return value;
        }

        /// <summary>
        /// Formats a phone number from +447764250509 to +44 7764 250509,
        /// So the AI can read it better.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 12) // Check if the phone number has enough digits to format
            {
                return phoneNumber; // Not enough digits to format
            }

            // Format the phone number
            return $"{phoneNumber.Substring(0, 3)} " + // Country code              +44
                   $"{phoneNumber.Substring(3, 4)} " + // Area code                 7764
                   $"{phoneNumber.Substring(7)}";      // Rest of the phone number

        }

        /// <summary>
        /// Formats a phone number from +447764250509 to +44 7764 250509,
        /// So the AI can read it better.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(this string phoneNumber, char separator)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 12) // Check if the phone number has enough digits to format
            {
                return phoneNumber; // Not enough digits to format
            }

            // Format the phone number
            return $"{phoneNumber.Substring(0, 3)}{separator} " + // Country code              +44
                   $"{phoneNumber.Substring(3, 4)}{separator} " + // Area code                 7764
                   $"{phoneNumber.Substring(7)}";      // Rest of the phone number

        }

        /// <summary>
        /// Replace place holders with data.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="data"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static string ReplaceTarget<T1>(this string prompt, T1 data, string objectName = "")
        {
            var toFields = data.GetType().GetProperties();

            prompt = prompt.Replace("{Now}", $"{DateTime.Now:yyyy-MM-dd}", StringComparison.InvariantCultureIgnoreCase);

            foreach (var toField in toFields)
            {
                var propertyName = toField.Name;
                var propertyType = toField.PropertyType.Name;

                var value = toField.GetValue(data, null);
                var target = string.IsNullOrEmpty(objectName) ? $"{{{propertyName}}}" : $"{{{objectName}.{propertyName}}}";

                switch (propertyType)
                {
                    case "IList`1":
                        prompt = prompt.Replace(target, JsonConvert.SerializeObject(value));
                        break;
                    case "Int32" or "String":
                        value = value ?? string.Empty;
                        prompt = prompt.Replace(target, value.ToString());
                        break;
                    case "DateTime":
                        {
                            if (value != null) prompt = prompt.Replace(target, $"{(DateTime)value:yyyy-MM-dd hh:mm:ss}");
                            break;
                        }
                    default:
                        {
                            if (value == null) continue;
                            prompt = prompt.ReplaceTarget(value, propertyName);
                            break;
                        }
                }
            }

            return prompt;
        }

        /// <summary>
        /// Converts a string to a decimal.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal ToDecimalOrDefault(this string value, decimal defaultValue = 0)
        {
            return decimal.TryParse(value, out decimal result) ? result : defaultValue;
        }

        /// <summary>
        /// Converts a string to an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToIntegerOrDefault(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Accepts multiple parameters, returns true if the string contains 1 or more.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool Contains(this string source, params string[] values)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (values == null) throw new ArgumentNullException(nameof(values));

            return values.Any(value => source.Contains(value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Is the string JSON.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsJson(this string input)
        {
            try
            {
                if (input == null) return false;
                JToken.Parse(input);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        /// <summary>
        /// Is the string XML.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsXml(this string input)
        {
            try
            {
                if (input == null) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
