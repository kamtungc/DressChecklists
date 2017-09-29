//+---------------------------------------------------------------------------
//
//  File: Validator.cs
//
//  Purpose:
//    1) Valid all required classes and commands
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DocuSign.DressChecklist.Validator
{
  public interface IValidator<T>
  {
      bool IsValid(T entity);
      IEnumerable<string> BrokenRules(T entity);
  }

  public interface IValidatable<T>
  {
      bool Validate(
        IValidator<T> validators, out IEnumerable<string> brokenRules);
  }

  public class ArgumentsValidator<T> : IValidator<T>
  {
      public bool IsValid(T entity)
      {
          return BrokenRules(entity).Count() == 0;
      }

      public IEnumerable<string> BrokenRules(T entity)
      {
          IParser val = entity as IParser;
          if (null == val)
          {
            yield return "Entry is not IParser.";
            yield break;
          }
          if (null == val.Arguments || val.Arguments.Length < 2)
          {
              yield return "Number of arguments is not valid.";
              yield break;
          }
          string inputFile = val.Arguments[0];
          string outputFile = val.Arguments[1];
          if (string.IsNullOrWhiteSpace(inputFile) ||
              !File.Exists(inputFile))
          {
              yield return string.Format("The input file {0} is not valid.",
                inputFile);
              yield break;
          }
          if (string.IsNullOrWhiteSpace(outputFile))
          {
              yield return string.Format("The output file {0} is not valid.",
                outputFile);
              yield break;
          }
          yield break;
      }
  }

  public class CommandValidator<T> : IValidator<T>
  {
      public bool IsValid(T entity)
      {
          return BrokenRules(entity).Count() == 0;
      }

      public IEnumerable<string> BrokenRules(T entity)
      {
          ICommandIterator val = entity as ICommandIterator;
          if (null == val)
          {
            yield return "Entry is not ICommandIterator.";
            yield break;
          }

          if (null != val.Current.RequiredItems)
          {
            foreach (var v in val.Current.RequiredItems)
            {

              if (!val.ProcessedIds.ContainsKey(v))
              {
                yield return string.Format(
                  "Required item ({0}) cannot be found.", v);
                yield break;
              }
            }
          }

          if (val.ProcessedIds.ContainsKey(val.Current.Id) &&
              val.ProcessedIds[val.Current.Id] > 1)
          {
            yield return string.Format(
              "Current item ({0}) has been used before.", val.Current.Id);
            yield break;
          }
          if (val.Current.IsError)
          {
            yield return string.Format(
              "Current item ({0}) is not allowed.", val.Current.Id);
          }

          yield break;
      }
  }

  public class CommandLineValidator<T> : IValidator<T>
  {
      public bool IsValid(T entity)
      {
          return BrokenRules(entity).Count() == 0;
      }

      public IEnumerable<string> BrokenRules(T entity)
      {
          IParser val = entity as IParser;
          if (null == val)
          {
            yield return "Entry is not IParser.";
            yield break;
          }
          if (string.IsNullOrWhiteSpace(val.CurrentLine))
          {
            yield return "String is empty or null.";
            yield break;
          }
          string[] pair = val.CurrentLine.Split(' ');
          if (null == pair || pair.Length < 2)
          {
            yield return "Number of commands is not valid.";
            yield break;
          }
          string type = pair[0];
          string cmds = pair[1];
          bool hasError = false;
          try
          {
            TemperatureType ttype;
            if (!Enum.TryParse(type, out ttype))
            {
              hasError = true;
            }
          }
          catch (Exception)
          {
            hasError = true;
          }
          if (hasError)
          {
            yield return "Head is not a vaild type.";
            yield break;
          }
          string[] vals = cmds.Split(',');
          if (null == vals || 0 == vals.Length)
          {
            yield return "No of command was found.";
            yield break;
          }
          foreach (var v in vals)
          {
            int number = 0;
            if (!Int32.TryParse(v, out number))
            {
              yield return "Entry is not a vaild number.";
              yield break;
            }
            if (!val.CommandResponses.ContainsKey(number))
            {
              yield return string.Format(
                "Command id {0} is not a vaild.", number);
              yield break;
            }
          }
          yield break;
      }
  }
}