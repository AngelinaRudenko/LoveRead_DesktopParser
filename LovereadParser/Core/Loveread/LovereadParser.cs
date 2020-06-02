using AngleSharp.Browser;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Core.Loveread
{
    class LovereadParser : IParser<string[]>
    {
        public string[] Parse(IHtmlDocument document)
        {
            //Для хранения заголовков
            List<string> list = new List<string>();
            //Здесь мы получаем заголовки
            IEnumerable<IElement> items = document.QuerySelectorAll("p")
                .Where(item => item.ClassName != null && item.ClassName.Contains("MsoNormal"));

            foreach (var item in items)
            {
                //Добавляем заголовки в коллекцию.
                list.Add(item.TextContent);
            }
            return list.ToArray();
        }
    }
}
