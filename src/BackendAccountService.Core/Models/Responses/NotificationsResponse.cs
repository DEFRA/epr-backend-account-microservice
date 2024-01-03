namespace BackendAccountService.Core.Models.Responses
{
    public class NotificationsResponse
    {
        public List<Notification> Notifications { get; set; }
    }

    public class Notification
    {
        public string Type { get; set; }
        public ICollection<KeyValuePair<string,string>> Data { get; set; }
    }
}
