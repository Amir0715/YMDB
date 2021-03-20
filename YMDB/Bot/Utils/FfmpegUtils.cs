using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace YMDB.Bot.Utils
{
    public static class FfmpegUtils
    {

        private static string _pathToFfmpegWin = "bin/ffmpeg";
        private static string _pathToFfmpegLin = "ffmpeg";

        public static Stream ConvertToPCM(string path)
        {
            var fileName = "";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fileName = _pathToFfmpegWin;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                fileName = _pathToFfmpegLin;
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"-i \"{path}\" -vn -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            return ffmpeg?.StandardOutput.BaseStream;
        }
        
    }
}