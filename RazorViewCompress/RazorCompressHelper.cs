using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RazorViewCompress
{
    internal static class RazorCompressHelper
    {
        private static string[] viewLocationFormats = new string[] {
            "~/Views/{1}/{0}.cshtml",
            "~/Views/{1}/{0}.vbhtml",
            "~/Views/Shared/{0}.cshtml",
             "~/Views/Shared/{0}.vbhtml"
        };
        private static string[] areaViewLocationFormats = new string[] {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Views/{1}/{0}.vbhtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.vbhtml"
        };

        public static string GetCompressedViewName(ControllerContext controllerContext, string viewName)
        {
            List<string> viewLocations = GetViewLocations(controllerContext, viewName);
            try
            {
                foreach (string viewLocation in viewLocations)
                {
                    string filePath = controllerContext.HttpContext.Request.MapPath(viewLocation);
                    if (File.Exists(filePath))
                    {
                        var compressedViewLocation = viewLocation.Insert(viewLocation.LastIndexOf('/') + 1, "Temp");
                        var compressedFilePath = HttpContext.Current.Request.MapPath(compressedViewLocation);

                        TryCompress(filePath, compressedFilePath);
                        return "Temp" + viewName;
                    }
                }
            }
            catch (Exception e)
            {
                //Find no exist file match the viewName in viewLocations or has some exception with multiple thread write file
                return viewName;
            }
            return viewName;


        }

        private static List<string> GetViewLocations(ControllerContext controllerContext, string viewName)
        {
            List<string> viewLocations = new List<string>();

            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            Array.ForEach(viewLocationFormats, viewLocationFormat => viewLocations.Add(string.Format(viewLocationFormat, viewName, controllerName)));

            object areaName;
            if (controllerContext.RouteData.Values.TryGetValue("area", out areaName))
            {
                Array.ForEach(areaViewLocationFormats, areaViewLocationFormat => viewLocations.Add(string.Format(areaViewLocationFormat, viewName, controllerName, areaName)));
            }

            return viewLocations;
        }

        private static void TryCompress(string filePath, string compressedFilePath)
        {

            if (NeedCompress(filePath, compressedFilePath))
            {
                //CreateDirectory(compressedFilePath);
                Compress(filePath, compressedFilePath);
            }
        }

        private static bool NeedCompress(string filePath, string outputPath)
        {
            if (!File.Exists(outputPath))
            {
                return true;
            }
            return File.GetLastWriteTimeUtc(filePath) > File.GetLastWriteTimeUtc(outputPath);
        }
        private static void Compress(string filePath, string compressedFilePath)
        {
            var fileContent = File.ReadAllText(filePath);

            fileContent = HanldePre(fileContent, GetCompressedString);
            File.Delete(compressedFilePath);

            using (var fileStream = File.Create(compressedFilePath))
            {
                using (var txtWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    txtWriter.WriteLine(fileContent);
                }
            }
            File.SetAttributes(compressedFilePath, FileAttributes.Hidden);
        }

        private static string HanldePre(string s, Func<string, string> action)
        {
            List<string> pres = new List<string>();
            var preStartIndex = 0;
            var preEndIndex = 0;
            var preStartMark = "<pre>";
            var preEndMark = "</pre>";
            while (s.IndexOf(preStartMark, preEndIndex) > -1)
            {
                preStartIndex = s.IndexOf(preStartMark, preEndIndex);
                preEndIndex = s.IndexOf(preEndMark, preStartIndex) + 6;
                pres.Add(s.Substring(preStartIndex, preEndIndex - preStartIndex));
            }

            s = action(s);

            preStartIndex = 0;
            preEndIndex = 0;
            int commentIndex = 0;
            while (s.IndexOf(preStartMark, preEndIndex) > -1)
            {
                preStartIndex = s.IndexOf(preStartMark, preEndIndex);
                preEndIndex = s.IndexOf(preEndMark, preStartIndex) + 6;
                s = s.Replace(s.Substring(preStartIndex, preEndIndex - preStartIndex), string.Format("\r\n{0}\r\n", pres[commentIndex]));
                commentIndex++;
            }
            return s;
        }
        private static string GetCompressedString(string fileContent)
        {
            Regex ra = new Regex(@"@:.*\r\n");
            var matcha = ra.Match(fileContent);
            if (matcha.Success)
            {
                fileContent = fileContent.Replace(matcha.Value, matcha.Value + "@/r/n@");
            }

            fileContent = Regex.Replace(fileContent, @">\s+<", "><");
            fileContent = Regex.Replace(fileContent, @"\r\n\s{0,}", " ");
            fileContent = Regex.Replace(fileContent, @"(@model[\t ]{1,}\S{1,})\s", "$1\r\n");

            //Regex r = new Regex(@"@model\s{1,}[0-9A-Za-z._]{1,}");
            //var match = r.Match(fileContent);
            //if (match.Success)
            //{
            //    fileContent = fileContent.Replace(match.Value, match.Value + "\r\n");
            //}

            //fileContent = fileContent.Replace("@/r/n@", "\r\n");
            return fileContent;
        }

        private static void CreateDirectory(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
