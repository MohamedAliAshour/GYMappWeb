namespace GYMappWeb.Helper
{
    public class UserParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        // Filtering parameters
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "UserCode";
        public bool SortDescending { get; set; } = false;
    }
}
