using WebAppBackend.Api.Enums;

namespace WebAppBackend.Api.Models
{
	public class ValidationDates
	{
		public Dictionary<ContentType, DateTime?> Dates { get; set; } = new Dictionary<ContentType, DateTime?>();

		public bool IsContentValid(ContentType ct)
			=> ct < ContentType.DutchErrorState && Dates[ct].HasValue;
		
		public DateTime? GetValidationDate(ContentType ct)
			=> ct < ContentType.DutchErrorState ? Dates[ct] : null;

		public DateTime? GetErrorDate(ContentType ct)
			=> ct < ContentType.DutchErrorState ? null : Dates[ct];
	}
}
