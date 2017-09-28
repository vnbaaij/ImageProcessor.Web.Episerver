using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor.Web.HttpModules;
using System.Web;
using System.Text.RegularExpressions;

namespace ImageProcessor.Web.Episerver
{
    public class EpiserverImageProcessingModule : ImageProcessingModule
    {
        private static readonly Regex PathRegex = new Regex(@",,\d+", RegexOptions.Compiled);

        protected override string GetRequestUrl(HttpRequest request)
        {
            var fixedUrl = PathRegex.Replace(request.Url.PathAndQuery, string.Empty);
            return fixedUrl;
        }
    }
}



