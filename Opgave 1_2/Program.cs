using System;

namespace Opgave_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var requester = new Requester();
            var consumer = new Consumer();
            
            requester.Run();

            Thread consumerThread = new Thread();
            consumerThread.start();
            consumerThread.run();
            
        }
    }
}