#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Date
{
	public static class DateTimeUtilities
	{
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || UNITY_2021_2_OR_NEWER
        public static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
#else
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif

        public static readonly long MaxUnixMs =
            (DateTime.MaxValue.Ticks - UnixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
        public static readonly long MinUnixMs = 0L;

        /// <summary>
        /// Return the number of milliseconds since the Unix epoch (1 Jan., 1970 UTC) for a given DateTime value.
        /// </summary>
        /// <remarks>The DateTime value will be converted to UTC (using <see cref="DateTime.ToUniversalTime"/> before
        /// conversion.</remarks>
        /// <param name="dateTime">A DateTime value not before the epoch.</param>
        /// <returns>Number of whole milliseconds after epoch.</returns>
        /// <exception cref="ArgumentOutOfRangeException">'dateTime' is before the epoch.</exception>
        public static long DateTimeToUnixMs(DateTime dateTime)
		{
            DateTime utc = dateTime.ToUniversalTime();
            if (utc.CompareTo(UnixEpoch) < 0)
				throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value may not be before the epoch");

			return (utc.Ticks - UnixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
		}

        /// <summary>
        /// Create a UTC DateTime value from the number of milliseconds since the Unix epoch (1 Jan., 1970 UTC).
        /// </summary>
        /// <param name="unixMs">Number of milliseconds since the epoch.</param>
        /// <returns>A UTC DateTime value</returns>
        /// <exception cref="ArgumentOutOfRangeException">'unixMs' is before 'MinUnixMs' or after 'MaxUnixMs'.
        /// </exception>
        public static DateTime UnixMsToDateTime(long unixMs)
		{
			if (unixMs < MinUnixMs || unixMs > MaxUnixMs)
				throw new ArgumentOutOfRangeException(nameof(unixMs));

            return new DateTime(unixMs * TimeSpan.TicksPerMillisecond + UnixEpoch.Ticks, DateTimeKind.Utc);
		}

		/// <summary>
		/// Return the current number of milliseconds since the Unix epoch (1 Jan., 1970 UTC).
		/// </summary>
		public static long CurrentUnixMs()
		{
			return DateTimeToUnixMs(DateTime.UtcNow);
		}

        public static DateTime WithPrecisionCentisecond(DateTime dateTime)
        {
            int millisecond = dateTime.Millisecond - (dateTime.Millisecond % 10);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, millisecond, dateTime.Kind);
        }

        public static DateTime WithPrecisionDecisecond(DateTime dateTime)
        {
            int millisecond = dateTime.Millisecond - (dateTime.Millisecond % 100);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, millisecond, dateTime.Kind);
        }

        public static DateTime WithPrecisionMillisecond(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Kind);
        }

        public static DateTime WithPrecisionSecond(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);
        }
    }
}
#pragma warning restore
#endif
