using System.Reflection.PortableExecutable;

namespace PackageTrackerTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }

    //    using iTextSharp.text.pdf;
    //using iTextSharp.text.pdf.parser;

    //PdfReader reader = new PdfReader(@"D:\test pdf\Blood Journal.pdf");
    //    int intPageNum = reader.NumberOfPages;
    //    string[] words;
    //    string line;

    //    for (int i = 1; i <= intPageNum; i++)
    //    {
    //        text = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());

    //        words = text.Split('\n');
    //        for (int j = 0, len = words.Length; j<len; j++)
    //        {
    //            line = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(words[j]));
    //        }
    //    }1Z8070VR9091251958
}