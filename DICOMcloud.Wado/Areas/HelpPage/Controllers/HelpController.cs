using System;
using System.Web.Http;
using System.Web.Mvc;

namespace DICOMcloud.Wado.Areas.HelpPage.Controllers
{
    public class HelpController : Controller
    {
        public HelpController()
        {
        }

        public ActionResult Index()
        {
            return Redirect ("/") ;
        }
    }
}