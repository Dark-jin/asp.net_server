using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace MemberWebServer.Model
{
	public class Registration
	{
		[Required] // 유효성 검사(값이 없으면 400 error)
		public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        [Required]
        public string gender { get; set; }
        [Key] //PK 설정
        [Required(ErrorMessage = "이메일을 입력해주십시오")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        [Required(ErrorMessage = "패스워드를 입력해주십시오")]
        public string password { get; set; }
        public string userAvatar { get; set; }
	}
}
