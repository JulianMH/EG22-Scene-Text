using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

public static class PlotViewer
{
    const string python3Executable = "python3";

    public static string GetPlotCode(IEnumerable<IEnumerable<(double, double)>> curves, string name, IEnumerable<string> curveLegend = null,
        IEnumerable<(string, double)> sections = null)
    {
        var codeBuilder = new StringBuilder();
        codeBuilder.AppendLine("import matplotlib.pyplot as plt");

        int index = 0;
        foreach (var curve in curves)
        {
            var nameX = "x" + index;
            var nameY = "y" + index;
            codeBuilder.AppendLine(nameX + " = [" + string.Join(", ", curve.Select(p => p.Item1.ToString(CultureInfo.InvariantCulture))) + "]");
            codeBuilder.AppendLine(nameY + " = [" + string.Join(", ", curve.Select(p => p.Item2.ToString(CultureInfo.InvariantCulture))) + "]");

            codeBuilder.AppendLine("plt.plot(" + nameX + ", " + nameY + ")");

            ++index;
        }

        if (sections != null && sections.Count() > 0)
        {
            codeBuilder.AppendLine("plt.axvline(0,color='gray',linewidth=0.5, linestyle='--',zorder=0)");
            var sectionStartX = 0.0;
            var allValuesY = curves.SelectMany(q => q.Select(q => q.Item2));
            var textY = allValuesY.Any() ? allValuesY.Max() : 0;
            foreach (var (sectionTitle, sectionEndX) in sections)
            {
                codeBuilder.AppendLine("plt.axvline(" + sectionEndX.ToString(CultureInfo.InvariantCulture) + ",color='gray',linewidth=0.5, linestyle='--',zorder=0,label='bla')");
                var textX = sectionStartX + (sectionEndX - sectionStartX) * 0.5;
                codeBuilder.AppendLine("plt.text(" + textX.ToString(CultureInfo.InvariantCulture) + ", " + textY.ToString(CultureInfo.InvariantCulture) + ", '" + sectionTitle + "',color='gray', ha='center', fontsize='x-small')");
                sectionStartX = sectionEndX;
            }
        }

        codeBuilder.AppendLine("plt.title(\"" + name + "\")");
        if (curveLegend != null)
        {
            codeBuilder.AppendLine("plt.legend([" + string.Join(",", curveLegend.Select(l => "\"" + l + "\"")) + "])");
        }
        codeBuilder.AppendLine("plt.show()");

        return codeBuilder.ToString();
    }

    public static void Plot(IEnumerable<IEnumerable<(double, double)>> curves, string name, IEnumerable<string> curveLegend = null,
        IEnumerable<(string, double)> sections = null)
    {
               var file = "Temp/plot" + name.Replace(" ", "-") + ".py";
        File.WriteAllText(file, GetPlotCode(curves, name, curveLegend, sections));
        Process.Start(python3Executable, file);
    }

    public static string GetHistCode(IEnumerable<double> values, int bins)
    {
        return "import matplotlib.pyplot as plt\n" +
            "x = [" + string.Join(", ", values.Select(p => p.ToString(CultureInfo.InvariantCulture))) + "]\n" +
            "plt.hist(x, " + bins + ")\n" +
            "plt.show()";
    }

    public static void Hist(IEnumerable<double> values, int bins)
    {
        var file = "Temp/hist.py";
        File.WriteAllText(file, GetHistCode(values, bins));
        Process.Start(python3Executable, file);
    }

}
