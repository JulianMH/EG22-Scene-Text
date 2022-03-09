using System.Diagnostics;
using System.IO;

namespace GraphGrammar
{
    public static class FileSaver
    {
        public static void SaveAndOpenTempFile(string fileName, string fileContents)
        {
            fileName = "Temp/" + fileName;
            Directory.CreateDirectory("Temp/");
            File.WriteAllText(fileName, fileContents);

            Process.Start(fileName);
        }
    }
}
