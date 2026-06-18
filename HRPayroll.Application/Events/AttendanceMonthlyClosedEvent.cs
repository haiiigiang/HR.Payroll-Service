using System;

namespace HRPayroll.Application.Events
{
    // Hợp đồng event do Attendance Service (N2) publish khi chốt công cuối tháng.
    // QUAN TRỌNG: namespace + tên class phải TRÙNG với bên N2 publish thì MassTransit mới định tuyến đúng.
    public class AttendanceMonthlyClosedEvent
    {
        public Guid? EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // === Số liệu chấm công để N3 tính lương theo thực tế đi làm ===
        public decimal StandardWorkdays { get; set; }  // công chuẩn của tháng (vd 22)
        public decimal ActualWorkdays { get; set; }     // số ngày công thực tế
        public decimal OvertimeHours { get; set; }      // tổng giờ tăng ca
        public decimal PaidLeaveDays { get; set; }      // nghỉ phép có lương
        public decimal UnpaidLeaveDays { get; set; }    // nghỉ không lương
    }
}
