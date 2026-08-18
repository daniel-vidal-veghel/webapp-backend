using System.Globalization;
using System.Xml.Linq;
using WebAppBackend.Api.Enums;
using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.DataAccess;

/// <summary>All code for reading/writing the site-content.xml, error-state.xml and validation-dates.xml files. </summary>
/// <remarks>Fun fact: if you rename it to FileAccess, System.Io.FileStream stops working. </remarks>
public class DataAccess (ILogger<DataAccess> logger, IWebHostEnvironment env, IConfiguration configuration) : IDataAccess
{
	private readonly ILogger<DataAccess> _logger = logger;
	private readonly string _siteContentFilePath = ContentPaths.SiteContentFilePath(env, configuration);
	private readonly string _englishContentFilePath = ContentPaths.EnglishContentFilePath(env, configuration);
	private readonly string _validationDatesFilePath = ContentPaths.ValidationDateFilePath(env, configuration);
	private readonly string _errorStateFilePath = ContentPaths.ErrorStateFilePath(env, configuration);

	public bool TouchContentFile(out ValidationResult? error)
	{
		if (!File.Exists(_siteContentFilePath))
		{
			_logger.LogWarning("XML file not found at {Path}", _siteContentFilePath);
			error = new ValidationResult
			{
				Id = "error",
				Order = 1,
				Title = "XML file not found",
				Description = "XML file not found.",
				Html = $"TouchContentFile"
			};
			return false;
		}

		error = null;
		return true;
	}
	
	public List<ContentSection> ReadSiteContent(ContentType ct, out List<ValidationResult>? criticalError)
	{
		switch (ct)
		{
			default:
			case ContentType.DutchSiteContent:
				return ParseSectionsFromFile(_siteContentFilePath, out criticalError);
			case ContentType.EnglishSiteContent:
				return ParseSectionsFromFile(_EnglishContentFilePath, out criticalError);
			case ContentType.ErrorState:
				return ParseSectionsFromFile(_errorStateFilePath, out criticalError);
		}   
	}
	
	public bool ErrorStateExists() => File.Exists(_errorStateFilePath);

