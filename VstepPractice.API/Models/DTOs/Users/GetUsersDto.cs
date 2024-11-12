using VstepPractice.API.Common.Enums;
using VstepPractice.API.Common.Utils;

namespace VstepPractice.API.Models.DTOs.Users;

public class GetUsersDto
{
    public string? SearchTerm { get; set; }
    public IDictionary<string, SortOrder>? SortColumnAndOrder { get; set; }
    public int PageIndex { get; set; } = PagedResult<UserDto>.DefaultPageIndex;
    public int PageSize { get; set; } = PagedResult<UserDto>.DefaultPageSize;
}
