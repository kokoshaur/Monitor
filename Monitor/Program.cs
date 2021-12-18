using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
//Гринченко Сергей
namespace Monitor
{
    class Program
    {
        struct ProcessInfo  //Вся нужная информация о процессе
        {
            public string name;
            public int maxSizeLife; //Допустимое время жизни 
            public int nowSizeLife; //Текущее время жизни
            public int refreshTime; //Частота проверки

            public int IncreaseLife()
            {
                nowSizeLife += refreshTime;
                return nowSizeLife;
            }
        }

        static void Main(string[] args)
        {
            Logger.Init();
            if (args.Length < 3)
            {
                Logger.AddMessage("Incorrect start, not enough arguments");
                return;
            }

            int refreshTime; 
            ProcessInfo[] processes = new ProcessInfo[1];   //Сделаем массивом, если понадобится указать сразу много разных процессов

            try
            {
                refreshTime = Convert.ToInt32(args[2]);

                processes[0] = new ProcessInfo()
                {
                    name = args[0],
                    maxSizeLife = Convert.ToInt32(args[1]),
                    nowSizeLife = 0,
                    refreshTime = refreshTime,
                };
                Logger.AddMessage("Successful start with " + args[0] + " " + args[1] + " " + args[2]);
            }
            catch
            {
                Logger.AddMessage("Incorrect start, incorrect arguments");
                return;
            }


            while (true)
            {
                Thread.Sleep(refreshTime * 60 * 1000);          //Ждём заданное кол-во минут

                Task.Run(() => { ChechProcesses(ref processes); }); //Проверку процессов следует делать в отдельном потоке, так как
                                                                    //мы не знаем, сколько времени потребуется на исполнение этой функции.
                                                                    //Даже одной миллисекунды хватит, чтобы начать накапливать ошибку
            }
        }

        static void ChechProcesses(ref ProcessInfo[] processes)
        {
            for (int i = 0; i < processes.Length; i++)
            {
                if (IsLive(processes[i].name))  //Если процесс(ы) с таким именем есть -> инкремируем время жизни этого процесса
                {
                    if (processes[i].IncreaseLife() >= processes[i].maxSizeLife)//Если время жизни сравнялось или превысило допустимое -> убиваем
                    {
                        processes[i].nowSizeLife = 0;   //Не забываем обнулить временя жизни процесса(ов)
                        KillProcess(processes[i].name);
                    }
                }
                else
                    processes[i].nowSizeLife = 0;   //Если процесс(ы) сам(и) куда-то пропал(и) -> обнуляем время жизни
            }
        }

        static bool IsLive(in string name)  //Проверяет наличие процесса(ов) с указанным именем
        {
            if (Process.GetProcessesByName(name).Length == 0)
                return false;
            else
                return true;
        }

        static void KillProcess(in string name) //Убивает процесс(ы) с указанным именем
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                try
                {
                    process.Kill();
                    Logger.AddMessage("Process " + name + " with id = " + process.Id + " was killed");  //Логгируем
                }
                catch   //Системные процессы вырубает, но касперского не осилил -> запишем это в лог
                {
                    Logger.AddMessage("Process " + name + " killing error, not enough rights");
                }
            }
        }
    }
}