using SigniFlowMiddlewareLibrary;

namespace ConsoleUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What is your first nmae?");
            string ? name = Console.ReadLine();

            Console.WriteLine("What is your surname?");
            string ? surname = Console.ReadLine();

            string fullnmae = PersonProcessor.JoinName(name, surname);
            Console.Write(fullnmae);

        }
    }
}
