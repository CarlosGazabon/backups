using Inventio.Models;
using Inventio.Models.DTO.Settings.Hours;

namespace Inventio.Repositories.Settings.HoursSetting
{
    public interface IHoursSettingRepository
    {
        Task<List<Hour>> GetHours();
        Task<DTOHoursRequest> AddHours(DTOHoursRequest request);
        Task<List<Hour>> EditHours(DTOHoursRequest request);
        Task<DeleteHoursDTO> DeleteHours();
    }
}


// [ {
//     id: "",
//     time: "",
//     sort: ""
//  }, ...
// ]

// {
// data: [],
//     status: "",
//     message: ""
// }