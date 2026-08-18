using WebAppBackend.Api.Enums;

namespace WebAppBackend.Api.Models
{
	public class ValidationDates
	{
		public Dictionary<ContentType, DateTime?> Dates { get; set; } = new Dictionary<ContentType, DateTime?>();

		public bool IsContentValid(out ContentType? ct)
		{
			if (Dates[ContentType.DutchErrorState].HasValue)
			{
				ct = ContentType.DutchErrorState;
				return false;
			}
			if (Dates[ContentType.EnglishErrorState].HasValue)
			{
				ct = ContentType.EnglishErrorState;
				return false;
			}
			ct = null;
			return true;
		}
		

		// The first 2 elements are languages, the second 2 are error states for the same languages.
		public DateTime? GetValidationDate(ContentType? ct)
			=> ct != null && ct < ContentType.DutchErrorState ? Dates[ct.Value] : null;

		public DateTime? GetErrorDate(ContentType? ct)
			=> ct != null && ct >= ContentType.DutchErrorState ? Dates[ct.Value] : null;
	}
}
