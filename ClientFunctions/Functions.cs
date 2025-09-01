using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientFunctions
{
    // contains functions used by both clients to avoid duplication
    public static class Functions
    {
        public static string GetContentTypeFromPath(string path)
        {
            var ext = (System.IO.Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();

            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";

                case ".txt":
                case ".log":
                case ".csv": return "text/plain";

                default:
                    // Unknown types are rejected by server unless they start with image/ or text/
                    return "application/octet-stream";
            }
        }
    }
}
