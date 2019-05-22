using System;

namespace Job
{
    class Daylight
    {
        private static void DaylightSavingTime()
        {
            DateTime thisTime = DateTime.UtcNow.AddMonths(-3);
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);

            var localZone = TimeZoneInfo.Local;
            var now = DateTime.Now;
            var utc = DateTime.UtcNow;

            Console.WriteLine(TimeZoneInfo.Local);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(DateTime.UtcNow);

            var x = DateTimeOffset.Now.Offset;
            Console.WriteLine(x);

            DateTime localTime1 = new DateTime(2019, 1, 10, 7, 0, 0);
            //localTime1 = DateTime.SpecifyKind(localTime1, DateTimeKind.Local);
            //Console.WriteLine(localTime1.ToUniversalTime());

            DateTime localTime2 = new DateTime(2019, 4, 10, 7, 0, 0);
            //localTime2 = DateTime.SpecifyKind(localTime2, DateTimeKind.Local);
            //Console.WriteLine(localTime2.ToUniversalTime());

            Console.WriteLine(new DateTimeOffset(localTime1));
            Console.WriteLine(new DateTimeOffset(localTime2));


            Console.WriteLine(DateTime.UtcNow.Add(x));
        }

    }
}
