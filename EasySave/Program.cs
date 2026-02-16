using EasySave.Controller;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            SauvegardeView controller = new SauvegardeView();
            controller.Start(args);
        }
    }
}