using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class FSWGen : IDisposable {
    
    Channel<System.IO.FileSystemEventArgs> _channel;
    System.IO.FileSystemWatcher _fsw;

    public FSWGen (string directoryPath, string filter)
    {
        _channel = Channel.CreateUnbounded<System.IO.FileSystemEventArgs> (new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true});
        _fsw = new System.IO.FileSystemWatcher(directoryPath, filter);
        _fsw.Changed += OnChanged;
        _fsw.Created += OnChanged;
        _fsw.Deleted += OnChanged;
    }

    private void OnChanged (object sender, System.IO.FileSystemEventArgs eventArgs)
    {
        _channel.Writer.WriteAsync (eventArgs);
    }

    ~FSWGen () => Dispose (false);

    public void Dispose () {
        Dispose (true);
        GC.SuppressFinalize (this);
    }

    public virtual void Dispose (bool disposing)
    {
        if (disposing) {
            _fsw.EnableRaisingEvents = false;
            _fsw.Dispose();

            _channel.Writer.Complete();
            _channel = null;
        }        
    }

    public async IAsyncEnumerable<System.IO.FileSystemEventArgs> Watch ([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try {
            _fsw.EnableRaisingEvents = true;
            var completion = _channel.Reader.Completion.ContinueWith((t) => 0);
            while (true) {
                var readOne = _channel.Reader.ReadAsync(cancellationToken).AsTask();
                Task<int> t = await Task.WhenAny(completion, readOne.ContinueWith((t) => 1)).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                switch (t.Result) {
                    case 0:
                        yield break;
                    case 1:
                        yield return readOne.Result;
                        break;
                    default:
                        throw new InvalidOperationException ("unexpected result from WhenAny");
                }
            }
        } finally {
            var fsw = _fsw;
            if (fsw != null)
                fsw.EnableRaisingEvents = false;
        }
    }  
}