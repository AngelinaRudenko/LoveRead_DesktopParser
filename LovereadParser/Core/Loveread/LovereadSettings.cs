using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Core.Loveread
{
    class LovereadSettings : IParserSettings
    {
        public string BaseUrl { get; set; } = "http://loveread.me"; //здесь прописываем url сайта.
        public string Postfix { get; set; } = "read_book.php?id={BookId}&p={CurrentId}"; //вместо CurrentID будет подставляться номер страницы
        public int StartPoint { get; set; }
        public int EndPoint { get; set; }

        public LovereadSettings(int start, int end, int bookId)
        {
            StartPoint = start;
            EndPoint = end;
            Postfix = Postfix.Replace("{BookId}", bookId.ToString());
        }
    }
}
