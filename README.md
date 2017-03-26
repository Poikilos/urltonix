# urltonix
("URL To 'Nix")
by expertmm
Windows(R) filetype handler for GNU/Linux (should work on other unix variants as well), for URL files
http://www.github.com/expertmm
This program is distributed under the GPL 3.0 (see LICENSE file)

## Requirements
* A non-Windows OS that has mono (and shell script recommended)

## How to Use
```
    mono urltonix filename.url
```
where filename.url is the URL file that you want to execute

However, creating a file association via a link seems more reliable:

### KDE
* create a \"File Association\" (i.e. in \"Settings\", \"Configure Konqueror\" in KDE browser) 
for *.URL and *.url file masks and use \"mono ./urltonix.exe\"
as the preferred application where '.' is the directory
where urltonix.exe is located.

### Gnome:
* create a file named ~/.local/share/applications/urltonix.desktop containing the text:
```
[Desktop Entry]
Version=1.0
Encoding=UTF-8
Name=urltonix
Comment=
Comment[en_US]=
Exec=/home/Owner/lib/urltonix/urltonix
GenericName=urltonix
GenericName[en_US]=urltonix
Icon=/home/Owner/.icons/MacOS-X/48x48/apps/gnome-globe.png
MimeType=;
Name[en_US]=urltonix
Path=$HOME/lib/urltonix
StartupNotify=true
Terminal=false
TerminalOptions=--noclose
Type=Application
X-DCOP-ServiceType=
#X-KDE-SubstituteUID=false
#X-KDE-Username=
```
* add the following two lines to ~/.local/share/applications/defaults.list:
```
text/x-uri=urltonix.desktop
application/x-mswinurl=urltonix.desktop
```

NOTE:
After making the file association, make sure that
the full command for the preferred application is used
(since at least in Konqueror 3.5.6-0.1.fc6, Konqueror reverts to using \"mono\",
so after setting the association you have to go back and re-add
the path to urltonix.exe after \"mono\" as shown above).


## Changes
* dig through file for all urls including href="" or lines that include http end url with newline
	* allow enders other than newline (in this order!) ". ", " ", "<", ".&nbsp;", "&nbsp;"
* upon load, automatically convert files with url extension but start with "http" to windows internet shortcuts ("[InternetShortcut]\n\rURL=")
* (2007-09-17) added support for local "file://" url notation and gnome integration.
* (2007-09-17) detects browser and only delays if browser is not open (only detects firefox*, mozilla*, and iceweasel* processes)


## Known Issues
* allow opening of non-executable files, and load appropriate wine WINDOWS application or select from list of installed windows apps!
    * change paths before forwarding to wine programs: change / to Z:/ then change slashes to backslashes
* Fix bad .url (check as case insensitive!) files--change to:
	"[InternetShortcut][\r\n]" --must be CRLF, \r\n
	"URL=http://www[...]"
* Add support for LNK files
    * NOTE: supposedly this is not needed, however, wine windows program loader doesn't seem to work with lnk files created on linux desktop by wine after installing Adobe PhotoShop Elements 5.0 (R)
	* lnk files can be unicode, and contain binary data
	* maintain a list of possible paths to C: and to D: separately.
	* remember to change '\' to '/'!
	* remember to look for case-insensitive matches by doing directory lists in linux!
	* lastly, try:
		* change "*My Documents" AND "*MYDOCU~1" to "~" OR "~/Documents" OR "~/My Documents"
			* OR change "?:\Documents and Settings\[search to next slash]\"
			           AND "?:\DOCUME~1\[search to next slash]\"
			    to "~/"
		(keep trying OR operands until a match is found.  If still not found, show "File not found" and list ALL tried, mentioning that the attempt to find a match was "(case-insensitive)")
	* FilePath is stored in unicode somewhere in the file, but there is no unicode header (0xFFFE which would become 0xFEFF). 
