# Upload berry source files to TASMOTA ESP32 device for rapid development
## Options

```
  -u, --url          (Default: http://192.168.178.101:5001) Host local server on this url

  -d, --device       Required. IP/Adress of tasmota device

  -f, --folder       Required. (Default: C:\temp\berry\) Folder to watch

  --filter           (Default: *.be) Filter for files to watch

  --restart          (Default: true) Restart device after each upload

  --initialUpload    (Default: true) Upload all files at program start

  --help             Display this help screen.

  --version          Display version information.

```

## Install

Use dotnet cli to compile
``` cmd
dotnet publish berrywatch.csproj -o c:\tools\berrywatch --no-self-contained
```
