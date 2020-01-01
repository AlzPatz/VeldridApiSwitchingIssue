using System;

namespace ApiSwitchingIssue
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demo to demonstrate potential problems switching Backend APIs, in particular going 'back' to Veldrid after OpenGL");

            var demo = new Demo();

            demo.Run();
        }
    }
}
