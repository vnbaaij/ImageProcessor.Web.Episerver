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
        private static readonly Regex QuestionMarkRegex = new Regex(@"\?", RegexOptions.Compiled);

        protected override string GetRequestUrl(HttpRequest request)
        {
            var fixedUrl = PathRegex.Replace(request.Url.PathAndQuery, string.Empty);

            fixedUrl = QuestionMarkRegex.Replace(fixedUrl, new SecondOccuranceFinder("&").MatchEvaluator);
            return fixedUrl;
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
        public MatchEvaluator MatchEvaluator
        {
            get; }

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



