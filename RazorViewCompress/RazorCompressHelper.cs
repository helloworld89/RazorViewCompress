using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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

        private static string _fileNamePrefix = "CompressedTemp";

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
                        var compressedViewLocation = viewLocation.Insert(viewLocation.LastIndexOf('/') + 1, _fileNamePrefix);
                        var compressedFilePath = HttpContext.Current.Request.MapPath(compressedViewLocation);

                        TryCompress(filePath, compressedFilePath);

                        return _fileNamePrefix + viewName;
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
            Mutex mutex = new Mutex(false, compressedFilePath.Replace(Path.DirectorySeparatorChar, '_'));
            try
            {
                mutex.WaitOne();
                if (NeedCompress(filePath, compressedFilePath))
                {
                    Compress(filePath, compressedFilePath);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
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

            ICompressorFactory compressorFactory = new RazorCompressorFactory();
            BaseCompressor compressor = compressorFactory.CreateCompressor();
            fileContent = compressor.Compress(fileContent);

            WriteCompressedFile(fileContent, compressedFilePath);
        }

        private static void WriteCompressedFile(string fileContent, string compressedFilePath)
        {
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