using System.Security.Claims;
using HRPayroll.Application.DTOs;
using HRPayroll.Application.Features.SalaryConfigs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalaryConfigsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalaryConfigsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{employeeId:guid}")]
    public async Task<ActionResult<SalaryConfigDto>> GetConfig(Guid employeeId)
    {
        // Nhân viên chỉ được xem cấu hình lương của chính mình
        if (User.IsInRole("Employee"))
        {
            var claim = User.FindFirstValue("employee_id");
            if (!Guid.TryParse(claim, out var selfId) || selfId != employeeId)
                return Forbid();
        }

        var result = await _mediator.Send(new GetSalaryConfigQuery { EmployeeId = employeeId });
        
        if (result == null)
            return NotFound(new { Message = "Salary configuration not found for this employee." });
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<SalaryConfigDto>> UpdateConfig([FromBody] UpdateSalaryConfigCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
