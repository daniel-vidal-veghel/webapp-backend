using WebAppBackend.Api.DataAccess;
using WebAppBackend.Api.Enums;
using WebAppBackend.Api.Models;
using WebAppBackend.Api.Validation;

namespace WebAppBackend.Api.Services;

#region Documentation
/// <summary>
/// WorkFlow:
/// <br/>
/// - Content and validation state are tracked per language (Dutch/English) via a <see cref="ValidationDates"/> matrix.
/// <br/>
/// - If either language has a recorded error, that language's error state takes priority and is served regardless
///   of which language was actually requested. If both languages have an error simultaneously, Dutch
///   is reported first.
/// <br/>
/// - If the content file has been changed after the error state was recorded, revalidate the changes.
/// <br/>
/// - If neither language has a recorded error, the requested language is served directly when its own validation
///   timestamp is newer than its own content file's last-write time. Otherwise, one revalidation attempt is made.
/// <br/>
/// - Revalidation happens at most once per request (depth-limited via the fromWeb flag) to avoid infinite loops;
///   if it still can't resolve a valid state afterward, a generic error section is logged and returned instead.
/// </summary>
#endregion
public class XmlContentService(ILogger<XmlContentService> logger, IDataAccess dataAccess, IContentValidator contentValidator) : IContentService
{
	private readonly ILogger<XmlContentService> _logger = logger;
	private readonly IDataAccess file = dataAccess;
	private readonly IContentValidator validator = contentValidator;

	// from startup, one time only.
	public bool InitValidation(out string? errorMessage)
	{
		byte _numLanguages = 2; // Dutch, English
		// run once per language. Exclude non-language routes.
		for (byte i = 0; i < _numLanguages; i++)
		{
			var sections = file.ReadSiteContent((ContentType)i, out var criticalReadError);
			if (criticalReadError != null)
			{
				errorMessage = $"{(ContentType)i}: " + (criticalReadError.FirstOrDefault()?.Description ?? "InitValidation failed to read");
				return false;
			}

			if (!validator.TryValidate(sections, (ContentType)i, out ValidationResult? criticalError))
			{
				errorMessage = $"{(ContentType)i}: " + (criticalError?.Description ?? "InitValidation failed to validate");
				return false;
			}
		}
		errorMessage = null;
		return true;
	}
	
	public IReadOnlyList<ContentSection> GetSections(bool fromWeb, string? language)
	{
		ValidationDates matrix = file.GetValidationMatrix();

		// Either one of the two is not valid.
		if (!matrix.IsContentValid(out var errorType))
			return HandleErrorState(matrix, errorType,fromWeb, language);

		return HandleHealthyContent(matrix, fromWeb, language);
	}

	private IReadOnlyList<ContentSection> HandleErrorState(ValidationDates matrix, ContentType? errorType, bool fromWeb, string? webLanguage)
	{
		var errorDate = matrix.GetErrorDate(errorType);
		ContentType contentLanguage = errorType == ContentType.DutchErrorState
			? ContentType.DutchSiteContent
			: ContentType.EnglishSiteContent;

		if (errorDate >= file.ContentXmlLastModified(contentLanguage))
			return GetFile(errorType!.Value);

		// avoid loops
		return fromWeb == true
			? RevalidateContent(contentLanguage, webLanguage)
			: ValidationFailure();
	}

	private IReadOnlyList<ContentSection> HandleHealthyContent(ValidationDates matrix, bool fromWeb, string? webLanguage)
	{
		// Hard coded. EN or NL.
		var contentLanguage = webLanguage == "en"
			? ContentType.EnglishSiteContent
			: ContentType.DutchSiteContent;

		if (!file.TouchFile(contentLanguage, out ValidationResult? error))
			return new List<ValidationResult>() { error! };

		if (matrix.GetValidationDate(contentLanguage) >= file.ContentXmlLastModified(contentLanguage)) // Validated after the last time the content was modified = OK!
			return GetFile(contentLanguage);
		// avoid loops
		return fromWeb == true
			? RevalidateContent(contentLanguage, webLanguage)
			: ValidationFailure();
	}

	private IReadOnlyList<ContentSection> RevalidateContent(ContentType contentLanguage, string? webLanguage)
	{
		var sections = file.ReadSiteContent(contentLanguage, out var criticalError);
		if (criticalError != null)
			return criticalError;

		return validator.TryValidate(sections, contentLanguage, out var criticalValidationError)
			? GetSections(false, webLanguage) // depth-limited: never loop more than once
			: new List<ValidationResult>() { criticalValidationError! };
	}

	private IReadOnlyList<ContentSection> GetFile(ContentType ct)
	{
		var output = file.ReadSiteContent(ct, out var criticalError);
		return criticalError != null
			? criticalError
			: output;
	}

	private IReadOnlyList<ContentSection> ValidationFailure()
	{
		// log and return an list with a single error section, so the site can still render something. Do not render unvalidated content.
		_logger.LogError("Could not resolve content validation state even after revalidating.");
		return new List<ValidationResult>
		{
			new ValidationResult
			{
				Id = "error",
				Order = 1,
				Title = "Error",
				Html = "<p>There was an error loading the site content. Please contact the site administrator.</p>",
				Description = "Critical error: revalidation failed."
			}
		};
	}	
}