using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SubtitlesParser.Classes;

namespace ZoomTranscriptAnalyzer
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("You must provide a path to a valid .vtt file.");

                return -1;

            }

            var path = args[0];

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("You must provide a path to a valid .vtt file.");
                return -1;
            }

            if (Path.GetExtension(path).ToLower() != ".vtt")
            {
                Console.WriteLine("You must provide a path to a valid .vtt file.");
                return -1;
            }
            var parser = new SubtitlesParser.Classes.Parsers.VttParser();


            Console.WriteLine("Reading File...");

            List<SubtitleItem> transcriptItems = new List<SubtitleItem>();


            using (var fileStream = File.OpenRead(path))
            {
                transcriptItems = parser.ParseStream(fileStream, System.Text.Encoding.Default);
            }

            Console.WriteLine($"Read File --> {transcriptItems.Count} lines");


            var zoomItems = new List<ZoomTranscriptItem>();

            Console.WriteLine($"Translating File");

            transcriptItems.ForEach(x =>

                zoomItems.Add(new ZoomTranscriptItem(x))
            );

            Console.WriteLine($"Translated File --> {zoomItems.Count} lines");

            var groupedZoomItems = zoomItems.GroupBy(x => x.Speaker).ToList();
            
            
            Console.WriteLine("----");
            Console.WriteLine($"Sumarizing as block");
            Console.WriteLine("----");


            var csv = new StringBuilder();
            csv.AppendLine($"Speaker,Total Words,Total Seconds");

            groupedZoomItems.ForEach(x =>
            {
                var totalWords = x.Sum(y => y.Speech.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length);
                var totalMilliseconds = x.Sum(y => y.EndTime - y.StartTime);

                var totalTimeSpan = new TimeSpan(0, 0, 0, 0, totalMilliseconds);
                var minutesAndSeconds = totalTimeSpan.ToString("mm\\:ss");

                Console.WriteLine($"{x.Key} --> Total Words: {totalWords}, Total Time: {minutesAndSeconds}");


                csv.AppendLine($"{x.Key},{totalWords},{totalTimeSpan.TotalSeconds}");
            });

            Console.WriteLine("----");
            Console.WriteLine($"Sumarizing as CSV");
            Console.WriteLine("----");

            Console.WriteLine(csv.ToString());

            Console.WriteLine("----");
            Console.WriteLine("Done.");

            return 1;
        }
    }

    public class ZoomTranscriptItem
    {
        Regex regex = new Regex(@"/\S+\s*:\s*\S+/g");
        public ZoomTranscriptItem(SubtitleItem x)
        {
            StartTime = x.StartTime;
            EndTime = x.EndTime;

            if (x.Lines.Any())
            {

                var stringParts = x.Lines.First().Split(':', StringSplitOptions.None);

                if (stringParts.Length > 1)
                {
                    Speaker = stringParts[0];
                    Speech = stringParts[1];
                }
                else
                {
                    Speech = stringParts[0];
                    Speaker = "No Speaker";
                }


            }
        }

        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public string Speaker { get; set; }
        public string Speech { get; set; }
    }
}
