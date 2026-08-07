using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.Services;

public interface IContentService
{
	IReadOnlyList<ContentSection> GetSections(bool fromWeb); 
	bool InitValidation(out string? errorMessage); 
}