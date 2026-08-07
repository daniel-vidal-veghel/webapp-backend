using WebAppBackend.Api.Models;
namespace WebAppBackend.Api.DataAccess;
public interface IDataAccess
{
	List<ContentSection> ReadSiteContent();
	List<ContentSection> ReadErrorState();
	bool ErrorStateExists();
	DateTime? ValidationDate();
	DateTime? ContentXmlLastModified();
	bool DeleteErrorState();
	bool DeleteValidationDate();
	bool WriteErrorState(List<ValidationResult> errorState);
	bool WriteValidationDate(DateTime validatedAtUtc);
}