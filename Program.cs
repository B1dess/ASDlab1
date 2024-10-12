using System.Diagnostics;

namespace lr1_sorting;

class Program
{
    static void Main()
    {
        MemoryController.LimitMemoryUsage(512);
        
        string filePath = "numbers.txt";
        string sortedFilePath = "sorted_numbers.txt";
        
        int partSize = 13107200; // 13107200
        int totalNumbers = 268435456; // 268435456

        var timer = new Stopwatch();
        
        // Console.WriteLine("Started generation");
        // timer.Start();
        // GenerateRandomFile(filePath, totalNumbers);
        // timer.Stop();
        // Console.WriteLine($"Finished generation in {timer.Elapsed}");

        Console.WriteLine("First 10 from unsorted:");
        PrintFirstTenNumbers(filePath);
        
        Console.WriteLine("Started sorting");
        timer.Restart();
        MultiPhaseSort(filePath, sortedFilePath, partSize);
        timer.Stop();
        Console.WriteLine($"Finished sorting in {timer.Elapsed}");
        
        Console.WriteLine("First 10 from sorted:");
        PrintFirstTenNumbers(sortedFilePath);
    }

    static void GenerateRandomFile(string filePath, int count)
    {
        Random random = new Random();
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < count; i++)
            {
                writer.WriteLine(random.Next(1, 1_000_000_000)); 
            }
        }
    }

    static void PrintFirstTenNumbers(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(reader.ReadLine());
            }
        }
    }
    
    static void MultiPhaseSort(string inputFilePath, string outputFilePath, int partSize)
    {
        List<string> sortedPartFiles = new List<string>();
        
        var timer = new Stopwatch();
        
        using (StreamReader reader = new StreamReader(inputFilePath))
        {
            int partIndex = 0;
            while (!reader.EndOfStream)
            {
                Console.WriteLine($"Started creating part_{partIndex}");
                timer.Restart();
                List<int> numbers = new List<int>();
                for (int i = 0; i < partSize && !reader.EndOfStream; i++)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line) && int.TryParse(line, out int number))
                    {
                        numbers.Add(number);
                    }
                }
                
                numbers.Sort();

                string partFilePath = $"part_{partIndex}.txt";
                sortedPartFiles.Add(partFilePath);

                using (StreamWriter writer = new StreamWriter(partFilePath))
                {
                    foreach (var number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                }
                timer.Stop();
                Console.WriteLine($"Finished creating part_{partIndex} in {timer.Elapsed}");
                partIndex++;
            }
        }
        
        Console.WriteLine("Started merging parts");
        timer.Restart();
        MergeSortedFiles(sortedPartFiles, outputFilePath);
        timer.Stop();
        Console.WriteLine($"Finished merging parts in {timer.Elapsed}");
        
        foreach (var partFile in sortedPartFiles)
        {
            File.Delete(partFile);
        }
    }
    
    private static void MergeSortedFiles(List<string> partFiles, string outputFile)
    {
        var queue = new PriorityQueue<FileEntry, int>();
        List<StreamReader> readers = new List<StreamReader>();

        try
        {
            foreach (var partFile in partFiles)
            {
                var reader = new StreamReader(partFile);
                readers.Add(reader);
                string line = reader.ReadLine();
                if (line != null)
                {
                    queue.Enqueue(new FileEntry(int.Parse(line), reader), int.Parse(line));
                }
            }

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                while (queue.Count > 0)
                {
                    FileEntry smallest = queue.Dequeue();
                    writer.WriteLine(smallest.Value);
                    string nextLine = smallest.Reader.ReadLine();
                    if (nextLine != null)
                    {
                        queue.Enqueue(new FileEntry(int.Parse(nextLine), smallest.Reader), int.Parse(nextLine));
                    }
                }
            }
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Close();
            }
        }
    }

    private class FileEntry
    {
        public int Value { get; }
        public StreamReader Reader { get; }

        public FileEntry(int value, StreamReader reader)
        {
            Value = value;
            Reader = reader;
        }
    }
}
