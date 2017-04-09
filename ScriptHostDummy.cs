//Purpose: This is a set of classes to replace IWshRuntimeLibrary.dll (which is a wrapper for wshom.ocx, which is Microsoft Windows (R) Script Host), for cross platform use of certain aspects of it.
//File: formerly IWshRuntimeLibrary.cs, formerly IWshRuntimeLibrary_UniWinForms.cs
//Originally developed by: expertmm (for UniWinForms)

using System;
using System.IO;

namespace IWshRuntimeLibrary {
	public class IWshShortcut {
		public string TargetPath="";//TODO: change to a get accessor
		public string WorkingDirectory="";//TODO: change to a get accessor

		public const uint Flag_HasShellItemIDList=1;
		public const uint Flag_PointsToFileOrDir_NotSomethingElse=2;
		public const uint Flag_HasDescription=4;
		public const uint Flag_HasRelPath=8;
		public const uint Flag_HasWorkingDir=16;
		public const uint Flag_HasArgs=32;
		public const uint Flag_HasCustomIcon=64;

		public const uint FileAttrib_ReadOnly=1;
		public const uint FileAttrib_Hidden=2;
		public const uint FileAttrib_System=4;
		public const uint FileAttrib_IsAVolumeLabel=8;//not possible
		public const uint FileAttrib_Directory=16;
		public const uint FileAttrib_Archive=32;
		public const uint FileAttrib_Encrypted_NTFS_EFS=64;
		public const uint FileAttrib_Normal=128;//unknown meaning
		public const uint FileAttrib_Temp=256;
		public const uint FileAttrib_Sparse=512;
		public const uint FileAttrib_HasReparsePointData=1024;
		public const uint FileAttrib_Compressed=2048;
		public const uint FileAttrib_Offline=4096;

		//only 1st 3 are allowed in the link property window:
		public const uint SW_HIDE=0;
		public const uint SW_NORMAL=1;
		public const uint SW_SHOWMINIMIZED=2;
		public const uint SW_SHOWMAXIMIZED=3;
		public const uint SW_SHOWNOACTIVATE=4;
		public const uint SW_SHOW=5;
		public const uint SW_MINIMIZE=6;
		public const uint SW_SHOWMINNOACTIVE=7;
		public const uint SW_SHOWNA=8;
		public const uint SW_RESTORE=9;
		public const uint SW_SHOWDEFAULT=10;

		//START HEADER
		uint dwL=(uint)'L';//always 'L'
		byte[] byarrGuid=new byte[]{0x01, 0x14, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46};//always 16 bytes, always {00021401-0000-0000-00C0-000000000046}
		uint Flags=0;
		uint FileAttribs=0;//attribs of target; zero if not a file
		ulong ulTimeCreated=0;
		ulong ulTimeModified=0;
		ulong ulTimeAccessed=0;
		uint dwFileLen=0;//of target
		uint dwIconNum=0;//zero unless Flag_HasCustomIcon
		uint SW_ShowWndOpCode=0;
		uint dwHotkey=0;
		uint dwReserved1=0;
		uint dwReserved2=0;
		//END HEADER
		///Shell item ID list - only if ( 0 != (Flags & Flag_HasShellItemIDList))
		ushort wShellItemIDListLen=0;//not present if no flag, does not include self
		byte[] byarrShellItemIDList=null; //helps get to the file --see ITEMIDLIST in win32 docs
		//i.e. {==42} {(ushort)50/*always?*/,(uint)FileLen,(uint)1047602592/*always*/,
		//(uint)wUnknown/*fileattrib?*/,(string)sLongName/*null-terminated*/,
		//(string)sShortName/*null-terminated*/, (uint)0/*length value of something? this ushort is included in dwShellItemIDListLen bytecount*/}

		///file Location Info - always present but only non-zero-length if ( 0 != (Flags & Flag_PointsToFileOrDir_NotSomethingElse) 
		public const uint FileLocator_Flag_Local=1;
		public const uint FileLocator_Flag_Network=2;
		//File locator info (offsets are relative to the start of this struct
		uint dwFileLocator_Len=0;//includes all pathnames data structures
		uint dwPointerToSkipThisStruct=0;
		uint FileLocator_Flags=0;
		uint dwOffsetOfLocalVolumeInfo=0; //offset of table (random if not FileLocator_Flag_Network)
		uint dwOffsetOfBasePathnameOnLocalSystem=0; //offset of base local path (random if not FileLocator_Flag_Network)
		uint dwOffsetOfNetworkVolumeInfo=0; //offset of network volume table
		uint dwOffsetOfRemainingPathname=0; //offset of final part of path

		//Local Volume Table
		public const uint VolumeType_Unknown=0;
		public const uint VolumeType_NoRootDir=1;
		public const uint VolumeType_Removable=2;
		public const uint VolumeType_Fixed=3;
		public const uint VolumeType_Remote=4;
		public const uint VolumeType_CDROM=5;
		public const uint VolumeType_RamDrive=6;

		uint dwLocalVolumeTable_MyLen=0;
		uint dwLocalVolumeTable_VolumeType=0;
		uint dwLocalVolumeTable_VolumeSerial=0;
		uint dwLocalVolumeTable_Offset=0x10;//always 16 -- offset of VolumeLabel within the struct
		string sVolumeLabel="";//load/save as null-terminated ascii string

		//Network Volume Table (unknowns are always same for windows file/printer sharing
		uint dwNetworkVolumeTable_Len=0;
		uint dwNetworkVolumeTable_Unknown=2;//2 (always?)
		uint dwNetworkVolumeTable_NetworkShareNameOffset=0x14;//always 20
		uint dwNetworkVolumeTable_Unknown2=0;//0 (always?)
		uint dwNetworkVolumeTable_Unknown3=0x20000;//131072 (always?)
		string sNetworkShareName="";//load/save as null-terminated ascii string

