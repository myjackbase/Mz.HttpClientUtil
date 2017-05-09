using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mz.HttpClientUtil
{
    public static class UrlHelper
    {
        public static string Combine(params string[] uriParts)
        {
            var uri = string.Empty;
            if (uriParts != null && uriParts.Any())
            {
                uriParts = uriParts.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
                char[] trimChars = { '\\', '/' };
                uri = (uriParts[0] ?? string.Empty).TrimEnd(trimChars);
                for (var i = 1; i < uriParts.Count(); i++)
                {
                    uri = string.Format(
                        "{0}/{1}",
                        uri.TrimEnd(trimChars),
                        (uriParts[i] ?? string.Empty).TrimStart(trimChars));
                }
            }

            return uri;
        }
    }
}
