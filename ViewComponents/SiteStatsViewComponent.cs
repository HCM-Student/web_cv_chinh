using Microsoft.AspNetCore.Mvc;
using WEB_CV.Models.ViewModels;
using WEB_CV.Services;

namespace WEB_CV.ViewComponents
{
    public class SiteStatsViewComponent : ViewComponent
    {
        private readonly ISiteCounter _counter;
        public SiteStatsViewComponent(ISiteCounter counter) => _counter = counter;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new SiteStatsVm(_counter.GetOnline(), await _counter.GetTotalVisitsAsync());
            return View(vm);
        }
    }
}
