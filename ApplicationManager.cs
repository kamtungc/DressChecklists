//+---------------------------------------------------------------------------
//
//  File: ApplicationManager.cs
//
//  Purpose:
//    1) Read CommandRules.xml file
//    2) Build required command rules lookup directory
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using DocuSign.DressChecklist.Model;

namespace DocuSign.DressChecklist
{
  public interface IApplicationManager
  {
      Dictionary<int, List<Response>> CommandResponseLookupDictionary { get; }
      void Initialize();
  }

  public class ApplicationManager : IApplicationManager
  {
    private Dictionary<int, List<Response>> commandResponseLookupDictionary;
    private CommandRules commandRules;
    private const string commandRuleFile = "CommandRules.xml";

    public Dictionary<int, List<Response>> CommandResponseLookupDictionary
    {
      get
      {
        return this.commandResponseLookupDictionary;
      }
    }

    public ApplicationManager()
    {
      this.commandResponseLookupDictionary =
        new Dictionary<int, List<Response>>();
    }

    public void Initialize()
    {
      if (string.IsNullOrWhiteSpace(commandRuleFile) ||
          !File.Exists(commandRuleFile))
      {
        throw new InvalidOperationException(
            string.Format(
                "The Command Rule file {0} is not valid.",
                commandRuleFile));
      }

      using (StreamReader sr = new StreamReader(commandRuleFile))
      {
        var serializer = new XmlSerializer(typeof(CommandRules));
        this.commandRules = (CommandRules)serializer.Deserialize(sr);
      }

      InitializeCommandRules();
    }

    private void InitializeCommandRules()
    {
      Dictionary<int, CommandRule> commandRuleIdLookupDictionary =
        new Dictionary<int, CommandRule>();

      foreach (CommandRule rule in this.commandRules.CommandRuleList)
      {
          if (string.IsNullOrWhiteSpace(rule.Name))
          {
              throw new InvalidOperationException(
                  string.Format(
                      "A command rule id \"{0}\" has an empty command name.",
                      rule.Id));
          }

          if (rule.ResponseGroup == null ||
              rule.ResponseGroup.ResponseList == null ||
              rule.ResponseGroup.ResponseList.Count == 0)
          {
              throw new InvalidOperationException(
                  string.Format(
                      "A command rule id \"{0}\" has no response path.",
                      rule.Id));
          }

          try
          {
              // this guards the uniqueness of the rule.Id
              commandRuleIdLookupDictionary.Add(rule.Id, rule);
          }
          catch (Exception ex)
          {
              throw new InvalidOperationException(
                  string.Format(
                      "A command rule id \"{0}\" has already been added: {1}",
                      rule.Id,
                      ex.ToString()));
          }

          foreach (var key in commandRuleIdLookupDictionary.Keys)
          {
            if (!this.commandResponseLookupDictionary.ContainsKey(key))
            {
              this.commandResponseLookupDictionary.Add(key,
                new List<Response>(
                  commandRuleIdLookupDictionary[key]
                  .ResponseGroup.ResponseList));
            }
          }
      }
    }
  }
}