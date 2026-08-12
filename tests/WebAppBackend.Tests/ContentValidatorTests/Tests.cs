using WebAppBackend.Api.Models;
using WebAppBackend.Api.Validation;
using Xunit;

namespace WebAppBackend.Tests.ContentValidatorTests;

public class Tests
{
	// 2 sets of data: one for bad XML sections, one for correct XML serctions, one for IO problems.
	public static readonly TheoryData<IReadOnlyList<ContentSection>, IReadOnlyList<ValidationResult>> _ErrorTestData = TestData.ContentErrorCases();
	public static readonly TheoryData<IReadOnlyList<ContentSection>> _ValidTestData = TestData.ContentValidCases();
	public static readonly TheoryData<bool, bool, ContentSection, ValidationResult> _WriteFailureTestData = TestData.WritingFailureCases();

	[Theory]
	[MemberData(nameof(_ErrorTestData))]
	public void TryValidate_Errors(IReadOnlyList<ContentSection> sections, IReadOnlyList<ValidationResult> expectedErrors)
	{
		var dataAccess = new FakeDataAccess();
		var validator = new ContentValidator(dataAccess);
		bool succeeded = validator.TryValidate(sections, out var criticalError);

		Assert.True(succeeded);
		Assert.Null(criticalError);
		Assert.Equal(expectedErrors, dataAccess.LastWrittenErrorState);
		Assert.Null(dataAccess.LastWrittenValidationDate);

		Assert.True(dataAccess.WriteErrorStateWasCalled);
		Assert.False(dataAccess.WriteValidationDateWasCalled);
	}

	[Theory]
	[MemberData(nameof(_ValidTestData))]
	public void TryValidate_Valid(IReadOnlyList<ContentSection> sections)
	{
		var dataAccess = new FakeDataAccess();
		var validator = new ContentValidator(dataAccess);
		bool succeeded = validator.TryValidate(sections, out var criticalError);

		Assert.True(succeeded);
		Assert.Null(criticalError);
		Assert.Null(dataAccess.LastWrittenErrorState);
		Assert.Equal(DateTime.UtcNow, dataAccess.LastWrittenValidationDate!.Value, TimeSpan.FromSeconds(2));

		Assert.False(dataAccess.WriteErrorStateWasCalled);
		Assert.True(dataAccess.WriteValidationDateWasCalled);
	}

	// -------------------------------------------------------------------
	// Disk-level failure paths. 
	// In both cases below, getting criticalError is the most important part.
	// -------------------------------------------------------------------

	[InlineData(false, true)]
	[InlineData(true, false)]
	[Theory]
	public void TryValidate_ClearValidationFails(bool DESS, bool DVDS)
	{
		var dataAccess = new FakeDataAccess { DeleteErrorStateSucceeds = DESS, DeleteValidationDateSucceeds = DVDS };
		var validator = new ContentValidator(dataAccess);

		bool succeeded = validator.TryValidate(
			new List<ContentSection> { TestData.Valid_Default_Section_Test },
			out var criticalError);

		Assert.False(succeeded);
		Assert.NotNull(criticalError);
		Assert.Null(dataAccess.LastWrittenErrorState);
		Assert.Null(dataAccess.LastWrittenValidationDate);
		Assert.Equal(TestData.ClearValidationFailed_Result, criticalError);

		// Regression test: DeleteValidationDate must still run even though
		// DeleteErrorState failed first. A short-circuiting && here would
		// skip it entirely, silently leaving a stale validation-date.xml
		// on disk right as a fresh validation was about to run.
		Assert.True(dataAccess.DeleteValidationDateWasCalled);
		Assert.True(dataAccess.DeleteErrorStateWasCalled);
	}

	[Theory]
	[MemberData(nameof(_WriteFailureTestData))]
	public void TryValidate_WriteErrorStateFails(bool WESS, bool WVDS, ContentSection data, ValidationResult predictedError)
	{
		var dataAccess = new FakeDataAccess { WriteErrorStateSucceeds = WESS, WriteValidationDateSucceeds = WVDS };
		var validator = new ContentValidator(dataAccess);

		bool succeeded = validator.TryValidate(
			new List<ContentSection> { data },
			out var criticalError);

		Assert.Equal(!WESS, dataAccess.WriteErrorStateWasCalled);
		Assert.Equal(!WVDS, dataAccess.WriteValidationDateWasCalled);

		Assert.False(succeeded);
		Assert.NotNull(criticalError);
		Assert.Equal(predictedError, criticalError); 
	}
}