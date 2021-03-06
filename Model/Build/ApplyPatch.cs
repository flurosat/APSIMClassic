﻿//css_import ../CSGeneral/Utility.cs
//css_import ../CSGeneral/StringManip.cs
//css_import ../CSGeneral/MathUtility.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
//using CSGeneral;

class JobSchedulerApplyPatch
{
    struct PatchDetail
    {
        public string FileName;
        public string Status;
        public string Revision;
    }

    /// <summary>
    /// This program applies a patch to an APSIM directory. It is called immediately after a patch file
    /// is unzipped into a virgin svn tree. All new files are added to svn. All deleted files (ie deleted via remote svn
    /// but still on this disk) are deleted.
    /// </summary>
    static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1)
               throw new Exception("Usage: ApplyPatch DirectoryName");
            Go(args[0]);
        }
        catch (Exception err)
        {
            Console.WriteLine(err.Message);
            return 1;
        }
        return 0;
    }

    private static void Go(string ApsimDirectoryName)
    {
        // Read in the patch information.
        List<PatchDetail> FileDetails = new List<PatchDetail>();
        string RevisionsFileName = Path.Combine(ApsimDirectoryName, "patch.revisions");
        if (!File.Exists(RevisionsFileName))
            throw new Exception("Cannot find the patch.revisions file in the APSIM root directory");
        StreamReader In = new StreamReader(RevisionsFileName);
        while (!In.EndOfStream)
        {
            string Line = In.ReadLine();
            StringCollection LineBits = CSGeneral.StringManip.SplitStringHonouringQuotes(Line, " ");
            if (LineBits.Count == 3)
            {
                // Convert either '/' or '\' to the local directory separator
                LineBits[0] = LineBits[0].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                FileDetails.Add(new PatchDetail {FileName=LineBits[0], Status=LineBits[1], Revision=LineBits[2] });
            }
        }
        In.Close();
        File.Delete(RevisionsFileName);

        // Find SVN.exe on the path.
        string SVNFileName;
        if (Path.DirectorySeparatorChar == '/') SVNFileName = CSGeneral.Utility.FindFileOnPath("svn");
        else SVNFileName = CSGeneral.Utility.FindFileOnPath("svn.exe");
        if (SVNFileName == "")
            throw new Exception("Cannot find svn on PATH");

        // Get a list of files currently known to SVN and their tip revision numbers.
        Process SVN = CSGeneral.Utility.RunProcess(SVNFileName, "-v stat", ApsimDirectoryName);
        string SVNStdOut = CSGeneral.Utility.CheckProcessExitedProperly(SVN);
        string[] Lines = SVNStdOut.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string> SVNFileNames = new Dictionary<string, string>();
        Regex R = new Regex("\\S+");
        foreach (string Line in Lines)
        {
            if (Line.Length >= 9 && Line[0] != '?' && Line.Substring(0, 9).Trim() != "")
            {
                // Get the tip revision and the filename from the SVN line.
                MatchCollection Matches = R.Matches(Line.Substring(10));
                if (Matches.Count > 3)
                {
                    string TipRevision = Matches[1].Value;
                    string FileName = "";
                    for (int i = 3; i < Matches.Count; i++)
                    {
                        FileName += Matches[i].Value + " ";
                    }
                    FileName = FileName.Trim();
                    SVNFileNames.Add(FileName, TipRevision);
                }
            }
        }

        // Some of the files in the patch file will be additions or deletions. We need to tell SVN
        // about these. We also need to check that all files are up to date.
        string OutOfDateFileNames = "";
        foreach (PatchDetail FileDetail in FileDetails)
        {
            string FullFileName = Path.Combine(ApsimDirectoryName, FileDetail.FileName);
            if (Path.GetFileName(FullFileName) != "patch.revisions")
            {
                string Arguments = null;
                if (FileDetail.Status == "Deleted")
                    Arguments += "delete --force " + CSGeneral.StringManip.DQuote(FullFileName);
                else if (FileDetail.Status == "Added")
                    Arguments += "add --force " + CSGeneral.StringManip.DQuote(FullFileName);

                if (Arguments != null)
                {
                    EnsureDirectoryIsUnderSVN(Path.GetDirectoryName(FullFileName), SVNFileName);
                    Console.WriteLine(SVNFileName + " " + Arguments);
                    Process P = CSGeneral.Utility.RunProcess(SVNFileName, Arguments, ApsimDirectoryName);
                    Console.WriteLine(CSGeneral.Utility.CheckProcessExitedProperly(P));
                }
                else
                {
                    if (!SVNFileNames.ContainsKey(FileDetail.FileName))
                        throw new Exception("Cannot find SVN info for patched file: " + FileDetail.FileName);
                    string TipRevision = SVNFileNames[FileDetail.FileName];

                    if (TipRevision != "?" && TipRevision != FileDetail.Revision)
                        OutOfDateFileNames += "\r\n" + FileDetail.FileName + " Tip: " + TipRevision + " File: " + FileDetail.Revision;
                }
            }
        }

        if (OutOfDateFileNames != "")
            throw new Exception("These files are out of date: " + OutOfDateFileNames);
    }

    /// <summary>
    /// Ensure a directory is under SVN control.
    /// </summary>
    private static void EnsureDirectoryIsUnderSVN(string DirectoryName, string SVNFileName)
    {
        if (!Directory.Exists(Path.Combine(DirectoryName, ".svn")))
        {
            int PosSlash = DirectoryName.LastIndexOf(Path.DirectorySeparatorChar);
            if (PosSlash == -1)
                throw new Exception("Invalid directory found: " + DirectoryName);
            string ParentName = DirectoryName.Substring(0, PosSlash);
            if (!Directory.Exists(Path.Combine(ParentName, ".svn")))
                EnsureDirectoryIsUnderSVN(ParentName, SVNFileName);  // parent dir.
            Directory.CreateDirectory(DirectoryName);
            Console.WriteLine(SVNFileName + " add " + CSGeneral.StringManip.DQuote(DirectoryName) + " --depth empty");
            Process P = CSGeneral.Utility.RunProcess(SVNFileName, "add " + CSGeneral.StringManip.DQuote(DirectoryName) + " --depth empty", DirectoryName);
            Console.WriteLine(CSGeneral.Utility.CheckProcessExitedProperly(P));
        }
    }




}

