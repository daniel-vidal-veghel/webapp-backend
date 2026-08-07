using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.Validation;
public interface IContentValidator
{
	bool TryValidate(List<ContentSection>? sections, out ValidationResult? criticalError);
}