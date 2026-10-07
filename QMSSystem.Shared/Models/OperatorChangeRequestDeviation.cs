using System.ComponentModel.DataAnnotations;

namespace QMSSystem.Shared.Models;

public class OperatorChangeRequestDeviation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChangeRequestId { get; set; }

    [Required]
    public int DeviationId { get; set; }
}