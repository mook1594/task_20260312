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
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType<PagedResult<EmployeeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
        [FromQuery] [Range(1, int.MaxValue, ErrorMessage = "page는 1 이상이어야 합니다.")] int page = 1,
        [FromQuery] [Range(1, 100, ErrorMessage = "pageSize는 1부터 100 사이여야 합니다.")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        PagedResult<EmployeeDto> result = await sender
            .Send(new GetEmployeesQuery(page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    /// <summary>
    /// 이름과 정확히 일치하는 직원 연락처 목록을 조회한다.
    /// </summary>
    /// <param name="name">trim 후 exact match로 조회할 직원 이름이다.</param>
    /// <param name="cancellationToken">요청 취소 토큰이다.</param>
    /// <returns>조건에 일치하는 직원 목록을 반환한다. 결과가 없으면 빈 배열을 반환한다.</returns>
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

        if (result.Created == 0)
        {
            throw new HttpProblemException(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "The request did not create any employees.");
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

}
