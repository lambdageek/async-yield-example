using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace async_yield
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var cancel = new CancellationTokenSource(); 
            Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, args) => {
                Console.WriteLine ("Ctrl-C pressed");
                cancel.Cancel();
                args.Cancel = true;
            });

            string directoryPath = args[0];
            string filter = args.Length > 1 ? args[1] : "*";

            Console.WriteLine ($"watching {directoryPath} for {filter}");
            
            try {
                using var fsw = new FSWGen (directoryPath, filter);

                await foreach (var u in fsw.Watch ().WithCancellation(cancel.Token)) {
                    Console.WriteLine ($"{u.ChangeType} {u.Name}");
                }
            } catch (OperationCanceledException) {Console.WriteLine ("cancelled!");}
            return 0;
        }

    }
}
