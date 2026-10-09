using Microsoft.AspNetCore.Mvc;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.ViewComponents
{
    public class ChangeRequestDetailsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(OperatorChangeRequest dto)
        {
            return View(dto);
        }
    }
}