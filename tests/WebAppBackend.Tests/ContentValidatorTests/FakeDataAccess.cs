using WebAppBackend.Api.DataAccess;
using WebAppBackend.Api.Enums;
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

	// Count, not just a bool: ClearValidation now calls DeleteValidationDate twice
	// per run (once for the content type, once for its matching error type) - a
	// count lets a test assert both actually happened, not just "at least one did".
	public int DeleteValidationDateCallCount { get; private set; }
	public bool DeleteValidationDateWasCalled => DeleteValidationDateCallCount > 0;
	public bool DeleteErrorStateWasCalled { get; private set; }

	public bool WriteValidationDateWasCalled { get; private set; }
	public bool WriteErrorStateWasCalled { get; private set; }

	public DateTime? LastWrittenValidationDate { get; private set; }

	// Tracks which ContentType each WriteValidationDate call actually used - the same
	// method records both "content validated OK" and "content failed, recording the
	// error date" depending on which ContentType TryStoreValidation passes in. A plain
	// WasCalled bool can't distinguish those two cases; this can.
	public List<ContentType> WriteValidationDateCalls { get; } = new();
	public ContentType? LastWrittenValidationDateType => WriteValidationDateCalls.Count > 0 ? WriteValidationDateCalls[^1] : null;

	public List<ValidationResult>? LastWrittenErrorState { get; private set; }

	public bool DeleteErrorState()
	{
		DeleteErrorStateWasCalled = true;
		return DeleteErrorStateSucceeds;
	}

	public bool DeleteValidationDate(ContentType ct)
	{
		DeleteValidationDateCallCount++;
		return DeleteValidationDateSucceeds;
	}

	public bool WriteValidationDate(DateTime validatedAtUtc, ContentType ct)
	{
		WriteValidationDateWasCalled = true;
		LastWrittenValidationDate = validatedAtUtc;
		WriteValidationDateCalls.Add(ct);
		return WriteValidationDateSucceeds;
	}

	public bool WriteErrorState(List<ValidationResult> errorState)
	{
		WriteErrorStateWasCalled = true;
		LastWrittenErrorState = errorState;
		return WriteErrorStateSucceeds;
	}

	// Not called by ContentValidator - stand-ins to satisfy IDataAccess.
	public bool TouchFile(ContentType ct, out ValidationResult? error) { error = null; return true; }
	public List<ContentSection> ReadSiteContent(ContentType ct, out List<ValidationResult>? criticalError) { criticalError = null; return new(); }
	public ValidationDates GetValidationMatrix() => new();
	public bool ErrorStateExists() => false;
	public DateTime? ContentXmlLastModified(ContentType ct) => null;
}
