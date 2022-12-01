# Upload berry source files to TASMOTA ESP32 device for rapid development

This tool monitors a local folder and automatically transfers the Berry files contained there to a TASMOTA ESP32 device and restarts it if desired. 

A local web server is started to transfer the files. The download is triggered via a GetUrl command on the TASMOTA devices.

## Install

Use dotnet cli to install this as global tool. Run the following in folder **Watcher**
``` cmd
dotnet pack
rem only if already installed
dotnet tool uninstall -g berrywatch
dotnet tool install --global --add-source ./nupkg berrywatch
```
## Usage
If this tool is installed via *dotnet tool install* you can simple run it on the command shell

```
berrywatch -u http://192.168.178.101:5001 -d 192.168.178.179 -f c:\Projekte\berry\m5stack
```

## Options

```
  -u, --url          Required. Host local server on this url. Example: http://192.168.178.101:5001

  -d, --device       Required. IP/Adress of tasmota device

  -f, --folder       Required. Folder to watch. Must be an absolute path.

  --filter           (Default: *.*) Filter for files to watch

  --restart          (Default: true) Restart device after each upload

  --initialUpload    (Default: true) Upload all files at program start

  --help             Display this help screen.

  --version          Display version information.

```

