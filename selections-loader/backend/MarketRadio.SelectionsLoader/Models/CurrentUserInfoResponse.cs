namespace MarketRadio.SelectionsLoader.Models
{
    public class CurrentUserInfoResponse
    {
        public CurrentUserInfoDto CurrentUserInfo { get; set; }
    }

    public class CurrentUserInfoDto
    {
        public string Email { get; set; }
    }
}