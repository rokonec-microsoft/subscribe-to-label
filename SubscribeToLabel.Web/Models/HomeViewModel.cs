namespace DotNet.SubscribeToLabel.Web.Models
{
    public class HomeViewModel
    {
        public HomeViewModel(string user, string labels)
        {
            Labels = labels;
            User = user;
        }

        public string Labels { get; }
        public string User { get; }
    }
}