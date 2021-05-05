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

# ---- DynHostsStart 26966DC52E85AF40F59B4FE73D8C323A
 This is a sample file
 This is a sample file line 2
 This is a sample file line 3
 This is a sample file line 4
 This is a sample file line 5
 This is a sample file line 6
 This is a sample file line 7
 This is a sample file line 8
 This is a sample file line 9
# ---- DynHostsEnd 26966DC52E85AF40F59B4FE73D8C323A
```

This process is repeated until stopped.

## Arguments

`-w <watchFileList>` The file or files list (; separated)  to watch;<br />
Notice paths can be written with path separator backslash (\\) or forward slash (/). 
Each watchfile must be specified with an absolute path. 

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

## Running as service

To silently perform the operations, store the binary and the configuration in any folder on your system. 
(see /bin)

1. Install the service:<br />
``` DynHostsUpdater.exe install ```

2. Configure the configuration

3. Start the service
``` DynHostsUpdater.exe start ```<br />
(or go to the services management console, search for the service and start it.)

## Removing the service
1. Stopping<br />
``` DynHostsUpdater.exe stop ```

2. Removing<br />
``` DynHostsUpdater.exe uninstall ```

3. Service not yet gone?<br />
Reboot your system.

## Multiple files to watch
If you have multiple files to watch, change the watchfile into a semicolon separated list of files.

i.e.

c:\source.txt;c:\temp\myhosts.txt;

By setting this, the following files will be watched.
- c:\source.txt 
- c:\temp\myhosts.txt

If either of both is changed, the setting for that file is updated.

## Configuration file

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
            <section name="DynHosts.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" allowExeDefinition="MachineToLocalUser" requirePermission="false" />
        </sectionGroup>
    </configSections>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
    </startup>
    <userSettings>
        <DynHosts.Properties.Settings>
            <setting name="watchFile" serializeAs="String">
                <value>c:\source.txt;c:\temp\myhosts.txt</value>
            </setting>
            <setting name="commentPrefix" serializeAs="String">
                <value>#</value>
            </setting>
            <setting name="targetFile" serializeAs="String">
                <value>d:/temp/hosts</value>
            </setting>
            <setting name="logWithFullDateTime" serializeAs="String">
                <value>True</value>
            </setting>
        </DynHosts.Properties.Settings>
    </userSettings>
</configuration>
```


## WARRANTEE

*USE THIS PRODUCT AT YOUR OWN RISC*

Bitpatroon is not responsible for problems that occurred due to using this software. 

Always make copies of the original data. 

The code is open source. You can never be charged for using this software.