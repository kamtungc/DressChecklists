//+---------------------------------------------------------------------------
//
//  File: Command.cs
//
//  Purpose:
//    1) Declare Command objects
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DocuSign.DressChecklist.Validator;

namespace DocuSign.DressChecklist
{
  public enum TemperatureType
  {
      None,
      HOT = 1,
      COLD = 2
  }

  public interface ICommandEnumerable
  {
      ICommandIterator GetIterator();
  }

  public interface ICommand
  {
      int Id { get; set; }
      TemperatureType Type { get; set; }
      List<int> RequiredItems { get; set; }
      string Response { get; set; }
      bool IsError { get; set; }
  }

  public interface ICommandIterator
  {
      void MoveFirst();
      void Reset();
      bool MoveNext();
      Dictionary<int,int> ProcessedIds { get; }
      Command Current { get; }
      Command First { get; }
      bool Validate(
        IValidator<ICommandIterator> validator,
        out IEnumerable<string> brokenRules);
  }

  public class Command : ICommand, IValidatable<Command>
  {
      public int Id { get; set; }
      public TemperatureType Type { get; set; }
      public List<int> RequiredItems { get; set; }
      public string Response { get; set; }
      public bool IsError { get; set; }
      public bool Validate(
        IValidator<Command> validator,
        out IEnumerable<string> brokenRules)
      {
        brokenRules = validator.BrokenRules(this);
        return validator.IsValid(this);
      }
  }

  public class CommandAggregate :
    ICommandEnumerable, IValidatable<CommandAggregate>
  {
      private List<Command> _commands = new List<Command>();

      public Command this[int index]
      {
          get { return _commands[index]; }
      }

      public ICommandIterator GetIterator()
      {
          return new CommandEnumerator(this);
      }

      public int Count
      {
          get { return _commands.Count; }
      }

      public void Add(Command command)
      {
          _commands.Add(command);
      }

      public bool Validate(
        IValidator<CommandAggregate> validator,
        out IEnumerable<string> brokenRules)
      {
        brokenRules = validator.BrokenRules(this);
        return validator.IsValid(this);
      }
  }

  public class CommandEnumerator : ICommandIterator
  {
      private CommandAggregate _aggregate;
      int _position;
      private Dictionary<int,int> _processedIds;

      public CommandEnumerator(CommandAggregate aggregate)
      {
          _aggregate = aggregate;
          _position = -1;
          _processedIds = new Dictionary<int,int>();
      }

      public void MoveFirst()
      {
          if (0 == _aggregate.Count)
          {
              throw new InvalidOperationException();
          }
          _position = 0;
      }

      public void Reset()
      {
          _processedIds = new Dictionary<int,int>();
          _position = -1;
      }

      public bool MoveNext()
      {
          _position++;
          bool res = _position < _aggregate.Count;
          if (res)
          {
            Command cur = Current;
            if (!_processedIds.ContainsKey(cur.Id))
            {
              _processedIds.Add(cur.Id, 1);
            }
            else
            {
              _processedIds[cur.Id]++;
            }
          }
          return res;
      }

      public Command Current
      {
          get
          {
              if (_position < _aggregate.Count)
              {
                  return _aggregate[_position];
              }
              throw new InvalidOperationException();
          }
      }

      public Command First
      {
          get
          {
            if (0 == _aggregate.Count)
            {
                throw new InvalidOperationException();
            }
            return _aggregate[0];
          }
      }

      public Dictionary<int,int> ProcessedIds
      {
          get
          {
              return _processedIds;
          }
      }

      public bool Validate(
        IValidator<ICommandIterator> validator,
        out IEnumerable<string> brokenRules)
      {
        brokenRules = validator.BrokenRules(this);
        return validator.IsValid(this);
      }
  }
}