using System.Text.Json.Serialization;

namespace WebAppBackend.Api.Models;

/// <summary>
/// Represents a single collapsible section of the page.
/// One instance maps to:
///   - one button in the Angular left-hand navigation panel
///   - one Angular Material expansion panel in the central column
/// </summary>
public class ContentSection
{
    /// <summary>
    /// Stable identifier used for deep-linking / scrolling.
    /// Comes from the "id" attribute on the &lt;Section&gt; element in the XML file.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Sequence ordering
    /// </summary>
    public int Order { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Description { get; set; }
	public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Raw (X)HTML markup for the body of the panel, taken verbatim from the
    /// XML file's &lt;Content&gt; element (typically wrapped in CDATA).
    /// </summary>
    public string Html { get; set; } = string.Empty;

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public SectionType Type { get; set; }

    public static SectionType TextToType(string? xmlAttribute)
    {
        return Enum.TryParse<SectionType>(xmlAttribute, true, out var result)
            ? result
            : SectionType.Expansion;
    }
}

public enum SectionType
{
    Divider,
	Expansion,
    Header,
}