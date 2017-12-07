using UnityEngine;
using System.Collections;
using System.IO;

public class FileHandler {
	public static string readFile(string name, string path, string namewithextension)	{
		StreamReader streamReader = new StreamReader(path);
		string contents = streamReader.ReadToEnd();
		streamReader.Close();
		return contents;
	}

	public static FileObject[] getSkinFiles(string folder)	{
		DirectoryInfo info = new DirectoryInfo (folder);
		FileInfo[] fileInfos = new FileInfo[0];
		try {
			fileInfos = info.GetFiles ();
		} catch (DirectoryNotFoundException) {
			FileHandler.createDirectory(folder);
		}
		FileObject[] files = new FileObject[fileInfos.Length];
		
		for (int i = 0; i < fileInfos.Length; i++) {
			files[i] = new FileObject(fileInfos[i].FullName);
		}
		
		return files;
	}

	public static FileObject[] getFiles(string folder)	{
		DirectoryInfo info = new DirectoryInfo (folder);
		FileInfo[] fileInfos = new FileInfo[0];
        try
        {
            fileInfos = info.GetFiles();
		} catch (DirectoryNotFoundException) {
			FileHandler.createDirectory(folder);
		}
		FileObject[] files = new FileObject[fileInfos.Length];

		for (int i = 0; i < fileInfos.Length; i++) {
			files[i] = new FileObject(fileInfos[i].FullName);
		}

		return files;
	}
 
	public static string[] getFolders(string folder)	{
		try {
			return Directory.GetDirectories (folder);
		}
		catch (DirectoryNotFoundException)	{
			createDirectory(folder);
			return Directory.GetDirectories (folder);
		}
	}

	public static bool writeFile(string path, string text)	{
		if (File.Exists(path + ".keys"))	{
			Debug.Log("File " + path + ".keys already exists!");
		    return false;
		}
		else {
			File.WriteAllText(path + ".keys", text);
			Debug.Log("Write file " + path + ".keys");
			return true;
		}
	}

	public static bool createDirectory(string path)	{
		if (Directory.CreateDirectory (path) != null)
			return true;
		else
			return false;
	}
}