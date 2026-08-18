using WebAppBackend.Api.Models;
using WebAppBackend.Api.Enums;

namespace WebAppBackend.Api.Validation;
public interface IContentValidator
{
	bool TryValidate(IReadOnlyList<ContentSection> sections, ContentType ct, out ValidationResult? criticalError);
}