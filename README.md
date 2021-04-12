# DynHost

Watches a file and pastes the content in another. 
Requires elevated rights depending on the target. 

Partice i.e.

`DynHostsUpdater -w source.txt -t target.txt`

When a change is made to source.txt, the content is enclosed in markers in target.txt. 

Example source.txt 

```
 This is a sample file
 This is a sample file line 2
 This is a sample file line 3
 This is a sample file line 4
 This is a sample file line 5
 This is a sample file line 6
 This is a sample file line 7
 This is a sample file line 8
 This is a sample file line 9
```

Example target.txt

```
Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
Ut at imperdiet eros. 
Nam commodo nibh eu lectus tristique elementum id et mi. 
Integer at ipsum imperdiet, blandit lectus nec, mollis felis. 
Donec sed tellus tortor. 
```

Source is updated, this results in target.txt:

```
Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
Ut at imperdiet eros. 
Nam commodo nibh eu lectus tristique elementum id et mi. 
Integer at ipsum imperdiet, blandit lectus nec, mollis felis. 
Donec sed tellus tortor. 

# ---- DynHostsStart
 This is a sample file
 This is a sample file line 2
 This is a sample file line 3
 This is a sample file line 4
 This is a sample file line 5
 This is a sample file line 6
 This is a sample file line 7
 This is a sample file line 8
 This is a sample file line 9
# ---- DynHostsEnd
```

This process is repeated until stopped.

## Arguments

`-w <watchFile>` The file to watch

Replaces the value set in the configuration

`-t <targetFile>` The file to write to

Replaces the value set in the configuration

`-ra`

When administrator rights are required to write to targetfile, this switch restarts the application with elevate rights.

## Practices

Use in combination with Docker Desktop For Windows

When deploying add you dns configuration in a file, specified by `DynHostsUpdater` as source.
Set the target of `DynHostsUpdater` to you (local) hosts file. 

`DynHostsUpdater -w source.txt -t "c:\Windows\Drivers\etc\hosts"`

When source is updated by the specific Docker container, source is update as following (i.e.) 

``` 
127.0.0.1   my.dev.local
```

Your hosts file automatically receives the new hosts. 
Each time you restart the docker container, the hosts file is updated.
And then you host is available in your host machine.

## WARRANTEE

*USE THIS PRODUCT AT YOUR OWN RISC*

Bitpatroon is not responsible for problems that occurred due to using this software. 

Always make copies of the original data. 

The code is open source. You can never be changed for using this software.