	public bool DeleteErrorState()
	{
		try
		{
			if (File.Exists(_errorStateFilePath))
				File.Delete(_errorStateFilePath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete error state file at {Path}", _errorStateFilePath);
		}
		return false;
	}

	public bool DeleteValidationDate(ContentType ct)
	{
		if (!File.Exists(_validationDatesFilePath))
			CreateValidationFile();
		
		try
		{
			XDocument document;
		
			using var stream = new FileStream(_validationDatesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			document = XDocument.Load(stream);

			var root = document.Root!;
			var tag = XmlTagFromContentType(ct);
			var existing = root.Element(tag);
			if (existing != null)
				existing.Value = string.Empty;
			else
				root.Add(new XElement(tag, string.Empty));

			document.Save(_validationDatesFilePath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to clear validation date for {ContentType} in {Path}", ct, _validationDatesFilePath);
		}
		return false;
	}

	/// <summary>
	/// Gets an object with all validation and error dates.
	/// </summary>
	public ValidationDates GetValidationMatrix()
	{
		var result = new ValidationDates();

		if (!File.Exists(_validationDatesFilePath))
			return result;

		try
		{
			using var stream = new FileStream(_validationDatesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var document = XDocument.Load(stream);

			foreach (ContentType ct in Enum.GetValues<ContentType>())
			{
				var raw = document.Root?.Element(XmlTagFromContentType(ct))?.Value;
			if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
					result.Dates[ct] = parsed.ToUniversalTime();

				else if (!string.IsNullOrEmpty(raw))
					_logger.LogWarning("Could not parse validation timestamp value '{Value}' for {ContentType} in {Path}", raw, ct, _validationDatesFilePath);
				else
					result.Dates[ct] = null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to read validation timestamp matrix from {Path}", _validationDatesFilePath);
		}

		return result;
	}

	/// <summary>
	/// Returns the last modified timestamp of the site-content.xml file, or null if the file does not exist.
	/// Has to be UTC because you might be editing the XML file on a local machine with a different time zone than the server.
	/// </summary>
	public DateTime? ContentXmlLastModified()
	{
		if (!File.Exists(_siteContentFilePath))
			return null;

		return File.GetLastWriteTimeUtc(_siteContentFilePath);
	}

	public bool WriteErrorState(List<ValidationResult> errorState)
	{
		try
		{
			var siteElement = new XElement("Site",
				errorState.Select(section => new XElement("Section",
					new XAttribute("id", section.Id ?? string.Empty),
					new XAttribute("title", section.Title ?? string.Empty),
					new XAttribute("type", section.Type.ToString()),
					string.IsNullOrWhiteSpace(section.Description) ? null : new XAttribute("description", section.Description),
					new XElement("Content", new XCData(section.Html ?? string.Empty))
				))
			);

			var document = new XDocument(
				new XDeclaration("1.0", "utf-8", null),
				new XComment(" Auto-generated by ContentValidator when content validation fails. Fix site-content.xml and this file is removed automatically once validation passes again. "),
				siteElement
			);

			document.Save(_errorStateFilePath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to write error state file at {Path}", _errorStateFilePath);
		}
		return false;
	}

	/// <summary>
	/// Writes the validation and error timestamps to the validation-dates.xml file.
	/// Has to be UTC because you don't know where a 3rd party server might be running.
	/// </summary>
	public bool WriteValidationDate(DateTime validatedAtUtc, ContentType ct)
	{
		if (!File.Exists(_validationDatesFilePath))
			CreateValidationFile();

		string tag = XmlTagFromContentType(ct);

		try
		{
			XDocument document;
		
				using var stream = new FileStream(_validationDatesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				document = XDocument.Load(stream);

			var root = document.Root!;
			var value = validatedAtUtc.ToUniversalTime().ToString("o");
			var existing = root.Element(tag);
			if (existing != null)
				existing.Value = value;
			else
				root.Add(new XElement(tag, value));

			document.Save(_validationDatesFilePath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to write validation date file at {Path}", _validationDatesFilePath);
		}
		return false;
	}

	// Use only if file is completely missing.
	private void CreateValidationFile()
	{
		if (File.Exists(_validationDatesFilePath))
			return;

		try
		{
			var root = new XElement("ValidationDates");
			foreach (ContentType ct in Enum.GetValues<ContentType>())
				root.Add(new XElement(XmlTagFromContentType(ct), string.Empty));

			var document = new XDocument(
				new XDeclaration("1.0", "utf-8", null),
				root
			);
			document.Save(_validationDatesFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create validation dates skeleton file at {Path}", _validationDatesFilePath);
		}
	}

	private List<ContentSection> ParseSectionsFromFile(string filePath, out List<ValidationResult>? criticalError)
	{
		// This a failsafe for when reading error-state. Shouldn't really happen.
		if (!File.Exists(filePath))
		{
			_logger.LogWarning("Content XML file not found at {Path}", filePath);
			var error = new ValidationResult()
			{
				Id = "error",
				Order = 1,
				Title = "File not found.",
				Description = "Could not find xml file at expected location.",
				Html = "ParseSectionsFromFile"
			};
			criticalError = new List<ValidationResult> { error };
			return new List<ContentSection>();
		}

		try
		{
			// FileShare.ReadWrite lets someone edit/save the XML file in another
			// program (e.g. Notepad) on Windows without the read here throwing
			// an IOException because the file is "in use".
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var document = XDocument.Load(stream, LoadOptions.None);

			var sections = document.Root?
				.Elements("Section")
				.Select((element, index) => new ContentSection
				{
					Id = (string?)element.Attribute("id"),
					Title = (string?)element.Attribute("title"),
					Description = (string?)element.Attribute("description"),
					Order = index + 1, // Avoid falsy 0-based order; start at 1.
					Html = (element.Element("Content")?.Value ?? string.Empty).Trim(),
					Type = ContentSection.TextToType((string?)element.Attribute("type"))
				})
				.ToList();
			criticalError = null;
			return sections ?? new List<ContentSection>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to read/parse content XML file at {Path}", filePath);
			
			var error = new ValidationResult()
			{
				Id = "error",
				Order = 1,
				Title = "Failed to parse XML file.",
				Description = "Failed to parse XML file.",
				Html = "ParseSectionsFromFile"
			};
			criticalError = new List<ValidationResult> { error };
			return new List<ContentSection>();
		}
	}

	private static string XmlTagFromContentType(ContentType ct)
	{
		switch (ct)
		{
			case ContentType.DutchSiteContent: return "NL_Valid";
			case ContentType.EnglishSiteContent: return "EN_Valid";
			case ContentType.DutchErrorState: return "NL_Error";
			case ContentType.EnglishErrorState: return "EN_Error";
			default: throw new ArgumentOutOfRangeException();
		}
	}
}