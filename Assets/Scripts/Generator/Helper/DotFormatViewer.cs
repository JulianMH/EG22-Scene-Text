using System.Diagnostics;
using System.IO;

public static class DotFormatViewer
{
    const string graphvizDotExecutable = "/usr/local/bin/dot";

    public static string SavePNG(IDotFormatExportable graph, string fileNameImage = null)
    {
        var fileName = "Temp/graph.dot";
        fileNameImage ??= "./Temp/graph.png";
        Directory.CreateDirectory("Temp/");
        File.WriteAllText(fileName, graph.ToDotFormat());

        Process.Start(graphvizDotExecutable, "-Kdot -Tpng " + fileName + " -o " + fileNameImage).WaitForExit();
        return fileNameImage;
    }

    public static string SaveSVG(IDotFormatExportable graph, string fileNameImage = null)
    {
        var fileName = "Temp/graph.dot";
        fileNameImage ??= "./Temp/graph.svg";
        Directory.CreateDirectory("Temp/");
        File.WriteAllText(fileName, graph.ToDotFormat());

        Process.Start(graphvizDotExecutable, "-Kdot -Tsvg " + fileName + " -o " + fileNameImage).WaitForExit();
        return fileNameImage;
    }

    public static void Display(IDotFormatExportable graph)
    {
        var fileName = "Temp/graph.dot";
        var fileNameImage = "Temp/graph.svg";
        Directory.CreateDirectory("Temp/");
        File.WriteAllText(fileName, graph.ToDotFormat());

        Process.Start(graphvizDotExecutable, "-Kdot -Tsvg " + fileName + " -o ./" + fileNameImage).WaitForExit();
        Process.Start(fileNameImage);
    }
}
