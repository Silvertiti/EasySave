using EasySave.Controller;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            SauvegardeController controller = new SauvegardeController();
            controller.Start(args);
        }
    }
}