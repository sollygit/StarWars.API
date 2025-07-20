using Newtonsoft.Json;
using System;

namespace StarWars.Model
{
    public interface IAuditable
    {
        DateTimeOffset? CreatedOn { get; set; }
        DateTimeOffset? UpdatedOn { get; set; }
    }

    public class AuditableEntity : IAuditable
    {
        [JsonIgnore]
        public DateTimeOffset? UpdatedOn { get; set; }
        [JsonIgnore]
        public DateTimeOffset? CreatedOn { get; set; }
    }
}
