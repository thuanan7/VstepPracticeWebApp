using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VstepPractice.API.Common.Constant;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Exams.Requests;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.DTOs.Questions.Requests;
using VstepPractice.API.Models.DTOs.Questions.Responses;
using VstepPractice.API.Models.DTOs.Sections.Requests;
using VstepPractice.API.Services.Exams;

namespace VstepPractice.API.Controllers.V1;

[ApiVersion(1)]
public class ExamController : ApiController
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// Get paged list of exams
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _examService.GetExamsAsync(pageIndex, pageSize, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get exam by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExam(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _examService.GetExamByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new exam
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateExam(
        [FromBody] CreateExamRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.CreateExamAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetExam),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Update an existing exam
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateExam(
        int id,
        [FromBody] UpdateExamRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.UpdateExamAsync(id, userId, request, cancellationToken);

        if (!result.IsSuccess)
            return result.Error == Error.NotFound ? NotFound(result.Error) : BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete an exam
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteExam(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.DeleteExamAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return NoContent();
    }

    /// <summary>
    /// Get exams created by current user
    /// </summary>
    [HttpGet("my-exams")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(typeof(PagedResult<ExamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyExams(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.GetExamsByUserIdAsync(userId, pageIndex, pageSize, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Update exam sections
    /// </summary>
    [HttpPut("{id}/sections")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateExamSections(
        int id,
        [FromBody] List<CreateSectionRequest> request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.UpdateExamSectionsAsync(id, userId, request, cancellationToken);

        if (!result.IsSuccess)
            return result.Error == Error.NotFound ? NotFound(result.Error) : BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Add question to section
    /// </summary>
    [HttpPost("{examId}/sections/{sectionId}/questions")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddQuestion(
        int examId,
        int sectionId,
        [FromBody] CreateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.AddQuestionAsync(examId, sectionId, userId, request, cancellationToken);

        if (!result.IsSuccess)
            return result.Error == Error.NotFound ? NotFound(result.Error) : BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetExam),
            new { id = examId },
            result.Value);
    }

    /// <summary>
    /// Delete question from section
    /// </summary>
    [HttpDelete("{examId}/sections/{sectionId}/questions/{questionId}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Teacher}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteQuestion(
        int examId,
        int sectionId,
        int questionId,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(CustomClaimTypes.UserId)!.Value);
        var result = await _examService.DeleteQuestionAsync(examId, sectionId, questionId, userId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return NoContent();
    }
}