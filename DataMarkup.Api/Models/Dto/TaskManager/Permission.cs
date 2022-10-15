using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.TaskManager;

public record Permission
{
    [Required]
    public string UserName { get; init; } = null!;

    [Required]
    public Guid? TaskTypeId { get; init; }
}
