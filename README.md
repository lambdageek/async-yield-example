# An exercise in using async yield

A simple stream generator `FSWGen` that hooks up to a `FileSystemWatcher` and yields a stream of events.

## Building

```console
dotnet build
```

## Running

```console
dotnet run -- /tmp *.txt
```

And in a separate console,

```console
touch /tmp/a.txt
touch /tmp/b.txt
touch /tmp/a.txt
rm /tmp/{a,b}.txt
```

Then hit *Ctrl-C* to terminate the watcher.
