using System;
using System.IO;
using System.Linq;

namespace vhd
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var FS = File.Create(@"D:\Temp\100M.vhd"))
            {
                VHD.CreateVHD(1024 * 1024 * 100, VhdType.FixedDisk, FS, false);
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }

#if DEBUG
        static void MemberwiseCompare<T>(T obj1, T obj2)
        {
            if (obj1 == null)
            {
                throw new ArgumentNullException(nameof(obj1));
            }
            if (obj2 == null)
            {
                throw new ArgumentNullException(nameof(obj2));
            }

            Console.Error.WriteLine("{0,-20}\t{1}\t{2,-20}\t{3,-20} ({4})", "Property name", "Same", "Hash1", "Hash2", "Name");

            foreach (var Prop in obj1.GetType().GetProperties())
            {
                var v1 = Prop.GetValue(obj1);
                var v2 = Prop.GetValue(obj2);

                Console.Error.WriteLine("{0,-20}\t{1}\t{2,-20}\t{3,-20} ({4})", Prop.Name, v1.Equals(v2), v1.GetHashCode(), v2.GetHashCode(), Prop.PropertyType.Name);
            }
        }
#endif
    }
}
