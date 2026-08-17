using Microsoft.AspNetCore.Mvc;
using WebAppBackend.Api.Models;
using WebAppBackend.Api.Services;

namespace WebAppBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
	private readonly IContentService _contentService;

	public ContentController(IContentService contentService)
	{
		_contentService = contentService;
	}

	/// <summary>
	/// Returns the ordered list of page sections, read live from the
	/// server-side XML file on every request.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(IReadOnlyList<ContentSection>), StatusCodes.Status200OK)]
	public ActionResult<IReadOnlyList<ContentSection>> Get([FromHeader(Name = "X-Lang")] string? lang)
	{
		var sections = _contentService.GetSections(true, lang);
#if DEBUG
		Console.WriteLine($"{DateTime.Now} - fetched: {sections}");
#endif
		return Ok(sections);
	}
}
