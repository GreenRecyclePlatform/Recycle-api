// recycle.Application/DTOs/Payment/PagedResultDto.cs

namespace recycle.Application.DTOs.Payment
{
    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}