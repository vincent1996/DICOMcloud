using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DICOMcloud.Wado.Controllers
{
    public class SwaggerController : Controller
    {
        // GET: Swagger
        public ActionResult Index()
        {
            return File ( "~/swagger/index.html", "text/html" ) ;
        }

        public ActionResult Json()
        {
            return File ( "~/swagger/swagger.json", "application/json") ;
        }
    }
}