using System.Diagnostics;
using System.IO;

namespace YMDB.Bot.Utils
{
    public static class FfmpegUtils
    {
        public static Stream ConvertToPCM(string path)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{path}"" -vn -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            return ffmpeg?.StandardOutput.BaseStream;
        }
    }
}