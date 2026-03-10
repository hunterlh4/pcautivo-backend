namespace PCautivoCore.Domain.Models;

public class Incident
{
    public int Id { get; set; }
    public int? InspectionId { get; set; }
    public int? PropertyId { get; set; }
    public int? RoomId { get; set; }
    public int? ItemId { get; set; }
    public IncidentType IssueType { get; set; }
    public DateTimeOffset IssueDate { get; set; }
    public string? Comments { get; set; }
    public string? Note { get; set; }
    public IncidentState CurrentState { get; set; }
    public int? CleaningUserId { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Property? Property { get; set; }
    public Room? Room { get; set; }
    public Guest? Guest { get; set; }
    public InspectionItem? Item { get; set; }
    public User? CleaningUser { get; set; }
}

public enum IncidentType
{
    Repair = 1,
    Replace = 2,
    Clean = 3,
    Missing = 4,
    Other = 5
}

public enum IncidentState
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4
}