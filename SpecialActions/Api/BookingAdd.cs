using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json; 

namespace skill_composer.SpecialActions
{
    public class BookingAdd : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var bookingString = RemoveFirstAndLastLines(task.Input);

            BookingRequest request = JsonConvert.DeserializeObject<BookingRequest>(bookingString);
  
            var bookingResult = await AddBooking(request);

            task.Output = bookingResult;

            return task;
        }
        private static string RemoveFirstAndLastLines(string input)
        {
            var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();

            filteredLines = filteredLines.Where(line => !line.TrimStart().StartsWith("--")).ToArray();

            return string.Join("\n", filteredLines);
        }

        private async Task<string> AddBooking(BookingRequest request)
        {
            try
            {
                request.BookingDate = DateTime.Now;
                request.StatusId = 1; 
 
                var databaseService = new DatabaseService();

                var parameters = new Dictionary<string, string>
                {
                    { "p_BusinessId", request.BusinessId.ToString() },
                    { "p_CustomerId", request.CustomerId.ToString() },
                    { "p_ResourceId", request.ResourceId.ToString() },
                    { "p_EmployeeId", request.EmployeeId.ToString() },
                    { "p_ServiceId", request.ServiceId.ToString() },
                    { "p_BookingDate", request.BookingDate.ToString("yyyy-MM-dd") },
                    { "p_StartDateTime", request.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "p_EndDateTime", request.EndDateTime?.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "p_StatusId", request.StatusId.ToString() },
                    { "p_StatusInformation", request.StatusInformation },
                    { "p_CustomerEmail", request.CustomerEmail },
                    { "p_CustomerFirstName", request.CustomerFirstName },
                    { "p_CustomerMiddleName", request.CustomerMiddleName },
                    { "p_CustomerLastName", request.CustomerLastName }
                };

                var dataSet = await databaseService.Execute("secretary.Booking_AddBooking", parameters);
                
                if (dataSet.Tables[0].Columns.Contains("BookingId")) // booking has been added
                {
                    request.BookingId = databaseService.GetInteger(dataSet.Tables[0].Rows[0], "BookingId");
                    return JsonConvert.SerializeObject(request);
                }
                else // suggest some other booking slots that are available
                {
                    var freeSlots = PropertyMapper.ConvertTo<BookingResponse>(dataSet.Tables[0].Rows);
                    return $"Booking Slot Unavailable, here are some slots that are available: {JsonConvert.SerializeObject(freeSlots)}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"BookingAdd.AddBooking error adding a booking: {e.Message}");
                return string.Empty;
            }
        }
    }
}
