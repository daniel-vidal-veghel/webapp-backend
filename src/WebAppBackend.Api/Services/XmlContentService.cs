using WebAppBackend.Api.DataAccess;
using WebAppBackend.Api.Models;
using WebAppBackend.Api.Validation;

namespace WebAppBackend.Api.Services;

#region Documentation
/// <summary>
/// Reads the site content from a local, uncompiled XML file.
/// <br/>
/// Expected XML shape:<br/>
/// &lt;Site&gt;<br/>
/// &lt;Section id="intro" title="Introduction"&gt;<br/>
/// &lt;Content&gt;&lt;![CDATA[ &lt;p&gt;Some HTML markup...&lt;/p&gt; ]]&gt;&lt;/Content&gt;<br/>
/// &lt;/Section&gt;<br/>
/// &lt;/Site&gt;<br/>
/// <br/>
/// WorkFlow:
/// <br/>
/// - If error-state.xml exists, prior validation failed. Return error-state instead.
/// <br/>
/// - Else, if validation-date's timestamp is newer than
///   site-content's last-write time, that file hasn't been modified after validation. Return that. This is the standard behavior.
/// <br/>
/// - Otherwise, validation is missing or stale. Validate and retry. 
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
		var sections = file.ReadSiteContent();

		if (validator.TryValidate(sections, out ValidationResult? criticalError))
		{
			errorMessage = null;
			return true;
		}
		else
		{
			errorMessage = criticalError?.Html ?? "Unknown error: XmlContentService.InitValidation()";
			return false;
		}
	}
	
	public IReadOnlyList<ContentSection> GetSections(bool fromWeb)
	{
		if (file.ErrorStateExists())
			return file.ReadErrorState();

		// If the file somehow goes missing, while there is a validation-date present, then the next comparison is falsely true.
		if (!file.TouchContentFile(out ValidationResult? error)) 
			return new List<ValidationResult>() { error!};

		if (file.ValidationDate() >= file.ContentXmlLastModified()) // Validated after the last time the content was modified = OK!
			return file.ReadSiteContent();

		if (fromWeb == true) // not relooped.
		{
			var sections = file.ReadSiteContent();
			return validator.TryValidate(sections, out var criticalError)
				? GetSections(false) // depth-limited: never loop more than once
				: new List<ValidationResult>() { criticalError! };
		}

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