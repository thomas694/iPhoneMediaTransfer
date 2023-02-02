# iPhoneMediaTransfer

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

A command-line tool for transferring photos and videos from an iPhone to a folder on the local computer.

## Overview

The tool downloads the media database from the iPhone, then it copies all photos and videos from the phone into the local media library path. The media files are stored inside `DCIM` and `DCIM_deleted`. Afterwards it replicates the album structure inside the folder `Albums` by using copies or optionally hardlinks.

```
Library Path
├── Albums
├── DCIM
└── DCIM_deleted
```

## Requirements

The iTunesMobileDevice.dll and dependent DLLs from the packages "Apple Application Support" and 
"Apple Mobile Device Support" (driver) are needed.

### How to download these packages?

Download an older iTunes Installer from the Internet, make sure to check the Digital Signature to be sure, 
and extract it with e.g. 7zip to get the MSI packages.
The referenced library project used the iTunesMobileDevice.dll 757.3.2.1 which belongs to iTunes 12.1.1 from Jan 2015.
So you can either use the "Application Support" (3.1.2) and "Mobile Device Support" (8.1.1.3) installers from this iTunes version and 
use the DLLs in their respective folders.
Or you can use a newer version of the driver (Mobile Device Support, tested 14.1.0.35), but then you have to use the project's old iTunesMobileDevice.dll as newer 
versions of the driver don't contain that one any more.
<br>

## Usage
```
iPhoneMediaTransfer v1.0 compiled 02.02.2023 (c) thomas694 (@GH)
Transferring photos and videos from iOS device to PC.
This program comes with ABSOLUTELY NO WARRANTY. This is free software, and you
are welcome to redistribute it under certain conditions; view GNU GPLv3 for more.

Usage: iPhoneMediaTransfer [options] <folder>
Options:
 -h[elp]|?    Shows this help screen
 -database    Copies the Photos.sqlite from the device to <folder>\Photos_yyyyMMddHHmmss.sqlite.
              If not specified, the newest DB in the folder will be used.
 -media       Transfer photos and videos from iOS device to PC.
 -hardlinks   Create hardlinks. Works on NTFS file systems only and needs admin rights.
 -adjust      Adjust the modified date of the media files with values from the DB.
 <folder>     Path to your target folder.
```

## License <a rel="license" href="https://www.gnu.org/licenses/gpl-3.0"><img alt="GNU GPLv3 license" style="border-width:0" src="https://img.shields.io/badge/License-GPLv3-blue.svg" /></a>

<span xmlns:dct="http://purl.org/dc/terms/" property="dct:title">iPhoneMediaTransfer</span> by thomas694 
is licensed under <a rel="license" href="https://www.gnu.org/licenses/gpl-3.0">GNU GPLv3</a>.
