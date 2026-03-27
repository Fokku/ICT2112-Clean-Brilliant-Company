namespace CleanBrilliant.DTO
{
    public class CarbonLogPageDTO
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<CarbonLogEntryDTO> Items { get; set; } = [];
    }
}
