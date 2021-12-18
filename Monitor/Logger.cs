using System;
using System.IO;

namespace Monitor
{
    public static class Logger
    {
        static StreamWriter writter;
        public static void Init(string path = "log.txt")
        {
            writter = new StreamWriter(path, true, System.Text.Encoding.Default);
        }
        public static void AddMessage(string message)
        {
            message = DateTime.Now.ToString() + ": " + message;

            Console.WriteLine(message);
            writter.WriteLine(message);
            writter.Flush();
        }
    }
}