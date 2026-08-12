using System.Runtime.ExceptionServices;
using WebAppBackend.Api.Models;
using Xunit;

namespace WebAppBackend.Tests.ContentValidatorTests;

/// <summary>
/// Hardcoded ContentSection inputs.<br/>
/// ContentSection_XYZ_Test matches ValidationResult_XYZ_Result. <br/>
/// </summary>
public static class TestData
{
	/// <summary> One row per logical path through ContentValidator.
	/// 3 sets of data: <br/>
	/// * One for bad sections and expected validation results.<br/>
	/// * One for valid sections and saving a date.<br/>
	/// * On for failing IO operations.<br/>
	/// </summary>
	public static TheoryData<IReadOnlyList<ContentSection>, IReadOnlyList<ValidationResult>> ContentErrorCases()
	{
		var data = new TheoryData<IReadOnlyList<ContentSection>, IReadOnlyList<ValidationResult>>();

		data.Add(
			new List<ContentSection> { Null_Id_Test },
			new List<ValidationResult> { Null_Id_Result });

		data.Add(
			new List<ContentSection> { Empty_Id_Test },
			new List<ValidationResult> { Empty_Id_Result });

		data.Add(
			new List<ContentSection> { Null_Title_Test },
			new List<ValidationResult> { Null_Title_Result });

		data.Add(
			new List<ContentSection> { Empty_Title_Test },
			new List<ValidationResult> { Empty_Title_Result });

		data.Add(
			new List<ContentSection> { Duplicate_Id_Test_A, Duplicate_Id_Test_B },
			new List<ValidationResult> { Duplicate_Id_Result });

		data.Add(
			new List<ContentSection> { Duplicate_Title_Test_A, Duplicate_Title_Test_B },
			new List<ValidationResult> { Duplicate_Title_Result });

		data.Add(
			new List<ContentSection> { Divider_With_Id_Test },
			new List<ValidationResult> { Divider_With_Id_Result });

		data.Add(
			new List<ContentSection>(),
			new List<ValidationResult> { No_Sections_Result });
		data.Add(
			null!,
			new List<ValidationResult> { No_Sections_Result });

		return data;
	}
	public static TheoryData<IReadOnlyList<ContentSection>> ContentValidCases()
	{
		var data = new TheoryData<IReadOnlyList<ContentSection>>();
		data.Add( new List<ContentSection> { Valid_Divider_test });
		data.Add( new List<ContentSection> { Valid_Default_Section_Test });
		data.Add( new List<ContentSection> { Valid_Header_Section_Test});
		data.Add( new List<ContentSection> { Valid_Expansion_Section_Test });
		return data;
	}

	public static TheoryData<bool, bool, ContentSection, ValidationResult> WritingFailureCases()
	{
		// First bool will set the writing of error state to fail. The Second one will set the writing of a validation date to fail. 
		// The ContentSection needs to trigger the normal behaviour you're testing against. IE: a bad section for failure, a good section for validation date.
		return new()
		{
			{ false, true, TestData.Null_Id_Test, TestData.WriteErrorStateFailed_Result}, // WriteError is set to fail.
			{ true, false, TestData.Valid_Default_Section_Test, TestData.WriteValidationDateFailed_Result}, // WriteValidationDate is set to fail.
		};
	}

	// ---------------------------------------------------------------
	// Inputs
	// ---------------------------------------------------------------

	// Bad sections
	public static readonly ContentSection Null_Id_Test = new()
	{
		Id = null,
		Title = "Introduction",
		Order = 1,
		Html = "<p>Body</p>"
	};

	public static readonly ContentSection Empty_Id_Test = new()
	{
		Id = "",
		Title = "Introduction",
		Order = 1,
		Html = "<p>Body</p>"
	};

	public static readonly ContentSection Null_Title_Test = new()
	{
		Id = "intro",
		Title = null,
		Order = 1,
		Html = "<p>Body</p>"
	};

	public static readonly ContentSection Empty_Title_Test = new()
	{
		Id = "intro",
		Title = "",
		Order = 1,
		Html = "<p>Body</p>"
	};

	public static readonly ContentSection Duplicate_Id_Test_A = new()
	{
		Id = "dup",
		Title = "First Title",
		Order = 1,
		Html = "<p>A</p>"
	};

	public static readonly ContentSection Duplicate_Id_Test_B = new()
	{
		Id = "dup",
		Title = "Second Title",
		Order = 2,
		Html = "<p>B</p>"
	};

	public static readonly ContentSection Duplicate_Title_Test_A = new()
	{
		Id = "first",
		Title = "Same Title",
		Order = 1,
		Html = "<p>A</p>"
	};

