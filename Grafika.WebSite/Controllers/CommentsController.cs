using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grafika.WebSite.ViewModels;

namespace Grafika.WebSite.Controllers
{
    [Route("comments")]
    public class CommentsController : Controller
    {
        [Route(""), AllowAnonymous]
        public IActionResult Index(CommentDisqusViewModel model)
        {
            return PartialView(model.TemplateName, model);
        }
    }
}