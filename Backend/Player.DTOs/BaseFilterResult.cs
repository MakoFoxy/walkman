using System.Collections.Generic;

namespace Player.DTOs
{
    public class BaseFilterResult<T>
    {
        public int? Page { get; set; }
        public int? ItemsPerPage { get; set; }
        public int TotalItems { get; set; }

        public ICollection<T> Result { get; set; } = new List<T>();
    }
}