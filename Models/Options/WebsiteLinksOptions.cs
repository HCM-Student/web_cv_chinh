namespace WEB_CV.Models.Options
{
    public class WebsiteLinkItem
    {
        public string Title { get; set; } = "";
        public string Url   { get; set; } = "";
        public string Image { get; set; } = "";
    }

    public class WebsiteLinksOptions
    {
        public List<WebsiteLinkItem> Items { get; set; } = new();
    }
}
