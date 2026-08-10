namespace WebAppBackend.Api.Models;

/// <summary> Alias class for better readability. ValidationResults are the output of the validation process, and are written to error-state.xml.
/// </summary>
/// <remarks> ContentSections are meant to flexible, today and in the future. That's why there's a validation process.<br/>
/// ValidationResults, on the other hand, are hard coded ad-hoc. To prevent accidental bugs, all fields are required.</remarks>
public class ValidationResult : ContentSection
{
#pragma warning disable CS8765 // strings are nullable in baseclass, but required in ValidationResult
	public override required string Id { get; set; }
	public override required int Order { get; set; }
	public override required string Description { get; set; }
	public override required string Title { get; set; }
	public override required string Html { get; set; }
#pragma warning restore CS8765
}
