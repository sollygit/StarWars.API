using System;

namespace StarWars.Model
{
    public class MovieRating : AuditableEntity
    {
        public Guid ID { get; set; }
        public string MovieID { get; set; }
        public string Rated { get; set; }
        public DateTime Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Language { get; set; }
        public int Metascore { get; set; }
        public decimal Rating { get; set; }
    }
}
