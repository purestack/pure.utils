
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Pure.Utils
{
    public class DateHelper
    {

        /// <summary>
        /// Handle parsing of dates with T-1, T+2 etc.
        /// </summary>
        /// <param name="dateStr">Dates with operation.</param>
        /// <returns>Calculated date.</returns>
        public static DateTime ParseTPlusMinusX(string dateStr)
        {
            return ParseTPlusMinusX(dateStr, DateTime.MinValue);
        }
        /// <summary>
        /// ��ȡʱ���
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time)
        {
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString();
        }
        /// <summary>  
        /// ��c# DateTimeʱ���ʽת��ΪUnixʱ�����ʽ  
        /// </summary>  
        /// <param name="time">ʱ��</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //��10000����Ϊ13λ      
            return t;
        }
    }
    /// <summary>
    /// Time parse result.
    /// </summary>
    public class DateParseResult
    {
        /// <summary>
        /// True if the date parse was valid.
        /// </summary>
        public readonly bool IsValid;


        /// <summary>
        /// Error of parse.
        /// </summary>
        public readonly string Error;


        /// <summary>
        /// Start datetime.
        /// </summary>
        public readonly DateTime Start;


        /// <summary>
        /// End datetime.
        /// </summary>
        public readonly DateTime End;


        /// <summary>
        /// Constructor to initialize the results
        /// </summary>
        /// <param name="valid">Valid flag.</param>
        /// <param name="error">Validation result.</param>
        /// <param name="start">Start datetime.</param>
        /// <param name="end">End datetime.</param>
        public DateParseResult(bool valid, string error, DateTime start, DateTime end)
        {
            IsValid = valid;
            Error = error;
            Start = start;
            End = end;
        }
    }



    /// <summary>
    /// Parses the dates.
    /// </summary>
    public class DateParser
    {
        /// <summary>
        /// Error for confirming start date &lt;= end date.
        /// </summary>
        public const string ErrorStartDateGreaterThanEnd = "End date must be greater or equal to start date.";


        ///// <summary>
        ///// Parses a string representing 2 dates.
        ///// The dates must be separated by the word "to".
        ///// </summary>
        ///// <param name="val">String representing two dates.</param>
        ///// <param name="errors">Parsing errors.</param>
        ///// <param name="delimiter">Delimiter separating the dates.</param>
        ///// <returns>Instance of date parse result with the result.</returns>
        //public static DateParseResult ParseDateRange(string val, IList<string> errors, string delimiter)
        //{
        //    int ndxTo = val.IndexOf(delimiter);

        //    // start and end date specified.
        //    string strStarts = val.Substring(0, ndxTo);
        //    string strEnds = val.Substring(ndxTo + delimiter.Length);
        //    DateTime ends = DateTime.Today;
        //    DateTime starts = DateTime.Today;
        //    int initialErrorCount = errors.Count;

        //    // Validate that the start and end date are supplied.
        //    if (string.IsNullOrEmpty(strStarts)) errors.Add("Start date not supplied.");
        //    if (string.IsNullOrEmpty(strEnds)) errors.Add("End date not supplied.");

        //    if (errors.Count > initialErrorCount)
        //        return new DateParseResult(false, errors[0], TimeParserConstants.MinDate, TimeParserConstants.MaxDate);

        //    // Validate that format of the start and end dates.
        //    if (!DateTime.TryParse(strStarts, out starts)) errors.Add("Start date '" + strStarts + "' is not valid.");
        //    if (!DateTime.TryParse(strEnds, out ends)) errors.Add("End date '" + strEnds + "' is not valid.");

        //    if (errors.Count > initialErrorCount)
        //        return new DateParseResult(false, errors[0], TimeParserConstants.MinDate, TimeParserConstants.MaxDate);

        //    // Validate ends date greater equal to start.
        //    if (starts.Date > ends.Date)
        //    {
        //        errors.Add(ErrorStartDateGreaterThanEnd);
        //        return new DateParseResult(false, errors[0], TimeParserConstants.MinDate, TimeParserConstants.MaxDate);
        //    }

        //    // Everything is good.
        //    return new DateParseResult(true, string.Empty, starts, ends);
        //}


        /// <summary>        
        /// ʱ���תΪC#��ʽʱ��        
        /// </summary>        
        /// <param name=��timeStamp��></param>        
        /// <returns></returns>        
        private DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// Handle parsing of dates with T-1, T+2 etc.
        /// </summary>
        /// <param name="dateStr">Dates with operation.</param>
        /// <param name="defaultVal">Default value.</param>
        /// <returns>Calculated date.</returns>
        public static DateTime ParseTPlusMinusX(string dateStr, DateTime defaultVal)
        {
            //(?<datepart>[0-9a-zA-Z\\/]+)\s*((?<addop>[\+\-]{1})\s*(?<addval>[0-9]+))? 
            string pattern = @"(?<datepart>[0-9a-zA-Z\\/]+)\s*((?<addop>[\+\-]{1})\s*(?<addval>[0-9]+))?";
            Match match = Regex.Match(dateStr, pattern);
            DateTime date = defaultVal;
            if (match.Success)
            {
                string datepart = match.Groups["datepart"].Value;
                if (datepart.ToLower().Trim() == "t")
                    date = DateTime.Now;
                else
                    date = DateTime.Parse(datepart);

                // Now check for +- days
                if (match.Groups["addop"].Success && match.Groups["addval"].Success)
                {
                    string addOp = match.Groups["addop"].Value;
                    int addVal = Convert.ToInt32(match.Groups["addval"].Value);
                    if (addOp == "-") addVal *= -1;
                    date = date.AddDays(addVal);
                }
            }
            return date;
        }
    }
}
