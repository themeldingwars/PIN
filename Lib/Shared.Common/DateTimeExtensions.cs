using System;

namespace Shared.Common;

public static class DateTimeExtensions
{
    private static DateTime? _epoch;

    private static DateTime CheckedEpochUTC
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

    public static DateTime Epoch => CheckedEpochUTC.ToLocalTime();

    public static DateTime EpochUTC => CheckedEpochUTC;

    public static double UnixTimestamp(this DateTime dt)
    {
        return (dt - CheckedEpochUTC.ToLocalTime()).TotalSeconds;
    }

    public static double UnixTimestampUTC(this DateTime dt)
    {
        return (dt - CheckedEpochUTC).TotalSeconds;
    }
}