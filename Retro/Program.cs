﻿using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Newtonsoft.Json;
using Retro.Dto;

namespace Retro
{
  internal class Program
  {
    private static Options _options = new Options();
    private static readonly string _retroFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "retro.json");

    private static int Main(string[] args)
    {
      return Parser.Default.ParseArguments<Options, DumpOptions, ClearOptions>(args)
        .MapResult(
          (Options opts) => RunAddAndReturnExitCode(opts),
          (DumpOptions opts) => RunDumpAndReturnExitCode(),
          (ClearOptions opts) => RunClearAndReturnExitCode(),
          errs => 1);
    }

    private static int RunAddAndReturnExitCode(Options opts)
    {
      _options = opts;
      if (_options.Kudos && string.IsNullOrEmpty(_options.KudosTarget))
      {
        Console.WriteLine("Kudos needs to be addressed");
        return -1;
      }

      var retro = LoadRetroObject();
      AddNewRecord(retro);
      SaveRetroObject(retro);

      return 0;
    }

    private static int RunDumpAndReturnExitCode()
    {
      var retro = LoadRetroObject();

      Console.WriteLine("================== Retro points ==================");
      Console.WriteLine("Positive points:");
      DumpRecords(retro.PositiveExperiences);
      Console.WriteLine();

      Console.WriteLine("Negative points:");
      DumpRecords(retro.NegativeExperiences);
      Console.WriteLine();

      Console.WriteLine("Kudos:");
      DumpRecords(retro.Kudos);

      return 0;
    }

    private static void DumpRecords(IEnumerable<Record> records)
    {
      foreach (var @record in records)
      {
        Console.WriteLine(record.ToString());
      }
    }

    private static int RunClearAndReturnExitCode()
    {
      if (File.Exists(_retroFileFullName))
      {
        File.Delete(_retroFileFullName);
      }

      return 0;
    }

    private static Dto.Retro LoadRetroObject()
    {
      if (File.Exists(_retroFileFullName))
      {
        return JsonConvert.DeserializeObject<Dto.Retro>(File.ReadAllText(_retroFileFullName));
      }

      return new Dto.Retro();
    }

    private static void AddNewRecord(Dto.Retro retro)
    {
      var newRecord = CreateRecord();
      switch (newRecord)
      {
        case Kudos kudos:
          retro.Kudos.Add(kudos);
          return;
        case NegativeRecord negativeRecord:
          retro.NegativeExperiences.Add(negativeRecord);
          return;
        case PositiveRecord positiveRecord:
          retro.PositiveExperiences.Add(positiveRecord);
          return;
      }

      throw new ArgumentOutOfRangeException("Unknown record type");
    }

    private static Record CreateRecord()
    {
      var description = _options.Description;
      var date = DateTime.Now;

      if (_options.Positive)
      {
        return new PositiveRecord(description, date);
      }

      if (_options.Negative)
      {
        return new NegativeRecord(description, date);
      }

      if (_options.Kudos)
      {
        return new Kudos(_options.KudosTarget, description, date);
      }

      throw new ArgumentOutOfRangeException("Unknow type of record");
    }

    private static void SaveRetroObject(Dto.Retro retro)
    {
      var serializedObject = JsonConvert.SerializeObject(retro, Formatting.Indented);
      File.WriteAllText(_retroFileFullName, serializedObject);
    }
  }
}
