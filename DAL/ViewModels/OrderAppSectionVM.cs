using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class OrderAppSectionVM
{
    public long SectionId { get; set; }
    public string SectionName { get; set; } = null!;
    public int AvailableCount { get; set; }
    public int AssignedCount { get; set; }
    public int RunningCount { get; set; }
}
