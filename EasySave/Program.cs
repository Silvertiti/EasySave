using EasySave.Controller;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            SauvegardeController controller = new SauvegardeController();
            controller.Start(args);
        }
    }
}