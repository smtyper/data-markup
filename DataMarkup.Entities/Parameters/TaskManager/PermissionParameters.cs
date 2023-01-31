using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.TaskManager;

public record PermissionParameters
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public Guid? TaskTypeId { get; set; }
}
