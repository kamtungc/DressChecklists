//+---------------------------------------------------------------------------
//
//  File: DressChecklists.cs
//
//  Usage:
//    DressChecklists.exe [InputFile] [OutputFile]
//
//  Build:
//    On a Windows Command-Prompt, run below command:
//      cmd> csc /t:exe DressChecklists.cs ApplicationManager.cs Command.cs
//           Commands.cs CommandRule.cs Parser.cs Validator.cs
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Collections.Generic;

namespace DocuSign.DressChecklist
{
  public class Program
  {
    static void Main(string[] args)
    {
      IEnumerable<string> brokenRules;

      Parser parser = new Parser(args);
      if (!parser.Validate(parser.ArgumentsValidator, out brokenRules))
      {
        foreach (var v in brokenRules)
        {
          Console.WriteLine("ERR: {0}", v);
        }
        Console.WriteLine(
          "Usage: {0} [InputFile] [OutputFile]",
          Assembly.GetExecutingAssembly().GetName().Name);
        return;
      }

      parser.InitialRuleObjects();
      parser.InitialCommandObjects();
      parser.RunCommandObjects();
    }
  }
}
