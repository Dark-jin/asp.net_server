
using System.ComponentModel.DataAnnotations;

namespace MemberWebServer.Model
{
    public class MemberDB
    {
        [Key]
        public string email { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? gender { get; set; }
        public string? password { get; set; }
        public string? userAvatar { get; set; }
        public string? accesstoken { get; set; }
        public string? refreshtoken { get; set; }
    }
}
