using Microsoft.Extensions.Logging;

namespace StarWars.Common
{
    public static class LoggingEvents
    {
        public static readonly EventId INIT_DATABASE = new EventId(101, "Error whilst creating/seeding database");
        public static readonly EventId SEND_EMAIL = new EventId(201, "Error whilst sending email");
    }
}
