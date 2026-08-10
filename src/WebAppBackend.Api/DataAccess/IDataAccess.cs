using WebAppBackend.Api.Models;
namespace WebAppBackend.Api.DataAccess;
public interface IDataAccess
{
	bool TouchContentFile(out ValidationResult? error);
	List<ContentSection> ReadSiteContent(out List<ValidationResult>? criticalError);
	List<ContentSection> ReadErrorState(out List<ValidationResult>? criticalError);
	bool ErrorStateExists();
	DateTime? ValidationDate();
	DateTime? ContentXmlLastModified();
	bool DeleteErrorState();
	bool DeleteValidationDate();
	bool WriteErrorState(List<ValidationResult> errorState);
	bool WriteValidationDate(DateTime validatedAtUtc);
}