using System.Xml.Linq;
using Scanner.Core;

namespace Scanner.Formatters;

public sealed class JunitScanResultFormatter : IScanResultFormatter
{
    public string Name => "junit";
    public string FileExtension => "xml";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var grouped = findings
            .GroupBy(f => f.Plugin ?? "unknown")
            .ToList();

        var testSuites = new XElement("testsuites");

        foreach (var pluginGroup in grouped)
        {
            var pluginName = pluginGroup.Key;

            var suite = new XElement("testsuite",
                new XAttribute("name", pluginName),
                new XAttribute("tests", pluginGroup.Count()),
                new XAttribute("failures", pluginGroup.Count(f => IsFailure(f)))
            );

            foreach (var finding in pluginGroup)
            {
                var testcase = new XElement("testcase",
                    new XAttribute("name", finding.Id ?? finding.Title ?? "unknown"),
                    new XAttribute("classname", pluginName)
                );

                if (IsFailure(finding))
                {
                    testcase.Add(new XElement("failure",
                        new XAttribute("message", finding.Title ?? "Issue detected"),
                        finding.Description ?? ""
                    ));
                }

                suite.Add(testcase);
            }

            testSuites.Add(suite);
        }

        var doc = new XDocument(testSuites);
        return doc.ToString();
    }

    private static bool IsFailure(ScanFinding finding)
    {
        if (finding.Severity is null)
            return false;

        return finding.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)
               || finding.Severity.Equals("Medium", StringComparison.OrdinalIgnoreCase);
    }
}

//- task: PublishTestResults@2
//inputs:
//testResultsFormat: 'JUnit'
//testResultsFiles: 'scan-results.xml'
//failTaskOnFailedTests: false