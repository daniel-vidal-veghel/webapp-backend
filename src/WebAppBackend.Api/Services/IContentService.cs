using WebAppBackend.Api.Models;

namespace WebAppBackend.Api.Services;

public interface IContentService
{
    /// <summary>
    /// Loads the page sections. Implementations are expected to read the
    /// backing XML file fresh every time this is called (no caching),
    /// so that edits to the file on the server are reflected on next browse
    /// without an app restart.
    /// </summary>
    IReadOnlyList<ContentSection> GetSections();
}
