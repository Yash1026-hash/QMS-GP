using System.ComponentModel.DataAnnotations;

namespace QMSSystem.Shared.Dtos;

public class OperatorChangeRequestDeviation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChangeRequestId { get; set; }

    [Required]
    public int DeviationId { get; set; }
}