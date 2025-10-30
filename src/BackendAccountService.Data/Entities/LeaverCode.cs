using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities
{
    public class LeaverCode
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? ReasonsForLeaving { get; set; }

        [MaxLength(1)]
        public string? Key { get; set; }
    }
}