using System.Xml.Linq;
using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.Services;

/// <summary>
/// Reads the site content from a local, uncompiled XML file every time
/// <see cref="GetSections"/> is called. 
/// <br/>
/// Expected XML shape:<br/>
/// &lt;Site&gt;<br/>
/// &lt;Section id="intro" order="1" title="Introduction"&gt;<br/>
/// &lt;Content&gt;&lt;![CDATA[ &lt;p&gt;Some HTML markup...&lt;/p&gt; ]]&gt;&lt;/Content&gt;<br/>
/// &lt;/Section&gt;<br/>
/// ...<br/>
/// &lt;/Site&gt;<br/>
/// </summary>
public class XmlContentService : IContentService
{
	private readonly ILogger<XmlContentService> _logger;
	private readonly string _xmlFilePath;

	public XmlContentService(ILogger<XmlContentService> logger, IWebHostEnvironment env, IConfiguration configuration)
	{
		_logger = logger;

		// The path can be overridden via appsettings.json ("ContentSettings:XmlFilePath"),
		// otherwise it defaults to Content/site-content.xml next to the app.
		var configuredPath = configuration["ContentSettings:XmlFilePath"];

		_xmlFilePath = string.IsNullOrWhiteSpace(configuredPath)
			? Path.Combine(env.ContentRootPath, "Content", "site-content.xml")
			: Path.IsPathRooted(configuredPath)
				? configuredPath
				: Path.Combine(env.ContentRootPath, configuredPath);
	}

	public IReadOnlyList<ContentSection> GetSections()
	{
		if (!File.Exists(_xmlFilePath))
		{
			_logger.LogWarning("Content XML file not found at {Path}", _xmlFilePath);
			return Array.Empty<ContentSection>();
		}

		try
		{
			// FileShare.ReadWrite lets someone edit/save the XML file in another
			// program (e.g. Notepad) on Windows without the read here throwing
			// an IOException because the file is "in use".
			using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var document = XDocument.Load(stream, LoadOptions.None);

			var sections = document.Root?
				.Elements("Section")
				.Select((element, index) => new ContentSection
				{
					Id = (string?)element.Attribute("id") ?? $"section-{index}",
					Title = (string?)element.Attribute("title") ?? $"Section {index + 1}",
					Description = (string?)element.Attribute("description") ?? string.Empty,
					Order = (int?)element.Attribute("order") ?? index,
					Html = (element.Element("Content")?.Value ?? string.Empty).Trim(),
					Type = ContentSection.TextToType((string?)element.Attribute("type"))
				})
				.OrderBy(s => s.Order)
				.ToList();

			return sections ?? new List<ContentSection>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to read/parse content XML file at {Path}", _xmlFilePath);
			return Array.Empty<ContentSection>();
		}
	}
}
