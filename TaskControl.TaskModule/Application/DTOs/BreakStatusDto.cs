namespace TaskControl.TaskModule.Application.DTOs;

public class BreakStatusDto
{
    public bool IsOnBreak { get; set; }
    public int AccumulatedMinutes { get; set; }
    public bool CanStartBreak { get; set; }
    public bool IsLimitReached { get; set; }
    public bool HasActiveTasks { get; set; }
}