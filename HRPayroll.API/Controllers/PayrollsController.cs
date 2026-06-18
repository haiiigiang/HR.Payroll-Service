using System.Security.Claims;
using HRPayroll.Application.DTOs;
using HRPayroll.Application.Features.Payrolls;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PayrollDto>>> GetPayrolls([FromQuery] int? month, [FromQuery] int? year, [FromQuery] Guid? employeeId)
    {
        // Nhân viên chỉ được xem bảng lương của chính mình → ép employeeId theo claim employee_id
        if (User.IsInRole("Employee"))
        {
            var claim = User.FindFirstValue("employee_id");
            if (!Guid.TryParse(claim, out var selfId))
                return Forbid();
            employeeId = selfId;
        }

        var result = await _mediator.Send(new GetPayrollsQuery
        {
            Month = month,
            Year = year,
            EmployeeId = employeeId
        });

        return Ok(result);
    }

    [HttpPost("calculate")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CalculatePayroll([FromBody] CalculatePayrollCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("simulate-event")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> SimulateEvent([FromBody] CalculatePayrollCommand command, [FromServices] MassTransit.IPublishEndpoint publishEndpoint)
    {
        await publishEndpoint.Publish(new HRPayroll.Application.Events.AttendanceMonthlyClosedEvent
        {
            Month = command.Month,
            Year = command.Year,
            EmployeeId = command.EmployeeId,
            StandardWorkdays = command.StandardWorkdays,
            ActualWorkdays = command.ActualWorkdays,
            OvertimeHours = command.OvertimeHours,
            PaidLeaveDays = command.PaidLeaveDays,
            UnpaidLeaveDays = command.UnpaidLeaveDays
        });
        return Accepted(new { message = "Event published to RabbitMQ. Payroll calculation will run in background." });
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PayrollDto>> Approve(Guid id)
    {
        try
        {
            return Ok(await _mediator.Send(new ApprovePayrollCommand { Id = id }));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { ex.Message }); }
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PayrollDto>> Pay(Guid id)
    {
        try
        {
            return Ok(await _mediator.Send(new MarkPayrollPaidCommand { Id = id }));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { ex.Message }); }
    }
}
