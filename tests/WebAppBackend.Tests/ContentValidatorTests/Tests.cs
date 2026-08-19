using WebAppBackend.Api.Enums;
using WebAppBackend.Api.Models;
using WebAppBackend.Api.Validation;
using Xunit;

namespace WebAppBackend.Tests.ContentValidatorTests;

public class Tests
{
	public static readonly TheoryData<IReadOnlyList<ContentSection>, IReadOnlyList<ValidationResult>> _ErrorTestData = TestData.ContentErrorCases();
	public static readonly TheoryData<IReadOnlyList<ContentSection>> _ValidTestData = TestData.ContentValidCases();
	public static readonly TheoryData<bool, bool, ContentSection, ValidationResult> _WriteFailureTestData = TestData.WritingFailureCases();

	[Theory]
	[MemberData(nameof(_ErrorTestData))]
	public void TryValidate_Errors(IReadOnlyList<ContentSection> sections, IReadOnlyList<ValidationResult> expectedErrors)
	{
		var dataAccess = new FakeDataAccess();
		var validator = new ContentValidator(dataAccess);
		bool succeeded = validator.TryValidate(sections, ContentType.DutchSiteContent, out var criticalError);

		Assert.True(succeeded);
		Assert.Null(criticalError);
		Assert.Equal(expectedErrors, dataAccess.LastWrittenErrorState);

		Assert.True(dataAccess.WriteValidationDateWasCalled);
		Assert.Equal(ContentType.DutchErrorState, dataAccess.LastWrittenValidationDateType);
		Assert.Equal(DateTime.UtcNow, dataAccess.LastWrittenValidationDate!.Value, TimeSpan.FromSeconds(2));

		Assert.True(dataAccess.WriteErrorStateWasCalled);
	}

	[Theory]
	[MemberData(nameof(_ValidTestData))]
	public void TryValidate_Valid(IReadOnlyList<ContentSection> sections)
	{
		var dataAccess = new FakeDataAccess();
		var validator = new ContentValidator(dataAccess);
		bool succeeded = validator.TryValidate(sections, ContentType.DutchSiteContent, out var criticalError);

		Assert.True(succeeded);
		Assert.Null(criticalError);
		Assert.Null(dataAccess.LastWrittenErrorState);
		Assert.Equal(DateTime.UtcNow, dataAccess.LastWrittenValidationDate!.Value, TimeSpan.FromSeconds(2));

		// Mirrors the check in TryValidate_Errors: confirms this write used the actual
		// content type, not the error type - the two tests together prove WriteValidationDate
		// consistently records the right ContentType for whichever branch actually ran.
		Assert.Equal(ContentType.DutchSiteContent, dataAccess.LastWrittenValidationDateType);

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
			new List<ContentSection> { TestData.Valid_Setup_Test, TestData.Valid_Default_Section_Test },
			ContentType.DutchSiteContent,
			out var criticalError);

		Assert.False(succeeded);
		Assert.NotNull(criticalError);
		Assert.Null(dataAccess.LastWrittenErrorState);
		Assert.Null(dataAccess.LastWrittenValidationDate);
		Assert.Equal(TestData.ClearValidationFailed_Result, criticalError);

		// Regression test: DeleteValidationDate must still run even though
		// DeleteErrorState failed first. A short-circuiting && here would
		// skip it entirely, silently leaving a stale validation-dates.xml
		// on disk right as a fresh validation was about to run.
		Assert.True(dataAccess.DeleteValidationDateWasCalled);
		Assert.True(dataAccess.DeleteErrorStateWasCalled);
	}

	[Fact]
	public void ClearValidation_DeleteValidationDateRunsForBothTypes_EvenWhenFirstCallFails()
	{
		var dataAccess = new FakeDataAccess { DeleteValidationDateSucceeds = false };
		var validator = new ContentValidator(dataAccess);

		validator.TryValidate(
			new List<ContentSection> { TestData.Valid_Setup_Test, TestData.Valid_Default_Section_Test },
			ContentType.DutchSiteContent,
			out _);

		// Regression test: DeleteValidationDate(ct) and DeleteValidationDate(errorType)
		// must both run, even when the first call fails. A short-circuiting && between
		// them would skip the second call entirely, silently leaving a stale error-type
		// validation date on disk.
		Assert.Equal(2, dataAccess.DeleteValidationDateCallCount);
	}

	[Theory]
	[MemberData(nameof(_WriteFailureTestData))]
	public void TryValidate_TryStoreValidationFails(bool WESS, bool WVDS, ContentSection data, ValidationResult predictedError)
	{
		var dataAccess = new FakeDataAccess { WriteErrorStateSucceeds = WESS, WriteValidationDateSucceeds = WVDS };
		var validator = new ContentValidator(dataAccess);

		bool succeeded = validator.TryValidate(
			new List<ContentSection> { TestData.Valid_Setup_Test, data },
			ContentType.DutchSiteContent,
			out var criticalError);

		// WriteValidationDate is always attempted first, in both the valid-content and
		// invalid-content branches of TryStoreValidation - on failure it's recording the
		// error date, not a "validation succeeded" date. Never skipped in this test.
		Assert.True(dataAccess.WriteValidationDateWasCalled);

		// WriteErrorState is only reached when the content itself was invalid AND
		// WriteValidationDate succeeded first (short-circuiting && between the two).
		// predictedError's own shape already tells us which scenario this row represents.
		bool contentWasInvalid = predictedError == TestData.WriteErrorStateFailed_Result;
		Assert.Equal(contentWasInvalid && WVDS, dataAccess.WriteErrorStateWasCalled);

		Assert.False(succeeded);
		Assert.NotNull(criticalError);
		Assert.Equal(predictedError, criticalError);
	}
}
