using System.ComponentModel.DataAnnotations;

namespace QMSSystem.Shared.Models;

public sealed class DocumentDecisionRequest
{
    [Required]
    [RegularExpression("^(Approve|Return|Reject)$")]
    public string Decision { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;
}