	public static readonly ContentSection Duplicate_Title_Test_B = new()
	{
		Id = "second",
		Title = "Same Title",
		Order = 2,
		Html = "<p>B</p>"
	};

	public static readonly ContentSection Divider_With_Id_Test = new()
	{
		Id = "should-not-have-id",
		Title = null,
		Order = 1,
		Type = ContentSection.SectionType.Divider,
		Html = ""
	};

	// Valid sections = no matching ValidationResult.
	public static readonly ContentSection Valid_Divider_test = new()
	{
		Id = null,
		Title = null,
		Order = 1,
		Type = ContentSection.SectionType.Divider,
		Html = ""
	};

	public static readonly ContentSection Valid_Default_Section_Test = new()
	{
		Id = "valid-id",
		Title = "Valid Title",
		Order = 1,
		Description = "Valid Description",
		Html = "<p>Body</p>"
	};

	public static readonly ContentSection Valid_Header_Section_Test = new()
	{
		Id = "valid-id",
		Title = "Header Title",
		Order = 1,
		Description = "Header Description",
		Type = ContentSection.SectionType.Header,
	};

	// Explicitly typed expansion
	public static readonly ContentSection Valid_Expansion_Section_Test = new()
	{
		Id = "valid-id",
		Title = "Valid Title",
		Order = 1,
		Html = "<p>Body</p>",
		Description = "Valid Description",
		Type = ContentSection.SectionType.Expansion
	};

	// ---------------------------------------------------------------
	// Expected outputs
	// ---------------------------------------------------------------

	public static readonly ValidationResult Null_Id_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 1 missing Id",
		Description = "Section number 1, Title: 'Introduction': is missing the Id attribute.",
		Html = "Section number 1, Title: 'Introduction'"
	};

	public static readonly ValidationResult Empty_Id_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 1 empty Id",
		Description = "Section number 1, Title: 'Introduction': has an empty Id attribute.",
		Html = "Section number 1, Title: 'Introduction'"
	};

	public static readonly ValidationResult Null_Title_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 1 missing Title",
		Description = "Section number 1, Id: 'intro': is missing the Title attribute.",
		Html = "Section number 1, Id: 'intro'"
	};

	public static readonly ValidationResult Empty_Title_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 1 empty Title",
		Description = "Section number 1, Id: 'intro': has an empty Title attribute.",
		Html = "Section number 1, Id: 'intro'"
	};

	public static readonly ValidationResult Duplicate_Id_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 2 duplicate Id",
		Description = "Section number 2, Title: 'Second Title': has a duplicate Id attribute: 'dup'",
		Html = "Section number 2, Title: 'Second Title'"
	};

	public static readonly ValidationResult Duplicate_Title_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 2 duplicate Title",
		Description = "Section number 2, Id: 'second': has a duplicate Title attribute: 'Same Title'",
		Html = "Section number 2, Id: 'second'"
	};

	public static readonly ValidationResult Divider_With_Id_Result = new()
	{
		Id = "error_1",
		Order = 1,
		Title = "Section nr 1 Divider has Id!",
		Description = "Section number 1 is a Divider but has an Id attribute: 'should-not-have-id'",
		Html = "Section number 1 is a Divider but has an Id attribute: 'should-not-have-id'"
	};

	public static readonly ValidationResult No_Sections_Result = new()
	{
		Id = "error",
		Order = 1,
		Title = "No sections found",
		Description = "The content has no sections defined.",
		Html = "The content has no sections defined."
	};

	// Critical (disk-level) errors, fixed text, not derived from input sections.

	public static readonly ValidationResult ClearValidationFailed_Result = new()
	{
		Id = "error",
		Order = 1,
		Title = "Validation state / date could not be cleared",
		Description = "Either validation-state or validation-date could not be cleared. This may indicate a file system permission issue.",
		Html = "The validation state could not be cleared. This may indicate a file system permission issue. Check log."
	};

	public static readonly ValidationResult WriteErrorStateFailed_Result = new()
	{
		Id = "error",
		Order = 1,
		Title = "Couldn't write error-state.xml",
		Description = "Failed to write error-state.xml to disk.",
		Html = "Failed to write error-state to disk.<br> Check Log. Check directory for permissions / conflicts. Check File for illegal characters."
	};

	public static readonly ValidationResult WriteValidationDateFailed_Result = new()
	{
		Id = "error",
		Order = 1,
		Title = "Couldn't write validation-date.xml",
		Description = "Failed to write validation-date.xml to disk.",
		Html = "Failed to write validation-date to disk.<br> Check Log. Check directory for permissions / conflicts. Check File for illegal characters."
	};
}
