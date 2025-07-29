### <div align="center"><b><a href="README.md">Readme on English</a> | <a href="README-RUS.md">Readme on Russian</a></b></div>

# NetworkChat

## Product Purpose
Graphical client-server application for exchanging text messages

## Prerequisites
- Install Visual Studio 2022 Community Edition

## Building the project on Windows
- Edit the Windows UI in Visual Studio `NcMainPage.xaml`
- Publish the NcWpf project

## Installing GLADE on Windows
- Install [MSYS2 - Software Distribution and Building Platform for Windows](https://www.msys2.org/)
  - Download the installer `msys2-x86_64-20250622.exe`
  - Install to the C:\msys64 directory
  - Run MSYS2 from the Start menu
  - Run `pacman -S mingw-w64-x86_64-glade` in the MSYS2 console
  - Run GLADE for Windows UI `c:\msys64\mingw64\bin\glade.exe`

## Building the project on Linux
- Edit the UI on Linux in GLADE `NcMainWindow.glade`
- Publish the NcGtk project

## Installing GLADE on Linux
```sudo apt-get install glade```

## Running on Ubuntu Linux
- Install VirtualBox
- Install Ubuntu
- Install VirtualBox Guest Additions
  - VirtualBox - Devices - Insert Guest Additions CD image
  - Install Guest Additions
```
sudo apt update
sudo apt upgrade -y
sudo apt install build-essential dkms linux-headers-$(uname -r)
cd /media/<UserName>/VBox_GAs_7.1.12
sudo sh VBoxLinuxAdditions.run
```
- Install GTK3 to run NcGtk
```
sudo apt update
sudo apt install -y build-essential libgtk-3-dev pkg-config
```
- Installieren Sie das .NET SDK, um NcGtk zu starten
```
sudo apt update
sudo add-apt-repository ppa:dotnet/backports
sudo apt install dotnet9
# sudo apt install dotnet-runtime-9.0
# sudo apt install aspnetcore-runtime-9.0
# sudo apt install dotnet-sdk-9.0
dotnet --version
dotnet --list-sdks
```
- Installieren Sie [LocalSend](https://localsend.org/ru) für den Austausch von Dateien über das lokale Netzwerk
```
sudo snap install localsend
localsend
```
- Starten Sie NcGtk
```
cd ~/Downloads/NcGtkRelease
dotnet NcGtk.dll
```
