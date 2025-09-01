using System;
using System.Collections.Generic;

namespace StarWars.Model
{
    public class OrderQueryParams
    {
        public string PharmacyId { get; set; }
        public List<Status> Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Sort { get; set; } = Constants.DEFAULT_SORT_BY;
        public string Dir { get; set; } = Constants.DEFAULT_DIRECTION;
        public int Page { get; set; } = Constants.DEFAULT_PAGE_START;
        public int PageSize { get; set; } = Constants.DEFAULT_PAGE_SIZE;

        public override string ToString()
        {
            var status = Status == null || Status.Count == 0 ? string.Empty : string.Join(",", Status);
            return $"PharmacyId:{PharmacyId}, Status:[{status}], From:{From}, To:{To}, Sort:{Sort}, Dir:{Dir}, Page:{Page}, PageSize:{PageSize}";
        }
    }
}
