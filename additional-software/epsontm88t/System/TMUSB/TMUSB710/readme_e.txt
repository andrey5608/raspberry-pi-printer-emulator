
                      TMUSB device driver version 7.10b

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
  + Microsoft(R) Windows(R) XP Professional SP3 32bit Version
  + Microsoft(R) Windows(R) XP Professional SP2 64bit Version
  + Microsoft(R) Windows Server(R) 2003 R2 SP2 32bit Version
  + Microsoft(R) Windows Server(R) 2003 R2 SP2 64bit Version
  + Microsoft(R) Windows Vista(R) SP2 32bit Version
  + Microsoft(R) Windows Vista(R) SP2 64bit Version
  + Microsoft(R) Windows Server(R) 2008 SP2 32bit Version
  + Microsoft(R) Windows Server(R) 2008 SP2 64bit Version
  + Microsoft(R) Windows Server(R) 2008 R2 SP1 64bit Version
  + Microsoft(R) Windows 7(R) SP1 32bit Version
  + Microsoft(R) Windows 7(R) SP1 64bit Version
  + Microsoft(R) Windows 8(R) 32bit Version
  + Microsoft(R) Windows 8(R) 64bit Version
  + Microsoft(R) Windows Server(R) 2012 64bit Version
  + Microsoft(R) Windows 8.1(R) 32bit Version
  + Microsoft(R) Windows 8.1(R) 64bit Version
  + Microsoft(R) Windows Server(R) 2012 R2 64bit Version

  NOTE:
  + It support Embedded OS of OS-based of the above.
  + Please install TMUSB device driver version 8.00 or later on Windows 10 or later.

2.2 Support USB Host controllers
  + Intel(R) USB Host controller with built-in chipset.
  + NEC EHCI USB Host controller.

  NOTE:
  + The NEC OHCI host controller is not supported by TMUSB.

2.3 Support USB driver stacks
  + Microsoft(R) USB driver stack which is included in OS.
  + TMUSB doesn't support any third party driver stacks.

  NOTE:
  + Please use the latest version of Microsoft(R) USB driver stack.

  + Please check the driver stack version when device is connected by 
    USB 2.0 High speed connection. (usbehci.sys)

      Microsoft(R) Windows(R) XP Professional
        version 5.1.2600.1243 and later
      Microsoft(R) Windows Server(R) 2003
        version 5.2.3790.1830 and later
      Microsoft(R) Windows Vista(R)
        version 6.0.6000.16386 and later
      Microsoft(R) Windows Server(R) 2008
        first version and later
      Microsoft(R) Windows 7(R)
        first version and later

  NOTE:
    How to update Microsoft(R) USB driver stack.
      1) Connect your pc to the Internet
      2) Double click the system applet in control panel
      3) Click the hardware tab
      4) Press a device manager button
      5) Select USB controller
      6) Double click the 'XXX USB Enhanced Host Controller' item
      7) Press the Update Driver button in the driver tab.

  NOTE:
    How to check the version of driver stack (usbehci.sys).
      1-1) Double click the 'XXX USB Enhanced Host Controller' item within
           device manager
      1-2) Press the Driver Details button in the driver tab.

    or

      2-1) Select a file of usbehci.sys which exists in the Windows(R) system
           directory system32\drivers\usbehci.sys, and click the right mouse 
           button.
      2-2) Select the property tab and confirm the version information.

  NOTE:
    About ÅhUSB 3.0 Super SpeedÅh Port.
     TMUSB support only Microsoft(R) USB3.0 driver stack which is included 
     in Windows 8 and later OS.

2.4 Supported USB HUBs
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

2.5 Support Device
  EPSON TM/EU/BA Series

2.6 Support USB connection
  Max stage of USB hub : 5 stages
  NOTE:
    Need to use cable and hub which are comply with USB 2.0 specification.

2.7 Support power saving combinations
    Please connect the device to the PC directly. When the device is
    connected through USB 2.0 HUB, the power saving function is disabled.
    It is also possible that a USB1.0/1.1HUB will not work normally.
   
    This Power Saving function is OFF in Windows Vista/2008.
    When you want to ON it, you install TMUSB Driver with p1 option.
    You must set Power Management setting of the USB Root hub which
    "Allow the computer to turn off this device to save power" is ON.


3. FILES
----------
  This archive file contains these folders.

  + TMUSB710b
     + SETUP.exe
     + TMUSBXP  .....Folder containing TMUSB Driver files for 32Bit version
     + TMUSB64  .....Folder containing TMUSB Driver files for 64Bit version
     + ReadMeJ.txt
     + ReadMeE.txt

4. HOW TO INSTALL THE TMUSB DEVICE DRIVER
-------------------------------------
  You have to login as an administrator at the time of installation when you
  uses Windows(R) XP or Windows Server(R) 2003.
  You have to run SETUP.exe as administrator when you uses Windows Vista(R) and
  Windows Server(R) 2008/Windows 7(R).

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
  + The OS cannot be in Stand By or Sleep mode while the device is operating
    
    If the device is powered off or the USB cable disconnected while the OS is
    entering stand by/hibernation mode, the system will not operate normally.
    
    Please close printing applications, or else power off the device before
    setting the OS to stand by/hibernation mode.
    
6. HISTORY
----------
  + 09/13/2019 Version 7.10b Windows 10 or later was excluded from support OS.
  + 12/01/2017 Version 7.10a Solves a problem of high speed device.
  + 01/08/2016 Version 7.00a Support USB Filter. Windows 2000(R) cannot be supported.
  + 03/15/2012 Version 6.10a Support PC Standby function on Windows 7.
  + 01/27/2012 Version 6.00a Support S9000/S2000 and Bug fixed for bluescreen of power saving function.
  + 11/25/2009 Version 5.10a Solves a problem of the scanner data reception.
  + 09/17/2009 Version 5.00a Support Windows 7(R) and overlaped communication.
  + 04/06/2009 Version 4.00c Add CAT file of Windows XP 64 bits.
  + 01/13/2009 Version 4.00b Revised the issue of installation of 64 bits OS.
  + 10/31/2008 Version 4.00a Support coexist with a TMCOMUSB driver and other drivers
  + 05/20/2008 Version 3.20d Fix for problem of TMUSB Installer memory access violation
  + 01/28/2008 Version 3.20c Fix for problem of TMUSB Installer
  + 10/29/2007 Version 3.20b Fix for problem of Windows(R) 2000 WHQL Driver signature.
  + 09/26/2007 Version 3.10b TM-S1000 CD Package 
  + 09/07/2007 Version 3.10a Support TM-S1000/Windows Vista(R)
  + 01/19/2007 Version 2.20a Fix for TMUSB cannot PC Standby
  + 12/16/2005 Version 2.10a Support power saving for TM-T88IV
  + 10/28/2005 Version 2.06a Fix for TMCOMUSB (No signature)
  + 10/15/2004 Version 2.00a Support J9000/TMCOMUSB
  + 01/15/2004 Version 1.91a Support High speed HUB
  + 09/25/2003 Version 1.81a Support Windows(R) 2000 SP4 (Authentic version)

--- EOF ---
