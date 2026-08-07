namespace WebAppBackend.Api.DataAccess;

/// <summary>
/// Resolves the on-disk paths for the XML files.
/// </summary>
public static class ContentPaths
{
	private const string _prefix = "ContentSettings:";
	private const string _dirKey = "ContentFolderPath";
	private const string _contentKey = "ContentFileName";
	private const string _validationKey = "ValidationFileName";
	private const string _errorKey = "ErrorStateFileName";
	private const string _dirDefault = "Content";
	private const string _contentDefault = "site-content.xml";
	private const string _errorDefault = "error-state.xml";
	private const string _validationDefault = "validation-date.xml";

	public static string SiteContentFilePath(IWebHostEnvironment env, IConfiguration configuration)
		=> Path.Combine(ContentFolderPath(env, configuration), configuration[_prefix + _contentKey] ?? _contentDefault);

	public static string ValidationDateFilePath(IWebHostEnvironment env, IConfiguration configuration)
		=> Path.Combine(ContentFolderPath(env, configuration), configuration[_prefix + _validationKey] ?? _validationDefault);

	public static string ErrorStateFilePath(IWebHostEnvironment env, IConfiguration configuration)
		=> Path.Combine(ContentFolderPath(env, configuration), configuration[_prefix + _errorKey] ?? _errorDefault);

	private static string ContentFolderPath(IWebHostEnvironment env, IConfiguration configuration)
		=> Path.Combine(env.ContentRootPath, configuration[_prefix + _dirKey] ?? _dirDefault );

	public static void ValidatePathSettings(IConfiguration configuration)
	{
		bool pathsOK =	!string.IsNullOrWhiteSpace(configuration[_prefix + _dirKey])&&
						!string.IsNullOrWhiteSpace(configuration[_prefix + _contentKey]) &&
						!string.IsNullOrWhiteSpace(configuration[_prefix + _validationKey]) &&
						!string.IsNullOrWhiteSpace(configuration[_prefix + _errorKey]);
		if (!pathsOK)
		{
			throw new Exception("ContentSettings in appsettings.json is missing or incorrectly setup.");
		}
	}
}