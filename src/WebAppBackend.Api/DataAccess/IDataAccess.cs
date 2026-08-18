using WebAppBackend.Api.Enums;
using WebAppBackend.Api.Models;
namespace WebAppBackend.Api.DataAccess;
public interface IDataAccess
{
	bool ErrorStateExists();
	bool TouchFile(ContentType ct, out ValidationResult? error);
	List<ContentSection> ReadSiteContent(ContentType ct,out List<ValidationResult>? criticalError);
	ValidationDates GetValidationMatrix();
	DateTime? ContentXmlLastModified(ContentType ct);
	bool DeleteErrorState();
	bool DeleteValidationDate(ContentType ct);
	bool WriteErrorState(List<ValidationResult> errorState);
	bool WriteValidationDate(DateTime validatedAtUtc, ContentType ct);
}