namespace Player.DTOs
{
    public class BaseFilterModel
    {
        public int? ItemsPerPage { get; set; }
        public int? Page { get; set; }
        public string SortBy { get; set; }
        public bool Descending { get; set; }
    }
}