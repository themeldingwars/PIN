using System;

namespace Shared.Common;

public static class DateTimeExtensions
{
    private static DateTime? _epoch;

    private static DateTime CheckedEpochUtc
    {
        get
        {
            if (_epoch == null || !_epoch.HasValue)
            {
                _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            return _epoch.Value;
        }
    }

    public static DateTime Epoch => CheckedEpochUtc.ToLocalTime();

    public static DateTime EpochUtc => CheckedEpochUtc;

    public static double UnixTimestamp(this DateTime dateTime)
    {
        return (dateTime - CheckedEpochUtc.ToLocalTime()).TotalSeconds;
    }

    public static double UnixTimestampUtc(this DateTime dateTime)
    {
        return (dateTime - CheckedEpochUtc).TotalSeconds;
    }
}