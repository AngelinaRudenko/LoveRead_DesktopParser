using Parser.Core;
using Parser.Core.Loveread;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Parser
{
    public partial class MainForm : Form
    {
        ParserWorker<string[]> parser_loveread;
        List<string> text;

        public MainForm()
        {
            InitializeComponent();
            text = new List<string>();

            parser_loveread = new ParserWorker<string[]>(new LovereadParser());
            //По заврешению работы парсера будет появляться уведомляющее окно.
            parser_loveread.OnComplited += Parser_loveread_OnComplited;
            //Заполняем наш listBox заголовками
            parser_loveread.OnNewData += Parser_loveread_OnNewData;
        }

        private void Parser_loveread_OnNewData(object arg1, string[] arg2)
        {
            //listBox1.Items.AddRange(arg2);
            foreach (string str in arg2)
                text.Add(str);
        }

        private void Parser_loveread_OnComplited(object obj)
        {
            MessageBox.Show("Работа завершена!");

            CreateDocument();

        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            pictureBox.Load($@"http://loveread.me/img/photo_books/{numericUpDownId.Value}.jpg");
            //using (WebClient client = new WebClient())
            //{ 
            //скачать всю страницу
            //    string str = client.DownloadString($@"http://loveread.me/view_global.php?id={numericUpDownId.Value}");                 
            //}
            var webGet = new HtmlWeb();
            var document = webGet.Load($@"http://loveread.me/view_global.php?id={numericUpDownId.Value}");
            var title = document.DocumentNode.SelectSingleNode("html/head/title").InnerText;
            string author = title.Substring(title.IndexOf("Автор книги ", 0) + 12);
            string name = title.Substring(13, title.Length - author.Length - 12 - 13);
            labelTitle.Text = $"{author} - {name}";

            document = webGet.Load($@"http://loveread.me/read_book.php?id={numericUpDownId.Value}&p=1");
            var lastPage = document.DocumentNode.SelectSingleNode("html/body/div/div/div/div[2]/div/div/div/div/div[3]/a[10]").InnerText;
            //Настройки для парсера
            parser_loveread.Settings = new LovereadSettings(1, Convert.ToInt32(lastPage), (int)numericUpDownId.Value);
            //Парсим!
            parser_loveread.Start();
        }


        private object lockThread = new object();
        private void CreateDocument()
        {
            Monitor.Enter(lockThread);
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF|*.pdf", ValidateNames = true })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4);
                    try
                    {
                        PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.OpenOrCreate));
                        doc.Open();
                        string ttf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIAL.TTF");
                        var baseFont = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        var font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);
                        
                        foreach (string str in text)
                            doc.Add(new iTextSharp.text.Paragraph(str,font));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        doc.Close();
                    }
                }
            }
            Monitor.Exit(lockThread);
        }
    }
}
