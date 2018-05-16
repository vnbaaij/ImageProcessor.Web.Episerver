using System.Text.RegularExpressions;
using System.Web;
using ImageProcessor.Web.HttpModules;

namespace ImageProcessor.Web.Episerver
{
    public class EpiserverImageProcessingModule : ImageProcessingModule
    {
        private static readonly Regex PathRegex = new Regex(@",,\d+", RegexOptions.Compiled);
        private static readonly Regex QuestionMarkRegex = new Regex(@"\?", RegexOptions.Compiled);

        protected override string GetRequestUrl(HttpRequest request)
        {
            string requestUrl = request.Url.AbsoluteUri;

            requestUrl = PathRegex.Replace(requestUrl, string.Empty);
            requestUrl = QuestionMarkRegex.Replace(requestUrl, new SecondOccuranceFinder("&").MatchEvaluator);

            return requestUrl;
        }
    }

    class SecondOccuranceFinder
    {
        public SecondOccuranceFinder(string replaceWith)
        {
            _replaceWith = replaceWith;
            MatchEvaluator = new MatchEvaluator(IsSecondOccurance);
        }

        private string _replaceWith;
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