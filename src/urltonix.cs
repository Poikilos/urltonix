using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IWshRuntimeLibrary; //needs IWshRuntimeLibrary.dll
 
namespace OrangejuiceElectronica {
	public class UrlToNix {
		public static string sFile="";
		public static string sBrowserNow="(before initialization)";
		public static bool bGood=false;
		public static int iFoundFiles=0;
		public static int iFoundUrls=0;
		public static string sLine="";
		public static string sLastComment="";
		public static string sMiniName="urltonix";
		public static string sMyName="UrlToNix";
		public static string sVersion="1.0";//loaded from 1.Version.txt
		public static readonly string[] sarrAllowedLinks=new string[]{"htm","php","asp"};
		public static int iDone=0;
		public static string sWholeCommand="";
		public static bool bWait=true;
		public static string sMyNameFull {
			get {
				return sMyName+" "+sVersion;
			}
		}
		public static bool HasAllowedLink(string sUrl) {
			bool bFound=false;
			int iMinLoc=2; //i.e. allows 1.htm but not ?htm where '?' is any character
			if (sUrl!=null && sUrl!="") {
				foreach (string sNow in sarrAllowedLinks) {
					if (sUrl.IndexOf(sNow)>=iMinLoc) {
						bFound=true;
						break;
					}
				}
			}
			return bFound;
		}
		public static void Main(string[] sarrArg) {
			UniWinForms.sFileOutput=Environment.GetFolderPath(Environment.SpecialFolder.Personal)+"/lib/urltonix/1.Output.txt"; //sFileOutput=UniWinForms.SpecialFolderByName("Personal")+"/lib/urltonix/1.Output.txt";
			if (File.Exists("1.Version.txt")) sVersion=UniWinForms.FileToString("1.Version.txt");
			sBrowserNow=UniWinForms.AnOpenBrowserName();
			bWait=true;
			if (sBrowserNow!="") {
				bWait=false;
				//sBrowserNow="(no browser detected)";
			}
			string sVerb="starting";
			StreamReader srNow=null;
			UniWinForms.WriteLine("(debug output file is \""+UniWinForms.sFileOutput+"\")");
			string sCumulative="";
			string sAllTried="";
			sWholeCommand="";
			try {
				int iArg=0;
				foreach (string sArg in sarrArg) {
					if (sWholeCommand!="") sWholeCommand+=" ";
					sWholeCommand+=sArg;
					sVerb="parsing argument "+sArg;
					if ((!(sArg=="mono"&&iArg==0)) && (!(sArg=="mono"&&iArg==0)) && (sArg!=sMiniName) && (sArg!=sMiniName+".exe")) {
						sFile=sArg;
						if (!File.Exists(sArg)) {//does not exist so try concatenation of arguments
							if (sCumulative!="") sCumulative+=" ";
							sCumulative+=sArg;
							sFile=sCumulative;
						}
						sVerb="parsing argument "+sArg+"(checking for quotes)";
						if (sFile.Length>2&&sFile[0]=='"'&&sFile[sFile.Length-1]=='"') sFile=sFile.Substring(1,sFile.Length-2);
						sVerb="parsing argument "+sArg+"(checking for local \"file://\" url notation)";
						if (sFile.Length>7&&sFile.ToLower().StartsWith("file://")) sFile=sFile.Substring(7);
						sVerb="parsing argument "+sArg+"(checking for percent-escaped characters)";
						UniWinForms.StripPercentEscapedByRef(ref sFile);
						if (sAllTried!="") sAllTried+=Environment.NewLine;
						sAllTried+="         "+sFile;
						if (File.Exists(sFile)) {//if parsed filename exist
							sCumulative="";
							iFoundFiles++;
							sVerb="opening file "+sFile;
							if (sFile.ToLower().EndsWith(".lnk")) {
								WshShell shell = new WshShell();
								IWshShortcut link = (IWshShortcut)shell.CreateShortcut(sFile);
								Console.WriteLine("Target path is {0}",link.TargetPath);
								Console.WriteLine("Working directory is {0}",link.WorkingDirectory);
							}
							else {//else not link file
							 srNow=File.OpenText(sFile);
							 int iLine=1;
							 ArrayList alNow=new ArrayList();
							 while (( sLine = srNow.ReadLine() ) != null ) {
								  try {
									 if (sFile.ToLower().EndsWith(".url")) alNow.Add(sLine);
									 sVerb="processing line \""+sLine+"\"";
									 while (sLine.StartsWith(" ")||sLine.StartsWith("\t")) sLine=sLine.Length>1?sLine.Substring(1):"";
									 while (sLine.EndsWith(" ")||sLine.EndsWith("\t")) sLine=sLine.Length>1?sLine.Substring(0,sLine.Length-1):"";
									 if (sLine.StartsWith("[")&&sLine.EndsWith("]")) {
										  sVerb="processing comment"+iLine.ToString()+" of "+sFile+": \""+sLine+"\"";
										  sLastComment="sLine";
									 }
									 else if (sLine!="") {// && !sLine.StartsWith("#")) {
										  sVerb="processing non-blank line"+iLine.ToString()+" of "+sFile+": \""+sLine+"\"";
										  int iSign=sLine.IndexOf("=");
										  string sHref="href=\"";
										  int iHref=sLine.IndexOf(sHref);
										  int iHrefCloser=-1;
										  if (iHref>=0) iHrefCloser=sLine.Substring(iHref+sHref.Length).IndexOf("\"")+(iHref+sHref.Length);
										  string sCommand = "";
										  int iUrl=sLine.IndexOf("http://");
										  if (sLine.StartsWith("http://")) {
											 sVerb="processing line "+iLine.ToString()+" of "+sFile+" which starts with \"http://\": \""+sLine+"\"";
											 int iSpace=sLine.IndexOf(" ");
											 sCommand=(iSpace>0)?sLine.Substring(0,iSpace):sLine;
										  }
										  else if (iHref>=0&&iHrefCloser>iHref) {//if (!sLine.StartsWith("http://")) {
												  sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has an href statement: \""+sLine+"\"";
												  iHref+=sHref.Length;
												  sCommand=sLine.Substring(iHref,iHrefCloser-iHref);
												  if (!HasAllowedLink(sCommand)) {
													 sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has an unusable href statement: \""+sLine+"\"";
													 sCommand="";
												  }
												  else {
													 sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has a usable href statement: \""+sLine+"\"";
												  }
										  }
										  else if (sLine.ToUpper().StartsWith("URL=")) {
											 sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has a standard URL= statement: \""+sLine+"\"";
											 sCommand=((iSign+1<sLine.Length)?sLine.Substring(iSign+1):"");
										  }
										  else if (iUrl>=0) {
											 sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has an unmarked link at index "+iUrl.ToString()+": \""+sLine+"\"";
											 int iEnder=sLine.Substring(iUrl).IndexOf(" ")+iUrl;
											 if (iEnder>iUrl) {
												  string sTemp=sLine.Substring(0,iUrl)+"["+sMiniName+"-found-url-here-to-end-of-line]"+sLine.Substring(iUrl);
												  sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has an unmarked link at character [index+1=]"+(iUrl+1).ToString()+" ending before ["+iEnder.ToString()+"]: \""+sTemp+"\"";
												  sCommand=sLine.Substring(iUrl,iEnder-iUrl);
											 }
											 else {
												  string sTemp=sLine.Substring(0,iUrl)+"["+sMiniName+"-found-url-here-to-end-of-line]"+sLine.Substring(iUrl);
												  sVerb="processing line "+iLine.ToString()+" of "+sFile+" which has an unmarked link at index [index+1=] "+(iUrl+1).ToString()+": \""+sTemp+"\"";
												  sCommand=sLine.Substring(iUrl);
											 }
										  }
										  if (sCommand!="") {
											 sVerb="processing command \""+sCommand+"\" on line "+iLine.ToString()+" of "+sFile+": \""+sLine+"\"";
											 iFoundUrls++;
											 System.Diagnostics.Process proc = new System.Diagnostics.Process();
											 proc.EnableRaisingEvents=false;
											 //if (iSpace>-1) {
											 //	sCommand=sLine.Substring(0,iSpace);
											 //	sArgs=sLine.Substring(iSpace+1);
												  proc.StartInfo.FileName=sCommand;
											 //	proc.StartInfo.Arguments=sArgs;
											 //}
											 //else {
											 //	proc.StartInfo.FileName=sLine;
											 //}
											 try {
												  if (iDone==1||iDone==0) { //==0 to prevent errors when program was just called but firefox isn't finished loading
													 UniWinForms.WriteLine();
													 if (bWait) {
														  UniWinForms.WriteLine("   Waiting for browser (prevents \"Firefox is already open\" errors)...");
														  UniWinForms.WriteLine();
														  Thread.Sleep(7000); //allows browser to load before dumping more stuff into it and confusing it (i.e. prevents "firefox already running" errors)
													 }
													 else {
														  UniWinForms.WriteLine("   Detected "+sBrowserNow+" so skipping program open delay.");
													 }
												  }
												  iFoundUrls++;
												  UniWinForms.WriteLine("   Telling computer to run: \""+sCommand+"\"");
												  proc.Start();
												  iDone++;
											 }
											 catch (Exception exn) {
												  UniWinForms.WriteLine("   OS could not execute \""+sCommand+"\"");
												  UniWinForms.WriteLine("      "+exn.ToString());
											 }
											 //proc.WaitForExit();
										  }//end if sCommand!=""
										  else UniWinForms.WriteLine("   NO URL in "+sFile+" LINE "+iLine.ToString());//+": "+sLine);
									 }//end if not blank
								  }
								  catch (Exception exn) {
									 UniWinForms.WriteLine("   Exception error while "+sVerb+".");
									 UniWinForms.WriteLine();
									 UniWinForms.WriteLine("   "+exn.ToString());
								  }
								  iLine++;
							 }//end while ReadLine
							 sVerb="closing file for reading";
							 srNow.Close();
							 sVerb="rewriting url file format: opening file for writing";
							 if (sFile.ToLower().EndsWith(".url")) {
								  StreamWriter swNow=new StreamWriter(sFile);
								  int iNow=0;
								  string sWindowsNewLine="\r\n";//used for url files
								  string sNewData="";
								  foreach (string sLineX in alNow) {
									 sVerb="rewriting url file format: line "+iNow.ToString();
									 if (iNow!=0) swNow.Write(sWindowsNewLine);
									 else if (sLineX.ToLower().StartsWith("http:")) {
										  swNow.Write("[InternetShortcut]"+sWindowsNewLine+"URL=");
									 }
									 swNow.Write(sLineX);
									 sVerb="rewriting url file format: finishing line "+iNow.ToString();
									 iNow++;
								  }
								  
								  sVerb="rewriting url file format: closing file";
								  swNow.Close();
								  sVerb="rewriting url file format: after closing file";
							 }//end if ends with ".url"
							 if (iFoundUrls<=0) UniWinForms.WriteLine("   No lines were found except "+((sLastComment!="")?("comments (i.e. "+sLastComment+")"):"blanks")+" in "+sFile);
							}//end else not lnk file
						}//end if file exists
					}//end if parameter in sarrArg is not the program itself
					//else {
						//UniWinForms.WriteLine("The file was not found, so nothing was done.");
					//}
					iArg++;
				}//end foreach sFile in sarrArg
				if (iFoundFiles<=0) {
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   Welcome to "+sMyName);
					UniWinForms.WriteLine();
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   To use this program, type");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("      mono "+sMiniName+".exe filename.url");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   where filename.url is the URL file that you want to execute");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   OR");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   create a \"File Association\" (i.e. in \"Settings\", \"Configure Konqueror\" in KDE browser)");
					UniWinForms.WriteLine("   for *.URL and *.url file masks and use \"mono ./"+sMiniName+".exe\"");
					UniWinForms.WriteLine("   as the preferred application where '.' is the directory");
					UniWinForms.WriteLine("   where "+sMiniName+".exe is located.");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("   TROUBLESHOOTING: after making the file association, make sure that");
					UniWinForms.WriteLine("   the full command for the preferred application is used");
					UniWinForms.WriteLine("   as of Konqueror 3.5.6-0.1.fc6, Konqueror reverts to using \"mono\",");
					UniWinForms.WriteLine("   so after setting the association you have to go back and re-add");
					UniWinForms.WriteLine("   the path to "+sMiniName+".exe after \"mono\" as shown above.");
					UniWinForms.WriteLine();
					UniWinForms.WriteLine("                     http://www.expertmultimedia.com/orangejuice/");
					UniWinForms.WriteLine();
				}
				bGood=iFoundUrls>0;
			}
			catch (Exception exn) {
				UniWinForms.WriteLine("   Exception error while "+sVerb+".");
				UniWinForms.WriteLine();
				UniWinForms.WriteLine("   "+exn.ToString());
			}
			//if (!bGood) UniWinForms.WriteLine("Finished unsuccessfully...");
			//UniWinForms.WriteLine("(Finished "+iCount.ToString()+" commands)");
			UniWinForms.WriteLine();
			UniWinForms.WriteLine("   Program Finished "+(bGood?"(Success)":"(nothing done)"));
			if (!bGood) {
				if (sAllTried!="") {
					UniWinForms.WriteLine("      Tried to find files:");
					UniWinForms.WriteLine(sAllTried);
				}
				else UniWinForms.WriteLine("      No files specified in command \""+sWholeCommand+"\".");
			}
			//Console.ReadLine();
			UniWinForms.WriteLine();
		}//end main
	}//end UrlToNix
}//end namespace
