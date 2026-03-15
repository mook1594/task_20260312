using EmployeeContacts.Api.Models;
using EmployeeContacts.Api.ProblemDetails;
using EmployeeContacts.Api.Services;
using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Application.Employees.Queries.GetEmployees;
using EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EmployeeContacts.Api.Controllers;

/// <summary>
/// 직원 연락처 조회와 일괄 등록 기능을 제공한다.
/// </summary>
[ApiController]
[Route("api/employee")]
public sealed class EmployeeController : ControllerBase
{
    private readonly ISender sender;
    private readonly EmployeeService employeeService;

    public EmployeeController(
        ISender sender,
        EmployeeService employeeService)
    {
        this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
    }

    /// <summary>
    /// 직원 목록을 페이지 단위로 조회한다.
    /// </summary>
    /// <param name="page">1부터 시작하는 페이지 번호다. 기본값은 1이다.</param>
    /// <param name="pageSize">페이지 크기다. 기본값은 20이고 최대값은 100이다.</param>
    /// <param name="cancellationToken">요청 취소 토큰이다.</param>
    /// <returns>이름과 식별자 오름차순으로 정렬된 직원 페이지 결과를 반환한다.</returns>
    /// <response code="200">직원 목록 조회 성공</response>
    /// <response code="400">page나 pageSize가 유효하지 않음</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType<PagedResponse<EmployeeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<EmployeeDto>>> GetEmployees(
        [FromQuery] [Range(1, int.MaxValue, ErrorMessage = "page는 1 이상이어야 합니다.")] int page = 1,
        [FromQuery] [Range(1, 100, ErrorMessage = "pageSize는 1부터 100 사이여야 합니다.")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        PagedResult<EmployeeDto> result = await sender
            .Send(new GetEmployeesQuery(page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        string? nextUrl = result.Page < result.TotalPages
            ? Url.Action(nameof(GetEmployees), new { page = result.Page + 1, pageSize })
            : null;
        string? prevUrl = result.Page > 1
            ? Url.Action(nameof(GetEmployees), new { page = result.Page - 1, pageSize })
            : null;

        var response = new PagedResponse<EmployeeDto>
        {
            Items = result.Items,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            Links = new PagedLinks { Next = nextUrl, Prev = prevUrl }
        };

        return Ok(response);
    }

    /// <summary>
    /// 이름과 정확히 일치하는 직원 연락처 목록을 조회한다.
    /// </summary>
    /// <param name="name">trim 후 exact match로 조회할 직원 이름이다.</param>
    /// <param name="cancellationToken">요청 취소 토큰이다.</param>
    /// <returns>조건에 일치하는 직원 목록을 반환한다. 결과가 없으면 빈 배열을 반환한다.</returns>
    /// <response code="200">직원 조회 성공. 조건에 맞는 직원이 없으면 빈 배열 반환</response>
    /// <response code="400">name이 유효하지 않음</response>
    [HttpGet("{name}")]
    [Produces("application/json")]
    [ProducesResponseType<IReadOnlyList<EmployeeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetEmployeesByName(
        string name,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<EmployeeDto> result = await sender
            .Send(new GetEmployeesByNameQuery(name), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    /// <summary>
    /// CSV 또는 JSON 형식의 직원 데이터를 일괄 등록한다.
    /// </summary>
    /// <remarks>
    /// 지원 Content-Type:
    ///
    /// - `application/json`: JSON 배열 본문
    /// - `text/csv`: CSV 텍스트 본문
    /// - `text/plain`: JSON 배열 우선 판별 후 실패 시 CSV 처리
    /// - `multipart/form-data`: `employeesFile` 파일 파트 1개
    ///
    /// JSON 예시:
    ///
    /// ```json
    /// [
    ///   { "name": "김철수", "email": "kim@example.com", "tel": "010-1234-5678", "joined": "2024-02-01" }
    /// ]
    /// ```
    ///
    /// CSV 예시:
    ///
    /// ```csv
    /// 김철수,kim@example.com,01012345678,2024-02-01
    /// ```
    /// </remarks>
    /// <param name="cancellationToken">요청 취소 토큰이다.</param>
    /// <returns>부분 성공을 포함한 일괄 등록 처리 결과를 반환한다.</returns>
    /// <response code="201">직원 일괄 등록 완료. created > 0이면 부분 성공, failed > 0이면 일부 행 실패</response>
    /// <response code="400">요청 본문 형식이 유효하지 않거나 필드 값이 검증 실패</response>
    /// <response code="415">Content-Type 헤더가 지원되지 않음</response>
    /// <response code="500">서버 오류 발생</response>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType<BulkCreateEmployeesResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkCreateEmployeesResult>> BulkCreateEmployees(CancellationToken cancellationToken)
    {
        IReadOnlyList<BulkEmployeeRecord> records = await employeeService
            .ParseRecordsAsync(Request, cancellationToken)
            .ConfigureAwait(false);
        BulkCreateEmployeesResult result = await sender
            .Send(new BulkCreateEmployeesCommand(records), cancellationToken)
            .ConfigureAwait(false);

        return StatusCode(StatusCodes.Status201Created, result);
    }

}
