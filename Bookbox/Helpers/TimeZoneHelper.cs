using System;

namespace Bookbox.Helpers
{
    public static class TimeZoneHelper
    {
        private static readonly TimeZoneInfo NepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
        
        public static DateTime ToLocalTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, NepalTimeZone);
        }
        
        public static DateTime? ToLocalTime(DateTime? utcTime)
        {
            if (!utcTime.HasValue) return null;
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, NepalTimeZone);
        }
        
        public static DateTime ToUtcTime(DateTime localTime)
        {
            // Ensure the datetime is unspecified kind before conversion
            localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localTime, NepalTimeZone);
        }
        
        public static DateTime? ToUtcTime(DateTime? localTime)
        {
            if (!localTime.HasValue) return null;
            return ToUtcTime(localTime.Value);
        }
        
        public static DateTime GetLocalNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NepalTimeZone);
        }
    }
}