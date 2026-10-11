using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DinnerPhilisohy2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Run(SolutionType.Naive, " 1. Наивная версия (Deadlock)");
            Run(SolutionType.OrderedResources, "2. Упорядочевание ресурсов");
            Run(SolutionType.TryEnter, "3. Monitor.TryEnter (с отступами)");
            Run(SolutionType.Waiter, "4. \"Официант\" (4 философа)");       

        }
        static void Run(SolutionType type, string description)
        {
            Console.WriteLine($"\n--- {description} ---");
            Philosophy.ResetStats();

            var stopwatch =Stopwatch.StartNew();

            object[] forks = new object[5];
            for (int i = 0; i < 5; i++) forks[i] = new object();

            Philosophy[] philosophers =
            {
                new Philosophy("Аристотель", forks[0], forks[1], 0, 1),
                new Philosophy("Платон", forks[1], forks[2], 1, 2),
                new Philosophy("Сократ", forks[2], forks[3], 2, 3),
                new Philosophy("Конфуций", forks[3], forks[4], 3, 4),
                new Philosophy("Диоген", forks[4], forks[0], 4, 0)
            };

            Console.WriteLine("\n====== ОБЕД ФИЛОСОФОВ ======");
            Console.WriteLine();

            Thread[] threads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                int idx = i; 
                threads[idx] = new Thread(() => philosophers[idx].Start(type));
                threads[idx].Start();
            }
            
            Thread watchdog = new Thread(() =>
            {
                bool completed = false;
                for (int t = 0; t < 10000; t += 500)
                {
                    Thread.Sleep(500);
                    
                    bool allAlive = false;
                    foreach (var th in threads) allAlive |= th.IsAlive;

                    if (!allAlive)
                    {
                        completed = true;
                        break;
                    }
                }

                if (!completed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n Зависание (Deadlock) (остановка прграммы)");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Для продолжения нужно закомментировать строку: (\"Run(SolutionType.Naive\")");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            });
            watchdog.IsBackground = true;
            watchdog.Start();

            foreach (var t in threads) t.Join();
            stopwatch.Stop();
            
            Console.WriteLine("ФИЛОСОФЫ НАЕЛИСЬ");           
            Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Общее кол-во приемов пищи: {Philosophy.GetTotalStatistic()}");           

            if (type == SolutionType.TryEnter)
            {
                Console.WriteLine($"Кол-во отступлений (TryEnter): {Philosophy.GetRetreats()}");
            }
            Console.WriteLine("-------------------------------");
        }
    }
}

