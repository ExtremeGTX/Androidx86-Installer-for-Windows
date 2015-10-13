#Hide All NSIS Windows
WindowIcon Off
#Enable silent mode
SilentInstall Silent
#AutoClose after exit
AutoCloseWindow True
#package icon
Icon "android.ico"
#package exe name
OutFile Androidx86-Installv2.exe

Section
	#create a directory for the app in TEMP
	StrCpy $INSTDIR $TEMP\droidinst_efi
	
	#Use that directory created in TEMP as the working directory
	SetOutPath $INSTDIR
	
	#Files to be included in the package
	File "..\tools\7z.dll"
	File "..\tools\7z.exe"
	File "..\tools\cygblkid-1.dll"
	File "..\tools\cygcom_err-2.dll"
	File "..\tools\cyge2p-2.dll"
	File "..\tools\cygext2fs-2.dll"
	File "..\tools\cyggcc_s-1.dll"
	File "..\tools\cygiconv-2.dll"
	File "..\tools\cygintl-8.dll"
	File "..\tools\cyguuid-1.dll"
	File "..\tools\cygwin1.dll"
	File "..\tools\dd.exe"
	File "..\tools\grub.cfg"
	File "..\tools\grubx64.efi"
	File "..\tools\mke2fs.conf"
	File "..\tools\mke2fs.exe"
	File "..\Build\Debug\Android_UEFIInstaller.exe"
	File "..\Build\Debug\MaterialDesignColors.dll"
	File "..\Build\Debug\MaterialDesignThemes.Wpf.dll"
	File "..\Build\Debug\Win32UEFI.dll"
	File "about.txt"
	
	#launch the main EXE inside the package
	ExecWait "$INSTDIR\Android_UEFIInstaller.exe"
	
	#change working directory
	SetOutPath $TEMP
	
	#remove working directory
	RMDir /r $INSTDIR
SectionEnd 