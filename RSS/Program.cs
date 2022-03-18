using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RobloxSoundScraper
{
    internal class Program
    {
        static List<string> URLBank = new List<string>();
        static List<string> IDBank = new List<string>();
        static string path;
        static string fileName;
        static string fileDirectory;
        static bool metaFileNames = true;
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            while (true)
            {
                Console.WriteLine("Enter path to valid .rbxlx path");
                path = @Console.ReadLine();
                Console.Clear();
                if (!File.Exists(@path))
                {
                    try
                    {
                        if (path[0] == '"')
                        {

                            path = path.Substring(1, path.Length - 2);

                            if (File.Exists(@path))
                                break;
                        }
                        Console.WriteLine("INVALID FILE PATH!");
                    }
                    catch
                    {
                        Console.WriteLine("INVALID FILE PATH!");
                    }
                }
                else
                    break;
            }
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Would you like meta file names? Type either 'y' or 'n'");
                string awnser = Console.ReadLine().ToLower();
                try
                {
                    if (awnser[0] == 'y')
                        metaFileNames = true;
                    else if (awnser[0] == 'n')
                        metaFileNames = false;
                    else
                        throw new Exception();
                    break;
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine("Incorrect input! Try Again!!!");
                }
            }
            Console.Clear();
            fileName = Path.GetFileName(@path);
            //fileName = fileName.Substring(0, fileName.Length - 6);
            fileDirectory = @Path.GetDirectoryName(@path);
            FileStream fs = File.OpenRead(@path);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            fs.Close();
            fs.Dispose();
            ms.Position = 0;
            string buffer = "";
            Console.WriteLine("Getting ID data...");
            Console.WriteLine("Total File Size:" + ms.Length);
            //Tuple<int, int> Pos = Tuple.Create(Console.CursorLeft, Console.CursorTop);
            //Console.CursorVisible = false;
            for(int i = 0; i < ms.Length; i++)
            {
                //This is too slow
                //Console.Write(i);
                //Console.SetCursorPosition(Pos.Item1, Pos.Item2);
                char letter = Convert.ToChar(Convert.ToByte(ms.ReadByte()));
                switch (letter)
                {

                    case '\n':
                        if(buffer.Length > 29)
                        {
                            try
                            {
                                if (buffer.Substring(0, 29) == "<Content name=\"SoundId\"><url>")
                                {
                                    buffer = buffer.Substring(0, buffer.Length - 16);
                                    buffer = buffer.Substring(29, buffer.Length - 29);
                                    URLBank.Add(buffer);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        
                        buffer = "";
                        break;

                    case '\t':
                        break;

                    default:
                        buffer += letter;
                        break;
                }
            }
            ms.Close();
            ms.Dispose();
            
            Console.WriteLine("Parsing ID Data...");
            for(int i = 0; i < URLBank.Count; i++)
            {
                try
                {
                    if (URLBank[i].Substring(0, 13) == "rbxassetid://")
                    {
                        URLBank[i] = URLBank[i].Substring(13, URLBank[i].Length - 13);
                        IDBank.Add(URLBank[i]);
                    }
                    else if (URLBank[i].Substring(0, 32) == "http://www.roblox.com/asset/?id=")
                    {
                        URLBank[i] = URLBank[i].Substring(32, URLBank[i].Length - 32);
                        IDBank.Add(URLBank[i]);
                    }
                }
                catch (Exception)
                {

                }
            }

            Console.WriteLine("Downloading...");

            if (!Directory.Exists(fileDirectory + "\\" + fileName + "_SOUNDS"))
                Directory.CreateDirectory(fileDirectory + "\\" + fileName + "_SOUNDS");

            Tuple<int, int> Pos = Tuple.Create(Console.CursorLeft, Console.CursorTop);
            Console.CursorVisible = false;
            for (int i = 0; i < IDBank.Count; i++)
            {
                Console.Write("DOWNLOADING SOUND: " + i + "-" +"ID: " + IDBank[i] + "          ");
                Console.SetCursorPosition(Pos.Item1, Pos.Item2);
                try
                {
                    string filename = fileDirectory + "\\" + fileName + "_SOUNDS" + "\\" + @downloadAssetInfo(client, IDBank[i], metaFileNames);
                    if (File.Exists(@filename + ".ogg") || File.Exists(@filename + ".wav") || File.Exists(@filename + ".mp3"))
                        continue;
                    client.DownloadFile(@"https://api.hyra.io/audio/" + IDBank[i], @filename + ".DOWNLOADING");
                    while (client.IsBusy)
                    {

                    }
                    FileStream dfs = new FileStream(@filename + ".DOWNLOADING", FileMode.Open, FileAccess.Read);
                    dfs.Position = 0;
                    byte head = Convert.ToByte(dfs.ReadByte());
                    dfs.Close();
                    dfs.Dispose();
                    switch (head)
                    {
                        case 79:
                            File.Move(@filename + ".DOWNLOADING", @filename + ".ogg");
                            break;

                        case 82:
                            File.Move(@filename + ".DOWNLOADING", @filename + ".wav");
                            break;

                        case 73:
                        case 255:
                            File.Move(@filename + ".DOWNLOADING", @filename + ".mp3");
                            break;

                        default:
                            File.Move(@filename + ".DOWNLOADING", @filename + ".UNKNOWN");
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
            Console.WriteLine("\nFinished downloading!\nCheck Folder:" + fileDirectory + "\\" + fileName + "_SOUNDS");
            Console.ReadLine();
        }



        static string downloadAssetInfo(WebClient wc, string ID, bool MetaMode)
        {
            string package = "";
            List<String> stringList = new List<String>();
            string buffer = "";
            stringList.Clear();
            try
            {
                wc.DownloadFile(@"https://www.roblox.com/studio/plugins/info?assetId=" + ID, @Path.GetTempPath() + @"\RobloxAudioArchiver-AssetInfo.html");
                while (wc.IsBusy)
                {

                }
                FileStream fs = File.Open(@Path.GetTempPath() + @"\RobloxAudioArchiver-AssetInfo.html", FileMode.Open);
                fs.Position = 0;
                for (int i = 0; i < fs.Length; i++)
                {
                    char c = Convert.ToChar(Convert.ToByte(fs.ReadByte()));
                    switch (c)
                    {
                        case '\n':
                            if (buffer != "")
                                stringList.Add(buffer);
                            buffer = "";
                            break;

                        case '\t':
                        case ' ':
                            break;

                        default:
                            buffer += c;
                            break;
                    }
                }
                fs.Close();
                fs.Dispose();
                for (int i = 0; i < stringList.Count; i++)
                {
                    try
                    {
                        if (stringList[i].Substring(0, 24) == "<h3class=\"plugin-title\">")
                        {
                            package = stringList[i].Substring(24, stringList[i].Length - 30);
                        }
                        if (MetaMode)
                        {
                            if (stringList[i].Substring(0, 10) == "by<ahref=\"")
                            {
                                string local = stringList[i].Substring(10, stringList[i].Length - 15);
                                for (int x = 0; x < local.Length; x++)
                                {
                                    if (local[x] == '\"')
                                    {
                                        package += "_" + local.Substring(x + 2, local.Length - x - 2);
                                    }
                                }
                            }
                        }
                        
                    }
                    catch (Exception)
                    {

                    }
                }
                if (MetaMode)
                    return ID + "_" + package;
                else
                    return package;
            }
            catch (WebException)
            {
                return null;
            }

        }
    }
}