		ushort wDescriptionLen=0;//only present if ( 0 != (Flags & Flag_HasDescription) )
		string sDescription="";//only present if ( 0 != (Flags & Flag_HasDescription) )
		ushort wRelPathLen=0;//only present if ( 0 != (Flags & Flag_HasRelPath) )
		string sRelPath="";//only present if ( 0 != (Flags & Flag_HasRelPath) )
		ushort wWorkingDirectoryLen=0;//only present if ( 0 != (Flags & Flag_HasWorkingDir) )
		string sWorkingDirectory="";//only present if ( 0 != (Flags & Flag_HasWorkingDir) )
		ushort wArgsLen=0;//only present if ( 0 != (Flags & Flag_HasCommandLine) )
		string sArgs="";//i.e. "/close" -- only present if ( 0 != (Flags & Flag_HasCommandLine) )
		ushort wIconFileNameLen=0;//only present if ( 0 != (Flags & Flag_HasCustomIcon) )
		string sIconFileName; //only present if ( 0 != (Flags & Flag_HasCustomIcon) )
		uint dwFooterDumpLen=0;//usually 0
		byte[] byarrFooterDump=null;
	}//end IWshShortcut
	public class WshShell {
		public static string sForeignPathDelimiter { get {return char.ToString(cForeignPathDelimiter);} }
		public static char cForeignPathDelimiter='\\';
		public IWshShortcut CreateShortcut(string sFile) { //TODO: in the real WshShell class this really returns something else that can be typecast to a IWshShortcut -- find out what
            IWshShortcut link=null;
            FileStream fsNow=null;
            BinaryReader brNow=null;
            string sParticiple="<before initializing>";
			try {
                sParticiple="creating IWshShortcut";
				link=new IWshShortcut();
				sParticiple="creating FileStream";
				fsNow=new FileStream(sFile, FileMode.Open, FileAccess.Read);
				sParticiple="creating BinaryReader";
				brNow = new BinaryReader(fsNow);
				sParticiple="getting FileInfo";
				long nBytes=(new FileInfo(sFile)).Length;
				sParticiple="reading bytes";
				byte[] byarrData=brNow.ReadBytes((int)nBytes);
				
				int iPos=0;
				int iPosStrucNow=0;
				//TODO: finish this:
				int iOffsetNow=1;//start at 1 since path is always is preceded by drive letter
				int iStringsMax=10;//debug hard-coded limitation
				int iStrings=0;
				int[] iarrStringStart=new int[iStringsMax];
				int[] iarrStringLen=new int[iStringsMax];
				int iFindNow;
				int iEnder;
				sParticiple="interpreting bytes";
				while (iOffsetNow+1<byarrData.Length && iStrings<iStringsMax) {
					 iFindNow=-1;
					 iEnder=-1;
				//for (int iNow=iOffsetNow; iNow+1<byarrData.Length; iNow++) {
					if ((char)byarrData[iOffsetNow]==':' && (char)byarrData[iOffsetNow+1]==cForeignPathDelimiter) {
						for (int iEnderNow=iOffsetNow+1; iEnderNow<byarrData.Length; iEnderNow++) {
							 if (byarrData[iEnderNow]=='\0'||iEnderNow==byarrData.Length) {
								iEnder=iEnderNow;
								iarrStringStart[iStrings]=iOffsetNow-1;
								iarrStringLen[iStrings]=iEnder-iarrStringStart[iStrings];
								iStrings++;
								iOffsetNow=iEnder+1;
								break;
							 }//end if found null-terminator
						}//end for looking for ender (iEnderNow starts at iOffsetNow+1)
					 }//end if found ":\"
					 //}//end for iNow (looking for ":\" ; starts at offset)
					 if (iEnder==-1) break;
				}//end while Offset not too close to end
				int iFoundExe=-1;
				string[] sarrResults=null;
				link.WorkingDirectory="";
				link.TargetPath="";
				sParticiple="interpreting strings";
				if (iStrings>0) {
					sarrResults=new string[iStrings];
					string sData="";
					for (int iChar=0; iChar<byarrData.Length; iChar++) {
						sData+=char.ToString((char)byarrData[iChar]);
					}
					for (int iNow=0; iNow<iStrings; iNow++) {
						sarrResults[iNow]=sData.Substring(iarrStringStart[iNow],iarrStringLen[iNow]);
						Console.Write("IWshRuntimeLibrary_Mono Found Link Target: \""+sarrResults[iNow]+"\"");
						if (sarrResults[iNow].ToLower().EndsWith(".exe")) {
							Console.Write(" (set as TargetPath)");
							iFoundExe=iNow;
						}
						Console.WriteLine();
					}
					//now find the working folder:
					if (iFoundExe>-1) {
						int iLastSlash=sarrResults[iFoundExe].LastIndexOf(sForeignPathDelimiter);
						if (iLastSlash>-1) link.WorkingDirectory=sarrResults[iFoundExe].Substring(0,iLastSlash);
					}
					if (link.WorkingDirectory=="") {
						for (int iNow=0; iNow<iStrings; iNow++) {
							 if (iNow!=iFoundExe) link.WorkingDirectory=sarrResults[iNow];
						}
					}
					if (link.WorkingDirectory!="") Console.WriteLine("Working Folder: \""+link.WorkingDirectory+"\"");
					else Console.WriteLine("Working Folder: none");
				}//end if iStrings>0
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in wshshell CreateShortcut while "+sParticiple+":");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			try { brNow.Close();}
			catch { }
			try { fsNow.Close();}
			catch { }
			return link;
		}//end CreateShortcut
	}//end WshShell
}//end namespace
