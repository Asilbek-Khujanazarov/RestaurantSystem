public class Attendance
{
    public int Id { get; set; }
    public int CustomID { get; set; }
    public DateTime AttendanceTime { get; set; }
}
public class AttendanceRequest
{
    public int AttendanceStaffId { get; set; }
}