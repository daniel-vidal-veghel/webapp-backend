using WebAppBackend.Api.DataAccess;
using WebAppBackend.Api.Models;

namespace WebAppBackend.Tests.ContentValidatorTests;

/// <summary>
/// DataAccess dummy. The 'Succeeds' properties determine success / failure results. Use these to control flow.
/// The 'WasCalled' members are for diagnostics. Use them to check the thread has been everywhere it should have been.
/// </summary>
public class FakeDataAccess : IDataAccess
{
	public bool DeleteErrorStateSucceeds { get; set; } = true;
	public bool DeleteValidationDateSucceeds { get; set; } = true;
	public bool WriteValidationDateSucceeds { get; set; } = true;
	public bool WriteErrorStateSucceeds { get; set; } = true;

	public bool DeleteErrorStateWasCalled { get; private set; }
	public bool DeleteValidationDateWasCalled { get; private set; }
	public bool WriteValidationDateWasCalled { get; private set; }
	public bool WriteErrorStateWasCalled { get; private set; }

	public DateTime? LastWrittenValidationDate { get; private set; }
	public List<ValidationResult>? LastWrittenErrorState { get; private set; }

	public bool DeleteErrorState()
	{
		DeleteErrorStateWasCalled = true;
		return DeleteErrorStateSucceeds;
	}

	public bool DeleteValidationDate()
	{
		DeleteValidationDateWasCalled = true;
		return DeleteValidationDateSucceeds;
	}

	public bool WriteValidationDate(DateTime validatedAtUtc)
	{
		WriteValidationDateWasCalled = true;
		LastWrittenValidationDate = validatedAtUtc;
		return WriteValidationDateSucceeds;
	}

	public bool WriteErrorState(List<ValidationResult> errorState)
	{
		WriteErrorStateWasCalled = true;
		LastWrittenErrorState = errorState;
		return WriteErrorStateSucceeds;
	}

	// Not called by ContentValidator - stand-ins to satisfy IDataAccess.
	public bool TouchContentFile(out ValidationResult? error) { error = null; return true; }
	public List<ContentSection> ReadSiteContent(out List<ValidationResult>? criticalError) { criticalError = null; return new(); }
	public List<ContentSection> ReadErrorState(out List<ValidationResult>? criticalError) { criticalError = null; return new(); }
	public bool ErrorStateExists() => false;
	public DateTime? ValidationDate() => null;
	public DateTime? ContentXmlLastModified() => null;
}
