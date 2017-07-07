using System;
using System.Globalization;

namespace Monahrq.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Beginnings the of week.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static DateTime BeginningOfWeek(this DateTime dateTime)
        {
            //difference in days
            int diff = (int)dateTime.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek; //sunday=always0, monday=always1, etc.

            //As a result we need to have day 0,1,2,3,4,5,6 
            if (diff < 0)
            {
                diff += 7;
            }
            return dateTime.AddDays(-1 * diff).Date.BeginningOfDay();
        }

        /// <summary>
        /// Beginnings the of month.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static DateTime BeginningOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
        }


        /// <summary>
        /// Beginnings the of day.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static DateTime BeginningOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        /// <summary>
        /// Ends the of day.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        /// <summary>
        /// Works the days between.
        /// </summary>
        /// <param name="startD">The start d.</param>
        /// <param name="endD">The end d.</param>
        /// <returns></returns>
        public static int WorkDaysBetween(DateTime startDate, DateTime endDate)
        {
            var calcBusinessDays = 1 + ((endDate - startDate).TotalDays * 5 - (startDate.DayOfWeek - endDate.DayOfWeek) * 2) / 7;
            if ((int)endDate.DayOfWeek == 6) calcBusinessDays--;
            if ((int)startDate.DayOfWeek == 0) calcBusinessDays--;
            return (int)calcBusinessDays;
        }

        /// <summary>
        /// To the unix timestamp.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>The Unix fixed Timestamp.</returns>
        public static long ToUnixTimestamp(this DateTime target)
        {
            var unixTimestamp = Convert.ToInt64((target - new DateTime(1970, 1, 1, 0, 0, 0, target.Kind)).TotalSeconds);
            return unixTimestamp;
        }

        /// <summary>
        /// Converts the specified unix timestamp to a .Net System.DateTime representation.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="unixTimestamp">The unix timestamp.</param>
        /// <returns>The System.DateTime representation of the specified unix timestamp</returns>
        public static DateTime ToDateTimeFromUnixTimestamp(this DateTime target, long unixTimestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, target.Kind).AddSeconds(unixTimestamp);
        }

		/// <summary>
		/// Remove Milliseconds from the return DataTime.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime TrimMilliseconds(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
		}
	}
}
