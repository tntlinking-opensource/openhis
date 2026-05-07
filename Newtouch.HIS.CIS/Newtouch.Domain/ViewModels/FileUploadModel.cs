using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Newtouch.Domain.ViewModels
{
    public class FileUploadModel
    {
        [Required(ErrorMessage = "请选择要上传的文件")]
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
        public string sqdh { get; set; }
        public string Description { get; set; }
    }
}
