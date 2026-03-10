using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectManagementApplication_IntegrationTests.Helpers
{
    public static class AntiForgeryTokenHelper
    {
        public static string ExtractAntiForgeryToken(string html)
        {
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);
            var input = doc.QuerySelector("input[name='__RequestVerificationToken']");
            if (input == null) throw new Exception("No anti-forgery token found");
            var raw = input.GetAttribute("value") ?? throw new Exception("Token value missing");
            return WebUtility.HtmlDecode(raw);
        }
    }
}
