using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Extensions;
using VstepPractice.API.Models.DTOs.Users;
using VstepPractice.API.Services.Users;

namespace VstepPractice.API.Controllers.V1;

[ApiVersion(1)]
public class UserController : ApiController
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
        : base()
    {
        _userService = userService;
    }

    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound();

        return Ok(result);
    }

    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(Result<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        string? searchTerm = null,
        string? sortColumnAndOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await _userService.GetUsersAsync(new GetUsersDto
        {
            SearchTerm = searchTerm,
            SortColumnAndOrder = SortOrderExtension.ConvertStringToSortOrderV2(sortColumnAndOrder, UserExtension.GetSortProductProperty),
            PageIndex = pageIndex,
            PageSize = pageSize
        });
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateUser(
        CreateUserDto model,
        CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(model, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.Id }, result.Value);
    }
}
