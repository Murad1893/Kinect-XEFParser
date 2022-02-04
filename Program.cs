using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;

namespace DepthExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments added.");
            }
            else {
                //Console.WriteLine(args[0]); // the .xef file
                //Console.WriteLine(args[1]); // the output dir for features

                // open up KStudioClient
                KStudioClient client = KStudio.CreateClient();
                client.ConnectToService();
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine(".xef file not found.");
                }
                else {
                    KStudioPlayback playback = client.CreatePlayback(args[0]);

                    string depthDir = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/depth/";
                    string rawdepthDir = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/raw_depth/";
                    string rgbDir = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/rgb/";
                    string alignedDir = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/aligned/";
                    string mapperDir = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/mapper/";
                    float rgb_fps = 0;
                    float depth_fps = 0;
                    float mapper_fps = 0;

                    if (!Directory.Exists(depthDir))
                        Directory.CreateDirectory(depthDir);
                    if (!Directory.Exists(rgbDir))
                        Directory.CreateDirectory(rgbDir);
                    if (!Directory.Exists(alignedDir))
                        Directory.CreateDirectory(alignedDir);
                    if (!Directory.Exists(mapperDir))
                        Directory.CreateDirectory(mapperDir);
                    if (!Directory.Exists(rawdepthDir))
                        Directory.CreateDirectory(rawdepthDir);

                    // start the color depth recorder
                    var rgb_proc = new Process();
                    var depth_proc = new Process();
                    var mapper_proc = new Process();
                    DirectoryInfo dataDir = new DirectoryInfo(Environment.CurrentDirectory); ;
                    rgb_proc.StartInfo.FileName = "Recorder.exe";
                    rgb_proc.StartInfo.Arguments = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + " \"rgb\"";
                    rgb_proc.StartInfo.UseShellExecute = false;
                    rgb_proc.StartInfo.CreateNoWindow = true;

                    depth_proc.StartInfo.FileName = "Recorder.exe";
                    depth_proc.StartInfo.Arguments = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + " \"depth\"";
                    depth_proc.StartInfo.UseShellExecute = false;
                    depth_proc.StartInfo.CreateNoWindow = true;

                    mapper_proc.StartInfo.FileName = "Recorder.exe";
                    mapper_proc.StartInfo.Arguments = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + " \"mapper\"";
                    mapper_proc.StartInfo.UseShellExecute = false;
                    mapper_proc.StartInfo.CreateNoWindow = true;
                    Console.WriteLine(">Processing " + Path.GetFileName(args[0]).Split('.')[0] + ".mp4: ");
                    {
                        Console.WriteLine(">Starting RGB extraction...");
                        rgb_proc.Start();
                        rgb_proc.WaitForInputIdle();

                        Console.WriteLine(">Video started...");

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        playback.Start();

                        while (playback.State != KStudioPlaybackState.Stopped)
                        {

                        }

                        Console.WriteLine(">Video stopped...");


                        rgb_proc.Kill();

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Console.WriteLine("Time taken to extract rgb frames: " + elapsedMs);

                        int fCount = Directory.GetFiles(rgbDir, "*", SearchOption.TopDirectoryOnly).Length;
                        Console.WriteLine("Number of frames: " + fCount);
                        rgb_fps = (float)fCount / (elapsedMs / 1000);
                        Console.WriteLine("FPS: " + rgb_fps);
                        Console.WriteLine("----------------------");
                    }
                    {
                        Console.WriteLine(">Starting Depth extraction...");
                        depth_proc.Start();
                        depth_proc.WaitForInputIdle();

                        Console.WriteLine(">Starting video...");

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        playback.Start();

                        while (playback.State != KStudioPlaybackState.Stopped)
                        {

                        }

                        Console.WriteLine(">Video stopped...");


                        depth_proc.Kill();

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Console.WriteLine("Time taken to extract depth frames: " + elapsedMs);

                        int fCount = Directory.GetFiles(rawdepthDir, "*", SearchOption.TopDirectoryOnly).Length;
                        Console.WriteLine("Number of frames: " + fCount);
                        depth_fps = (float)fCount / (elapsedMs / 1000);
                        Console.WriteLine("FPS: " + depth_fps);
                        Console.WriteLine("----------------------");
                    }
                    {
                        Console.WriteLine(">Starting Mapper extraction...");
                        mapper_proc.Start();
                        mapper_proc.WaitForInputIdle();

                        Console.WriteLine(">Starting video...");

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        playback.Start();

                        while (playback.State != KStudioPlaybackState.Stopped)
                        {

                        }

                        Console.WriteLine(">Video stopped...");


                        mapper_proc.Kill();

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Console.WriteLine("Time taken to extract mapper frames: " + elapsedMs);

                        int fCount = Directory.GetFiles(mapperDir, "*", SearchOption.TopDirectoryOnly).Length;
                        Console.WriteLine("Number of frames: " + fCount);
                        mapper_fps = (float)fCount / (elapsedMs / 1000);
                        Console.WriteLine("FPS: " + mapper_fps);
                        Console.WriteLine("----------------------");
                    }

                    Console.WriteLine(">Aligning frames...");
                    var proc = new Process();
                    proc = new Process();
                    proc.StartInfo.FileName = "rgbdalign.exe";
                    proc.StartInfo.Arguments = args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();
                    proc.Close();
                    Console.WriteLine(">Frames aligned...");

                    // create video from process rgb frames
                    Console.WriteLine(">Creating .mp4 video for RGB");
                    using (Process ffmpegProc = new Process())
                    {
                        ffmpegProc.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "ffmpeg.exe",
                            Arguments =
                                $"-framerate 30 -i \"{args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/rgb/raw_rgb_%05d.bmp" }\" " +
                                $"-c:v libx264 -r 30 -pix_fmt yuv420p -y " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "_rgb.mp4",
                            //$"-r " + fps.ToString() + " -pix_fmt yuv420p -r " + fps.ToString() + " " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + ".mp4 -y",
                            UseShellExecute = false,
                            CreateNoWindow = false, // Set to false to see ffmpeg output (good progress indicator)
                            RedirectStandardInput = true,
                        };

                        ffmpegProc.Start();
                        ffmpegProc.WaitForExit();
                        ffmpegProc.Close();

                    }

                    Console.WriteLine(">Creating .mp4 video for Depth");
                    using (Process ffmpegProc = new Process())
                    {
                        ffmpegProc.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "ffmpeg.exe",
                            Arguments =
                                $"-framerate 30 -i \"{args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/depth/depth_%05d.png" }\" " +
                                $"-c:v libx264 -r 30 -pix_fmt yuv420p -y " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "_depth.mp4",
                            //$"-r " + fps.ToString() + " -pix_fmt yuv420p -r " + fps.ToString() + " " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + ".mp4 -y",
                            UseShellExecute = false,
                            CreateNoWindow = false, // Set to false to see ffmpeg output (good progress indicator)
                            RedirectStandardInput = true,
                        };

                        ffmpegProc.Start();
                        ffmpegProc.WaitForExit();
                        ffmpegProc.Close();

                    }

                    Console.WriteLine(">Creating .mp4 video for Aligned");
                    using (Process ffmpegProc = new Process())
                    {
                        ffmpegProc.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "ffmpeg.exe",
                            Arguments =
                                $"-framerate 30 -i \"{args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/aligned/aligned_rgb_%05d.png" }\" " +
                                $"-c:v libx264 -r 30 -pix_fmt yuv420p -y " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "_aligned.mp4",
                            //$"-r " + fps.ToString() + " -pix_fmt yuv420p -r " + fps.ToString() + " " + args[1] + "/" + Path.GetFileName(args[0]).Split('.')[0] + "/" + Path.GetFileName(args[0]).Split('.')[0] + ".mp4 -y",
                            UseShellExecute = false,
                            CreateNoWindow = false, // Set to false to see ffmpeg output (good progress indicator)
                            RedirectStandardInput = true,
                        };

                        ffmpegProc.Start();
                        ffmpegProc.WaitForExit();
                        ffmpegProc.Close();

                    }

                    Console.WriteLine(">Processing complete.");
                }
            }

                //Console.ReadKey();
            }
    }
}
