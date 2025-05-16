namespace BillingService.API.Utils
{
    public class DateTimeConverter
    {
        public static string ConvertDateTimeToString(DateTime dateTime)
        {
            // Format the DateTime to the desired string format (without URL encoding)
            string formattedDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // URL encode the colons (:)
            string urlEncodedDateTime = formattedDateTime.Replace(":", "%3A");

            return urlEncodedDateTime;
        }
    }
}
