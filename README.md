# Platform Scan

Tooling to run on scans to detect drift or other changes that might be useful to be aware of. 

Essentially the idea is you'd run the scanner from a scheduler, such as Azure Devops pipeline or AKS cronjob etc. Azure devops is potentially better for running on a daily basis and tracking changes over time as it has built in storage of previous runs, but ultimately this is down to the user of the tools to decide.


## Scan Plugins

* [TLS and Certificate Plugin](Scanner/Scanner.Plugins.Tls/README.md)
* [HTTP Header Plugin](Scanner/Scanner.Plugins.HttpHeader/README.md)
* [CVE Plugin](Scanner/Scanner.Plugins.Cve/README.md)
* [AKS Plugin](Scanner/Scanner.Plugins.Aks/README.md)

