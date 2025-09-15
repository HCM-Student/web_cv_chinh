using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WEB_CV.Models.Options;

namespace WEB_CV.ViewComponents
{
    public class WebsiteLinksViewComponent : ViewComponent
    {
        private readonly WebsiteLinksOptions _opt;
        public WebsiteLinksViewComponent(IOptionsSnapshot<WebsiteLinksOptions> opt) => _opt = opt.Value;

        public IViewComponentResult Invoke()
        {
            // lọc item thiếu URL/Title cho chắc
            var items = _opt.Items.Where(x => !string.IsNullOrWhiteSpace(x.Url) &&
                                              !string.IsNullOrWhiteSpace(x.Title)).ToList();
            return View(items);
        }
    }
}
