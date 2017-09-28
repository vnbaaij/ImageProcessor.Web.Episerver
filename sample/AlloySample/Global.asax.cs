using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.HttpModules;

namespace AlloySample
{
    public class EPiServerApplication : EPiServer.Global
    {
        private static readonly Regex PathRegex = new Regex(@",,\d+", RegexOptions.Compiled);
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)

        }

        private void ImageProcessingModule_ValidatingRequest(object sender, ValidatingRequestEventArgs args)
        {
            // Nothing to process, return immediately
            if (string.IsNullOrWhiteSpace(args.QueryString))
                return;
            if (!string.IsNullOrWhiteSpace(args.QueryString))
            {
                args.QueryString = PathRegex.Replace(args.QueryString, string.Empty);
            }
        }
    }
}