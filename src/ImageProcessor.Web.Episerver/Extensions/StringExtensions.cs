using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor.Web.Episerver
{
    public static class StringExtensions
    {
        #region Files and Paths
        /// <summary>
        /// Checks the string to see whether the value is a valid virtual path name.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>True if the given string is a valid virtual path name</returns>
        public static bool IsValidVirtualPathName(this string expression)
        {
            Uri uri;

            return Uri.TryCreate(expression, UriKind.Relative, out uri) && uri.IsWellFormedOriginalString();
        }
        #endregion
    }
}
