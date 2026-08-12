namespace WebAppBackend.Api.Models;

/// <summary> Alias class for better readability. ValidationResults are the output of the validation process, and are written to error-state.xml.
/// </summary>
/// <remarks> ContentSections are meant to flexible, today and in the future. That's why there's a validation process.<br/>
/// ValidationResults, on the other hand, are hard coded ad-hoc. To prevent accidental bugs, all fields are required.</remarks>
public class ValidationResult : ContentSection, IEquatable<ValidationResult>
{
#pragma warning disable CS8765 // strings are nullable in baseclass, but required in ValidationResult
	public override required string Id { get; set; }
	public override required int Order { get; set; }
	public override required string Description { get; set; }
	public override required string Title { get; set; }
	public override required string Html { get; set; }
#pragma warning restore CS8765
	
	public bool Equals(ValidationResult? other) => this == other;                               // Required by IEquatable
	public override bool Equals(object? obj) => this == (obj as ValidationResult);              // CS0660
	public override int GetHashCode() => HashCode.Combine(Id, Title, Description, Html, Type);  // CS0661
	public static bool operator !=(ValidationResult? left, ValidationResult? right) => !(left == right);
	public static bool operator ==(ValidationResult? left, ValidationResult? right)
	{
		if (ReferenceEquals(left, right)) return true;
		if (left is null || right is null) return false;
		return left.Id == right.Id
			&& left.Title == right.Title
			&& left.Description == right.Description
			&& left.Html == right.Html
			&& left.Type == right.Type;
	}
}
