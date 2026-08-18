using WebAppBackend.Api.Enums;

namespace WebAppBackend.Api.Models
{
	public class ValidationDates
	{
		public Dictionary<byte, DateTime?> Dates { get; set; } = new Dictionary<byte, DateTime?>();

		public bool IsContentValid(ContentType ct)
			=> ct < ContentType.ErrorState && Dates[(byte)ct].HasValue;
		
		public DateTime? GetValidationDate(ContentType ct)
			=> ct < ContentType.ErrorState
				? Dates[(byte)ct]
				: null;

		public DateTime? GetErrorDate(ContentType ct)
			=> ct < ContentType.ErrorState
				? Dates[(byte)ct]
				: null;
	}
}
