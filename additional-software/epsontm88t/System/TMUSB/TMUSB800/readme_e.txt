
                      TMUSB device driver version 8.00b

                                                                   09/13/2019
                                                      SEIKO EPSON Corporation

1. INTRODUCTION
---------------
  TMUSB is a USB device driver for EPSON TM/EU/BA Series.
  Although the device driver installation is generally done by the device,
  wizard, this setup utility also supports a silent installation.

2. ENVIRONMENT
--------------
2.1 Support OS
  + Microsoft(R) Windows 10(R) 32bit Version
  + Microsoft(R) Windows 10(R) 64bit Version
  + Microsoft(R) Windows Server(R) 2016 64bit Version
  + Microsoft(R) Windows Server(R) 2019 64bit Version

  NOTE:
  + It support Embedded OS of OS-based of the above.
  + Please install TMUSB device driver version 7.10b on Windows 8.1 or earlier.


2.2 Support USB driver stacks
  + Microsoft(R) USB driver stack which is included in OS.
  + TMUSB doesn't support any third party driver stacks.

  NOTE:
  + Please use the latest version of Microsoft(R) USB driver stack.

2.3 Supported USB HUBs
  USB 1.1 Full Speed HUB
  USB 2.0 High Speed HUB
  USB 3.0 Super Speed HUB

  NOTE
    + TMUSB doesn't support USB 1.0 HUB.

    + A High Speed connection becomes available when a Full Speed device is
      connected via the USB 2.0 HUB, even if your EPSON TM/EU/BA series device
      does not support USB 2.0 High Speed
      (only when your host PC has an EHCI host controller).

      Again, see the requirements in "When the device is connected
      via USB 2.0 High Speed connection" described earlier.

2.4 Support Device
  EPSON TM/EU/BA Series

2.5 Support USB connection
  Max stage of USB hub : 5 stages
  NOTE:
    Need to use cable and hub which are comply with USB 2.0 specification.

3. FILES
----------
  This archive file contains these folders.

  + TMUSB800b
     + SETUP.exe
     + TMUSBXP  .....Folder containing TMUSB Driver files for 32Bit version
     + TMUSB64  .....Folder containing TMUSB Driver files for 64Bit version
     + ReadMeJ.txt
     + ReadMeE.txt

4. HOW TO INSTALL THE TMUSB DEVICE DRIVER
-------------------------------------
  You have to run SETUP.exe as administrator.

4.1 How to install/update

  1) Power off all devices which are connected to the system by a USB cable.
  2) Execute the setup program on the command prompt. Attach start-up options as necessary.
     For details on start-up options, see 4.2 Options for TMUSBSetup.
  3) Power on the device when installation is complete.

      Setup program copies both INF file and SYS file to the system.
      Setup program return a result of installation.

      You can refer the result by ERRORLEVEL variable.
        0    : Succeeded
        2    : Installation files are not found.
        1151 : Not supported OS.
        1223 : A user canceled installation.
        3010 : Installation is successful.Changes will not be effective until
               the system is rebooted.

4.2 Options for TMUSBSetup

  -s2 Silent installation
     Don't install the specified version of TMUSB when a newer version
     is installed already.
  -p1 Enable Device Power Saving.
  -p2 Disable Device Power Saving.

4.3 Notes for installation of a TM-C100

  Please be sure to install the printer driver of TM-C100 first before
  connecting a TM-C100 to the Host PC.
  
  When "USB Printing Support" is displayed on the device manager,
  please do the following:
  
   1) Connect the TM-C100 to the PC and turn on the power supply.
   2) If "USB Printing Support" is displayed in the Device Manager, 
      right-click "USB Printing Support" and select "Delete".
   3) Turn the power for the TM-C100 off and then back on again.
   4) Confirm that "EPSON USB Controller for TM/BA/EU Series" is
      displayed in the Device Manager.

5. RESTRICTIONS
---------------
  + You must run SETUP.exe as administrator.
  + All applications that use the TMUSB device driver must be closed before
    executing the setup program.
  + The device cannot be powered of, or the USB cable disconnected while the 
    device is printing.
  + Windows(R) must be restarted, or else the device powered off and then 
    powered back on again, in order for the system to recognize that a new 
    device driver is installed.
  + You need wait 5 seconds to power on after device's power is off.
    This time is required to ensure that the device driver is unloaded.
  + When you login as an guest, the system reports that there is no driver's 
    signature.
  + In order to use the setup program, Internet Explorer version 4.0 or
    higher is required.
  + If the device is powered off or the USB cable disconnected while the OS is
    entering stand by/hibernation mode, the system will not operate normally.
    
    Please close printing applications, or else power off the device before
    setting the OS to stand by/hibernation mode.
    
6. HISTORY
----------
  + 09/13/2019 Version 8.00b Support Windows 10 security features. Windows 8.1 or earlier was excluded from support OS.

--- EOF ---
