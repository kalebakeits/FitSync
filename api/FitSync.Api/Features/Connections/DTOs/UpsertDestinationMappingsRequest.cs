namespace FitSync.Api.Features.Connections.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpsertDestinationMappingsRequest : IValidatableObject
{
    [Required]
    public required string SourceServiceType { get; set; }

    [Required]
    public required List<string> DestinationServiceTypes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DestinationServiceTypes.Contains(SourceServiceType))
        {
            yield return new ValidationResult(
                "Source and destination service types must be different.",
                [nameof(DestinationServiceTypes)]
            );
        }
    }
}
