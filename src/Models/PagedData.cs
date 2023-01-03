using System.Collections.Generic;

namespace api.Models
{
    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }
        public bool HasPagination { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
        public long? TotalDocuments { get; set; }
    }
}