using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using ImageProcessor.Web.Configuration;
using ImageProcessor.Web.HttpModules;
using ImageProcessor.Web.Services;

namespace ImageProcessor.Web.Episerver
{
    public class EpiserverImageProcessingModule : ImageProcessingModule
    {
        /// <summary>
        /// The base assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = typeof(ImageFactory).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The web assembly version.
        /// </summary>
        private static readonly string WebAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly Regex PathRegex = new Regex(@",,\d+", RegexOptions.Compiled);
        private static readonly Regex QuestionMarkRegex = new Regex(@"\?", RegexOptions.Compiled);

        protected override string GetRequestUrl(HttpRequest request)
        {
            string requestUrl = "";

            var service = ImageProcessorConfiguration.Instance.ImageServices.FirstOrDefault<IImageService>();

            if (service != null && service.Settings.Count == 0)
                //No settings, so it must be local BLOB storage
                requestUrl = request.Url.PathAndQuery;
            else
                //Otherwise its on Azure
                requestUrl = request.Url.AbsoluteUri;

            requestUrl = PathRegex.Replace(requestUrl, string.Empty);
            requestUrl = QuestionMarkRegex.Replace(requestUrl, new SecondOccuranceFinder("&").MatchEvaluator);

            return requestUrl;
        }

        /// <summary>
        /// This will make the browser and server keep the output
        /// in its cache and thereby improve performance.
        /// </summary>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides
        /// references to the intrinsic server objects
        /// </param>
        /// <param name="responseType">The HTTP MIME type to send.</param>
        /// <param name="dependencyPaths">The dependency path for the cache dependency.</param>
        /// <param name="maxDays">The maximum number of days to store the image in the browser cache.</param>
        /// <param name="statusCode">An optional status code to send to the response.</param>
        public static new void SetHeaders(HttpContext context, string responseType, string[] dependencyPaths, int maxDays, HttpStatusCode? statusCode = null)
        {
            HttpResponse response = context.Response;

            if (response.Headers["ImageProcessedBy"] == null)
            {
                response.AddHeader("ImageProcessedBy", $"ImageProcessor/{AssemblyVersion} - ImageProcessor.Web.Episerver/{WebAssemblyVersion}");
            }

            HttpCachePolicy cache = response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.VaryByHeaders["Accept-Encoding"] = true;

            if (!string.IsNullOrWhiteSpace(responseType))
            {
                response.ContentType = responseType;
            }

            if (dependencyPaths != null)
            {
                context.Response.AddFileDependencies(dependencyPaths.ToArray());
                cache.SetLastModifiedFromFileDependencies();
            }

            if (statusCode != null)
            {
                response.StatusCode = (int)statusCode;
            }

            cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(maxDays));
            cache.SetMaxAge(new TimeSpan(maxDays, 0, 0, 0));
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            AddCorsRequestHeaders(context);
        }
    }

    class SecondOccuranceFinder
    {
        public SecondOccuranceFinder(string replaceWith)
        {
            _replaceWith = replaceWith;
            MatchEvaluator = new MatchEvaluator(IsSecondOccurance);
        }

        private readonly string _replaceWith;
        public MatchEvaluator MatchEvaluator { get; }

        private int _matchIndex;

        public string IsSecondOccurance(Match m)
        {
            _matchIndex++;
            if (_matchIndex % 2 == 0)
                return _replaceWith;
            else
                return m.Value;
        }
    }
}