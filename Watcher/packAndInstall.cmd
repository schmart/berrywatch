dotnet pack
rem only if already installed
dotnet tool uninstall -g berrywatch
dotnet tool install --global --add-source ./nupkg berrywatch
