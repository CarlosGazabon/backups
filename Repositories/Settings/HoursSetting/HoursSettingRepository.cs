

using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO.Settings.Hours;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Settings.HoursSetting
{
    public class HoursSettingRepository : IHoursSettingRepository
    {
        private readonly ApplicationDBContext _context;

        public HoursSettingRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Hour>> GetHours()
        {
            if (_context.Hour == null)
            {
                throw new Exception("Failed to get Hours");
            }

            var Hours = await _context.Hour.ToListAsync();

            return Hours;
        }

        public async Task<DTOHoursRequest> AddHours(DTOHoursRequest request)
        {
            var hoursToAdd = new List<Hour>();
            if (TimeSpan.TryParse(request.Time, out TimeSpan initialTime))
            {
                for (int i = 0; i < 24; i++)
                {
                    var newHour = new Hour
                    {
                        TimeTypeID = 1,
                        Time = DateTime.Today.Add(initialTime.Add(TimeSpan.FromHours(i))).ToString("h:mm tt"),
                        Sort = i + 1
                    };
                    hoursToAdd.Add(newHour);
                }
            }
            else
            {
                throw new ArgumentException("The initial Time format is invalid");
            }

            await _context.Hour.AddRangeAsync(hoursToAdd);
            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<List<Hour>> EditHours(DTOHoursRequest request)
        {
            if (TimeSpan.TryParse(request.Time, out TimeSpan initialTime))
            {
                var hours = await _context.Hour.OrderBy(h => h.Sort).ToListAsync();

                for (int i = 0; i < 24; i++)
                {
                    var updatedTime = DateTime.Today.Add(initialTime.Add(TimeSpan.FromHours(i))).ToString("h:mm tt");
                    var hour = hours[i];

                    hour.Time = updatedTime;
                    hour.Sort = i + 1;

                    _context.Hour.Update(hour);
                }

                await _context.SaveChangesAsync();

                var updatedHours = await _context.Hour.OrderBy(h => h.Sort).ToListAsync();

                return updatedHours;
            }
            else
            {
                throw new ArgumentException("The initial Time format is invalid");
            }
        }

        public async Task<DeleteHoursDTO> DeleteHours()
        {
            if (_context.Hour == null)
            {
                throw new Exception("Failed to get Hours");
            }

            await _context.Hour.ExecuteDeleteAsync();

            return new DeleteHoursDTO
            {
                Message = "Hours deleted successfully."
            };
        }
    }
}