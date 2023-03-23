using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.TaskManager;

public record RemovePermissionParameters
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public Guid? TaskTypeId { get; set; }
}
