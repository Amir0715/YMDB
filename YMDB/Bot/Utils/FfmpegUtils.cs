using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace YMDB.Bot.Utils
{
    public static class FfmpegUtils
    {
        private const string PathToFfmpegWin = "bin/ffmpeg";
        private const string PathToFfmpegLin = "ffmpeg";

        public static Stream ConvertToPcm(string path)
        {
            var fileName = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fileName = PathToFfmpegWin;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                fileName = PathToFfmpegLin;
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