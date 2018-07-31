
using System;
using NodaTime;
using NodaTime.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a non-local-time DateTime to a local-time DateTime based on the
    /// specified timezone. The returned object will be of Unspecified DateTimeKind 
    /// which represents local time agnostic to servers timezone. To be used when
    /// we want to convert UTC to local time somewhere in the world.
    /// </summary>
    /// <param name="dateTime">Non-local DateTime as UTC or Unspecified DateTimeKind.</param>
    /// <param name="timezone">Timezone name (in TZDB format).</param>
    /// <returns>Local DateTime as Unspecified DateTimeKind.</returns>
    public static DateTime ToZone(this DateTime dateTime, string timezone)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            throw new ArgumentException("Expected non-local kind of DateTime");

        var zone = DateTimeZoneProviders.Tzdb[timezone];
        Instant instant = dateTime.ToInstant();
        ZonedDateTime inZone = instant.InZone(zone);
        DateTime unspecified = inZone.ToDateTimeUnspecified();

        return unspecified;
    }
        
    /// <summary>
    /// Converts a local-time DateTime to UTC DateTime based on the specified
    /// timezone. The returned object will be of UTC DateTimeKind. To be used
    /// when we want to know what's the UTC representation of the time somewhere
    /// in the world.
    /// </summary>
    /// <param name="dateTime">Local DateTime as UTC or Unspecified DateTimeKind.</param>
    /// <param name="timezone">Timezone name (in TZDB format).</param>
    /// <returns>UTC DateTime as UTC DateTimeKind.</returns>
    public static DateTime InZone(this DateTime dateTime, string timezone)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            throw new ArgumentException("Expected non-local kind of DateTime");

        var zone = DateTimeZoneProviders.Tzdb[timezone];
        LocalDateTime asLocal = dateTime.ToLocalDateTime();
        ZonedDateTime asZoned = asLocal.InZoneLeniently(zone);
        Instant instant = asZoned.ToInstant();
        ZonedDateTime asZonedInUtc = instant.InUtc();
        DateTime utc = asZonedInUtc.ToDateTimeUtc();

        return utc;
    }
}