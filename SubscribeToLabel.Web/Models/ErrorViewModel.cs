namespace DotNet.SubscribeToLabel.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class UpdateLabelSubscriptionsRequestModel
    {
        public string? Labels { get; set; }
    }
}
