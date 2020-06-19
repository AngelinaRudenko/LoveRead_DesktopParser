using Parser.Core;
using Parser.Core.Loveread;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HtmlAgilityPack;
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
            parser_loveread.OnComplited += Parser_loveread_OnComplited;
            parser_loveread.OnNewData += Parser_loveread_OnNewData;
        }

        private void Parser_loveread_OnNewData(object arg1, string[] arg2)
        {
            foreach (string str in arg2)
                text.Add(str);
            progressBar.Value += 1;
        }

        private void Parser_loveread_OnComplited(object obj)
        {
            if (parser_loveread.IsActive)
            {
                CreateDocument();
                MessageBox.Show("Работа завершена!");
            }
            buttonLoad.Text = "Загрузить";          
            pictureBox.Image = null;
            progressBar.Value = 0;
            labelTitle.Text = null;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonLoad.Text == "Загрузить")
                {
                    pictureBox.LoadAsync($@"http://loveread.me/img/photo_books/{numericUpDownId.Value}.jpg");
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
                    progressBar.Maximum = Convert.ToInt32(lastPage);
                    parser_loveread.Settings = new LovereadSettings(1, Convert.ToInt32(lastPage), (int)numericUpDownId.Value);
                    parser_loveread.Start();
                    buttonLoad.Text = "Остановить загрузку";
                }
                else
                {
                    parser_loveread.Stop();
                    buttonLoad.Text = "Загрузить";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.ToString()}. Возможно вы ввели неверный id книги. \nЕсли вы уверены, что id введен верно, обратитесь к разработчику: angel.rudenko.007@m"+"ail.ru", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDocument()
        {
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
                            doc.Add(new iTextSharp.text.Paragraph(str, font));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Во время сохранения произошла ошибка: {ex.ToString()}.\nОбратитесь к разработчику: angel.rudenko.007@m"+"ail.ru", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        doc.Close();
                    }
                }
            }
        }
    }
}
