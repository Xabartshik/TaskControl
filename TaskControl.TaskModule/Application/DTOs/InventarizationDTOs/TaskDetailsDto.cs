namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public sealed record TaskProgressDto
    {
        public int Total { get; init; }
        public int Completed { get; init; }
        public double Percentage => Total == 0 ? 0 : System.Math.Round((double)Completed / Total * 100, 1);
    }

    public sealed record TaskBusinessIdentifiersDto
    {
        public int? OrderId { get; init; }
        public int? TaskId { get; init; }
    }

    public sealed record TaskDetailsDto
    {
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; init; } = CurrentSchemaVersion;
        public int AssignmentId { get; init; }
        public TaskProgressDto Progress { get; init; } = new();
        public TaskBusinessIdentifiersDto BusinessIdentifiers { get; init; } = new();
    }
}
