using System;
using System.Threading.Tasks;
using FfAdmin.Common;
namespace FfAdmin.Test
{
    public static class Events
    {
        public static Task<Event[]> ReadEvents<T>(this T testClass, int num)
            where T : ServiceConsumerTest
            => typeof(T).ReadEvents(num);
        public static async Task<Event[]> ReadEvents(this Type testClass, int num)
        {
            await using var str = typeof(Events).Assembly.GetManifestResourceStream($"FfAdmin.Test.data.{testClass.Name}.{num}.json");
            if (str == null)
                return Array.Empty<Event>();
            var events = await Event.ReadAll(str);
            return events;
        }
    }
}
