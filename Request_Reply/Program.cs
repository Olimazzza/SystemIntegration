using System;

namespace RequestReplyPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            
                var requester = new Requester();
                var replier = new Replier();

                Thread replierThread = new Thread(replier.Run);
                
                replierThread.Start();
                
                requester.Run();

        }
    }
}