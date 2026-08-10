using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.Validation;
public interface IContentValidator
{
	bool TryValidate(IReadOnlyList<ContentSection> sections, out ValidationResult? criticalError);
}