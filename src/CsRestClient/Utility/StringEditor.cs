using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// StringEditor.cs
// https://github.com/pjc0247/StringEditor.cs
namespace CsRestClient.Utility
{
    class StringEditCommand
    {
        public int index { get; set; }
        public int length { get; set; }

        public string replaceTo { get; set; }

        public StringEditCommand(int index, int length, string replaceTo)
        {
            this.index = index;
            this.length = length;
            this.replaceTo = replaceTo;
        }
    }
    class StringEditor
    {
        private string original { get; set; }
        private StringBuilder sb { get; set; }
        private List<StringEditCommand> commands { get; set; }

        public StringEditor(string input)
        {
            sb = new StringBuilder(input);
            original = input;
            commands = new List<StringEditCommand>();
        }

        public char this[int idx]
        {
            get
            {
                return sb[idx];
            }
        }

        public StringEditor Replace(
            string old, string to,
            StringComparison comp = StringComparison.CurrentCulture)
        {
            for (int i = 0; i < original.Length; i++)
            {
                var foundAt = original.IndexOf(old, i, comp);
                if (foundAt == -1)
                    break;

                commands.Add(
                    new StringEditCommand(foundAt, old.Length, to));
                i = foundAt + old.Length;
            }

            return this;
        }

        public string Commit()
        {
            var dirty = new List<bool>(Enumerable.Repeat<bool>(false, sb.Length));
            var offset = 0;

            commands = commands.OrderBy(x => x.index).ToList();

            foreach (var command in commands)
            {
                int begin = command.index + offset;

                if (dirty.GetRange(begin,
                        Math.Min(sb.Length, command.replaceTo.Length + begin) - begin).Any(x => x))
                    continue;

                // exp
                if (command.replaceTo.Length > command.length)
                {
                    sb.Insert(begin, new string(' ', command.replaceTo.Length - command.length));
                    dirty.InsertRange(begin, Enumerable.Repeat<bool>(false, command.replaceTo.Length - command.length));
                    offset += command.replaceTo.Length - command.length;
                }
                // shrink
                else if (command.replaceTo.Length < command.length)
                {
                    sb.Remove(begin, command.length - command.replaceTo.Length);
                    dirty.RemoveRange(begin, command.length - command.replaceTo.Length);
                    offset -= command.length - command.replaceTo.Length;
                }

                int to = Math.Min(sb.Length, command.replaceTo.Length + begin);

                for (int i = begin; i < to; i++)
                {
                    sb[i] = command.replaceTo[i - begin];
                    dirty[i] = true;
                }
            }

            commands.Clear();
            return sb.ToString();
        }

        public override string ToString()
        {
            return Commit();
        }
    }
}
