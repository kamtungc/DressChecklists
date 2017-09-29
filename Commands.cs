//+---------------------------------------------------------------------------
//
//  File: Commands.cs
//
//  Purpose:
//    1) Declare Commands Iterator objects
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DocuSign.DressChecklist
{
  public interface ICommandsEnumerable
  {
      ICommandsIterator GetIterator();
  }

  public interface ICommandsIterator
  {
    void MoveFirst();
    void Reset();
    bool MoveNext();
    CommandAggregate Current { get; }
  }

  public class CommandsAggregate : ICommandsEnumerable
  {
      private List<CommandAggregate> _commands = new List<CommandAggregate>();

      public CommandAggregate this[int index]
      {
          get { return _commands[index]; }
      }

      public ICommandsIterator GetIterator()
      {
          return new CommandsEnumerator(this);
      }

      public int Count
      {
          get { return _commands.Count; }
      }

      public void Add(CommandAggregate command)
      {
          _commands.Add(command);
      }
  }

  public class CommandsEnumerator : ICommandsIterator
  {
      private CommandsAggregate _aggregate;
      int _position;

      public CommandsEnumerator(CommandsAggregate aggregate)
      {
          _aggregate = aggregate;
          _position = -1;
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
          _position = -1;
      }

      public bool MoveNext()
      {
          _position++;
          return _position < _aggregate.Count;
      }

      public CommandAggregate Current
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
  }
}