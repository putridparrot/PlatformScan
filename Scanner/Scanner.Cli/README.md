# Platform Scan CLI

# Use default appsettings.json
platformscan run --plugin tls-scanner

# Use custom config file
platformscan run --plugin tls-scanner --config /path/to/custom-config.json
platformscan run --plugin tls-scanner -c ./production-settings.json

# Works with other options too
platformscan run --plugin aks-health --config ./azure-config.json --format junit --output results.xml