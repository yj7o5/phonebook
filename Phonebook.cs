using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Phonebook
{
    public record Entry(string Name, string Number, string Type);

    /*
     * Rules:
     *  - Cannot maintain any state in the class itself
     *  - IterateOrderedByName() should order by name after a given name
     */
    public class PhonebookPartOne
    {
        private string filename;

        public const string Format = "{0},{1},{2}";

        public const int Record_SizeInBytes = 64;

        public PhonebookPartOne(string filename)
        {
            this.filename = filename;
        }

        // Cannot re-write the whole file
        public void InsertOrUpdate(Entry entry)
        {
            using var file = File.OpenWrite(this.filename);

            using var writer = new BinaryWriter(file);

            WriteEntry(writer, entry);
        }

        private void WriteEntry(BinaryWriter writer, Entry entry)
        {
            var raw = string.Format(Format, entry.Name, entry.Number, entry.Type).PadRight(Record_SizeInBytes);

            writer.Write(raw);
        }

        // Should be faster than O(N)
        public Entry GetByName(string name)
        {
            using var file = File.OpenRead(this.filename);

            // Naive implementation ~ O(N)
            var count = file.Length / Record_SizeInBytes;
            var buffer = new byte[Record_SizeInBytes];

            // Read into buffer
            var offset = 0;
            while ((offset += file.Read(buffer, offset, Record_SizeInBytes)) < file.Length)
            {
                var raw = Encoding.UTF8.GetString(buffer);
                var split = raw.Split(",");

                if (Equals(split[0], name))
                {
                    return new Entry(split[0], split[1], split[2].TrimEnd());
                }
            }

            return default;
        }

        // Should stream results, not store in memory
        public IEnumerable<Entry> IterateOrderedByName(string afterName = null)
        {
            using var file = File.OpenRead(this.filename);

            // Naive implementation ~ O(N)
            var count = file.Length / Record_SizeInBytes;
            var buffer = new byte[Record_SizeInBytes];

            var stream = false;

            // Read into buffer
            var offset = 0;
            while ((offset += file.Read(buffer, offset, Record_SizeInBytes)) < file.Length)
            {
                var raw = Encoding.UTF8.GetString(buffer);
                var split = raw.Split(",");

                if (stream)
                {
                    yield return new Entry(split[0], split[1], split[2].TrimEnd());
                }

                if (Equals(split[0], afterName))
                {
                    stream = true;
                }
            }
        }
    }
}
