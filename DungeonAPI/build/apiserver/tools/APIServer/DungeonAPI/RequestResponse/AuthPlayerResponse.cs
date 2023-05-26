using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse
{
    public class AuthPlayerResponse
    {
        [Required]
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
