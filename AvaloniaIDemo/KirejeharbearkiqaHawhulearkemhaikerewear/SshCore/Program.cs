﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnetCampus.Configurations;
using dotnetCampus.Configurations.Core;

using Renci.SshNet;

namespace SshCore;

internal class Program
{
    static async Task Main(string[] args)
    {
        var currentLine = new StringBuilder();
        bool isSendTab = false;

        var standardInput = Console.OpenStandardInput();



        var file = @"c:\lindexi\CA\ssh.coin";
        var fileConfigurationRepo = ConfigurationFactory.FromFile(file, RepoSyncingBehavior.Sync);
        var appConfigurator = fileConfigurationRepo.CreateAppConfigurator();

        var sshConfiguration = appConfigurator.Of<SshConfiguration>();
        //sshConfiguration.Host = "127.0.0.1";
        //sshConfiguration.UserName = "root";
        //sshConfiguration.Password = "lindexi";

        var processStartInfo = new ProcessStartInfo("ssh", ["-o", "ServerAliveInterval=600",$"{sshConfiguration.UserName}@{sshConfiguration.Host}"]);
        Process.Start(processStartInfo);
        var output = Console.OpenStandardOutput();
        var reader = new StreamReader(output);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
            {
                break;
            }
        }


        var sshClient = new SshClient(sshConfiguration.Host, sshConfiguration.UserName, sshConfiguration.Password);
        await sshClient.ConnectAsync(CancellationToken.None);

        var openStandardInput = standardInput;
        var openStandardOutput = Console.OpenStandardOutput();
        var shell = sshClient.CreateShell(openStandardInput, openStandardOutput, openStandardOutput);
        shell.Start();
        while (true)
        {
            var consoleKeyInfo = Console.ReadKey(true);
            if (consoleKeyInfo.Key == ConsoleKey.Escape)
            {
                break;
            }
        }

        var shellStream = sshClient.CreateShellStream("xxx",(uint) Console.WindowWidth,(uint) Console.WindowHeight, (uint) Console.WindowWidth, (uint) Console.WindowHeight, Console.BufferWidth*Console.BufferHeight);
        shellStream.DataReceived += (sender, e) =>
        {
            var message = e.Line;
            if (string.IsNullOrEmpty(message))
            {
                message = Encoding.UTF8.GetString(e.Data);
            }

            Console.WriteLine($"[SSH] {message}");
        };

       

        while (true)
        {
            var line = Console.ReadLine();

            if (line is null)
            {
                break;
            }

            shellStream.WriteLine(line);
        }

        var streamReader = new StreamReader(shellStream);
        Console.SetIn(streamReader);
        var streamWriter = new StreamWriter(shellStream);
        Console.SetOut(streamWriter);

     

        while (true)
        {
            await Task.Delay(1000);
        }

        await fileConfigurationRepo.SaveAsync();
    }
}

class SshConfiguration : Configuration
{
    public SshConfiguration() : base("")
    {
    }

    public string Host
    {
        get => GetString();
        set => SetValue(value);
    }

    public int Port
    {
        get => GetInt32() ?? 22;
        set => SetValue(value);
    }

    public string UserName
    {
        get => GetString();
        set => SetValue(value);
    }

    public string Password
    {
        get => GetString();
        set => SetValue(value);
    }
}