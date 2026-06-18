using HRPayroll.Application.DTOs;
using HRPayroll.Application.Features.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardReportDto>> GetDashboard([FromQuery] int month, [FromQuery] int year)
    {
        var result = await _mediator.Send(new GetDashboardReportQuery
        {
            Month = month,
            Year = year
        });
        
        return Ok(result);
    }
}
