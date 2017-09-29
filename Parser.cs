//+---------------------------------------------------------------------------
//
//  File: Parser.cs
//
//  Purpose:
//    1) Parse command lines arguemts
//    2) Initial required objects
//    3) Verify commands
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DocuSign.DressChecklist.Model;
using DocuSign.DressChecklist.Validator;

namespace DocuSign.DressChecklist
{
  public interface IParser
  {
      Dictionary<int, List<Response>> CommandResponses { get; }
      CommandsAggregate Commands { get; }
      IValidator<IParser> ParserValidator { get; }
      IValidator<ICommandIterator> CommandValidator { get; }
      IValidator<IParser> ArgumentsValidator { get; }
      string CurrentLine { get; }
      string[] Arguments { get; }
      bool Validate(
        IValidator<IParser> validator, out IEnumerable<string> brokenRules);
  }

  public class Parser : IParser, IValidatable<IParser>
  {
    private ApplicationManager applicationManager;

    private CommandsAggregate commands;

    private IValidator<IParser> parserValidator;

    private IValidator<IParser> argumentsValidator;

    private IValidator<ICommandIterator> commandValidator;

    private string currentLine;

    private string[] arguments;

    public Dictionary<int, List<Response>> CommandResponses
    {
      get
      {
        return this.applicationManager.CommandResponseLookupDictionary;
      }
    }

    public CommandsAggregate Commands
    {
      get
      {
        return this.commands;
      }
    }

    public string CurrentLine
    {
      get
      {
        return this.currentLine;
      }
    }

    public string[] Arguments
    {
      get
      {
        return this.arguments;
      }
    }

    public IValidator<IParser> ParserValidator
    {
      get
      {
        return this.parserValidator;
      }
    }

    public IValidator<IParser> ArgumentsValidator
    {
      get
      {
        return this.argumentsValidator;
      }
    }

    public IValidator<ICommandIterator> CommandValidator
    {
      get
      {
        return this.commandValidator;
      }
    }

    public Parser(string[] arguments)
    {
      this.applicationManager = new ApplicationManager();
      this.commands = new CommandsAggregate();
      this.parserValidator = new CommandLineValidator<IParser>();
      this.argumentsValidator = new ArgumentsValidator<IParser>();
      this.commandValidator = new CommandValidator<ICommandIterator>();
      this.currentLine = string.Empty;
      this.arguments = arguments;
    }

    public void InitialRuleObjects()
    {
      this.applicationManager.Initialize();
    }

    public void InitialCommandObjects()
    {
      IEnumerable<string> brokenRules;
      if (!this.Validate(ArgumentsValidator, out brokenRules))
      {
        throw new InvalidOperationException(
          string.Format("The given files are not valid: {0}",
            string.Join(";", brokenRules)));
      }
      string inputFile = Arguments[0];
      //string outputFile = Arguments[1];
      using (FileStream sr = new FileStream(inputFile, FileMode.Open))
      //using (StreamWriter writer = new StreamWriter(outputFile))
      using (StreamReader reader = new StreamReader(sr))
      {
        this.currentLine = reader.ReadLine();
        while (!string.IsNullOrEmpty(this.currentLine))
        {
          if (!this.Validate(ParserValidator, out brokenRules))
          {
            foreach (var v in brokenRules)
            {
              //writer.WriteLine("ERR: {0}", v);
              Console.WriteLine("ERR: {0}", v);
            }
            this.currentLine = reader.ReadLine();
            continue;
          }

          string[] pair = this.currentLine.Split(' ');
          CommandAggregate cmd = new CommandAggregate();
          TemperatureType type = ParseEnum<TemperatureType>(pair[0]);
          cmd.Add(new Command(){ Type = type });

          string[] val = pair[1].Split(',');
          foreach (var v in val)
          {
            int id = StringToInt32(v);
            var response = CommandResponses[id]
              .FirstOrDefault(r => r.Type == type);
            cmd.Add(new Command() {
                Id = id,
                Type = type,
                Response = response.Message,
                IsError = response.Error,
                RequiredItems = (null != response.RequiredGroup) ?
                  new List<int>(response.RequiredGroup.RequiredList) :
                  new List<int>() });
          }
          this.commands.Add(cmd);
          this.currentLine = reader.ReadLine();
        }
      }
    }

    public void RunCommandObjects()
    {
      IEnumerable<string> brokenRules;
      if (!this.Validate(ArgumentsValidator, out brokenRules))
      {
        throw new InvalidOperationException(
          string.Format("The given files are not valid: {0}",
            string.Join(";", brokenRules)));
      }
      string outputFile = Arguments[1];

      ICommandsIterator cmds = this.commands.GetIterator();

      cmds.Reset();

      using (StreamWriter writer = new StreamWriter(outputFile))
      {
        while (cmds.MoveNext())
        {
            ICommandIterator cmd = cmds.Current.GetIterator();

            cmd.Reset();
            cmd.MoveFirst();

            List<string> res = new List<string>();
            while (cmd.MoveNext())
            {
              bool isValid = cmd.Validate(CommandValidator, out brokenRules);
              if (!isValid)
              {
                foreach (var v in brokenRules)
                {
                  //writer.WriteLine("ERR: {0}", v);
                  Console.WriteLine("ERR: {0}", v);
                }
                res.Add("false");
                break;
              }
              else
              {
                res.Add(cmd.Current.Response);
              }
            }
            writer.WriteLine(string.Join(",", res));
        }
      }
    }

    public bool Validate(
      IValidator<IParser> validator,
      out IEnumerable<string> brokenRules)
    {
      brokenRules = validator.BrokenRules(this);
      return validator.IsValid(this);
    }

    private T ParseEnum<T>(string parse)
    {
      T val = (T)Enum.Parse(typeof(T), parse, true);
      return val;
    }

    private int StringToInt32(string value)
    {
        try
        {
            if (null == value)
            {
                return 0;
            }
            int result = 0;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        catch
        {
            return 0;
        }
    }
  }
}