using System.ComponentModel.DataAnnotations;
using Biblio.Models.Entities;

namespace Biblio.Api.DTOs;

public record CategoryDto(int Id, string Name);

public class CreateCategoryDto
{
 [Required]
 [StringLength(100)]
 public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryDto : CreateCategoryDto { }
