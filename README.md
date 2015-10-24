# Androidx86 Installer for Windows
This installer will help users install Android-x86 on PC from windows, without HDD repartioning or messing things up

![Alt text](docs/droidinst.png "Androidx86 Installer UEFI")

## Features
- Support UEFI-Enabled PCs
- Support Legacy-BIOS PCs
- Install/Uninstall Android directly from Windows
- Install to Any FAT32/NTFS partitions


## Requirements
- UEFI-Enabled x64 PC
- Secure Boot Disabled
- Windows 8/8.1/10
- .Net Framework 4.5
- Android System image with **UEFI** Support from [Android-x86.org](www.android-x86.org)


## Change log
v2.1
 - User-defined Data size
 - Responsive UI
 - Installation Status update
 - Support Devices with 32-bit firmware
 - Support booting from NTFS with compression enabled
 - log includes more info about Device BIOS

v2.0
 - Initial Version


## Build Instructions
TODO


## External Components used:
- GRUB2 Bootloader [GNU GRUB](https://www.gnu.org/software/grub/)
- Android icon by [benbackman](http://benbackman.deviantart.com/art/Android-Icon-178754467)
- MaterialDesignXamlToolkit by [ButchersBoy](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- 7zip by [Igor Pavlov](http://www.7-zip.org/)
- mke2fs tools by [Cygwin](https://www.cygwin.com/)
- UEFILib by [ExtremeGTX](https://github.com/ExtremeGTX/Win32-UEFILibrary)
