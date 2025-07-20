using Microsoft.Extensions.Logging;

namespace StarWars.Common
{
    public static class LoggingEvents
    {
        public static readonly EventId INIT_DATABASE = new (101, "Error whilst creating/seeding database");
        public static readonly EventId SEND_EMAIL = new(201, "Error whilst sending email");
    }
}
