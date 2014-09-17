using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using PatchingEventSourcing.ValueTypes;

namespace PatchingEventSourcing.Tests {
    [TestFixture]
    public class ObjectBuilderTests {
        [Test]
        public void Simple() {
            var builder = new ObjectBuilder<User>(new TypeTreeCache(new TypeTreeBuilder()), AllValueTypesProvider.ValueTypes);

            var patches = new List<Patch>
            {
                new Patch() {Operation = "replace", Path = "/Name", Value = "Niclas"},
           //     new Patch() {Operation = "replace", Path = "/Address/Street", Value = "My street"}
            };

            var test = builder.Build(patches);
            var watch = new Stopwatch();

            watch.Start();

            for (var i = 0; i < 500000; i++) {
                var user = builder.Build(patches);
            }

            watch.Stop();

            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [Test]
        public void ListTest() {
            var builder = new ObjectBuilder<ObjectWithList>(new TypeTreeCache(new TypeTreeBuilder()), AllValueTypesProvider.ValueTypes);

            var patches = new List<Patch>
            {
                new Patch() {Operation = "add", Path = "/Addresses/0", Value = ""},
                new Patch() {Operation = "add", Path = "/Addresses/1", Value = ""},
                new Patch() {Operation = "add", Path = "/Addresses/0/Users/0", Value = ""},
                new Patch() {Operation = "add", Path = "/Addresses/0/Users/1", Value = ""},
                //new Patch() {Operation = "replace", Path = "/Addresses/0/Street", Value = "1234"},
                //new Patch() {Operation = "replace", Path = "/Addresses/0/Users/0/Name", Value = "1234"},
                new Patch() {Operation = "remove", Path = "/Addresses/0/Users/0", Value = ""}
  
            };

            var watch = new Stopwatch();

            watch.Start();
            for (var i = 0; i < 100000; i++) {
                var user = builder.Build(patches);
            }
            watch.Stop();

            Trace.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (var i = 0; i < 100000; i++)
            {
                var user = new User();
                user.Addresses.Add(new Address());
                user.Addresses[0].Street = "1234";
                user.Addresses[0].Users.Add(new User());
                user.Addresses[0].Users[0].Name = "1234";
                user.Addresses[0].Users.RemoveAt(0);
            }
            watch.Stop();

            Trace.WriteLine(watch.ElapsedMilliseconds);
        }
    }

    public class ObjectWithList {
        public ObjectWithList() {
            Addresses = new List<Address>();
        }
        public IList<Address> Addresses { get; set; }
    }

    public class User {
        public User() {
            Addresses = new List<Address>();
        }
        public string Name { get; set; }
        public Address Address { get; set; }
        public IList<Address> Addresses { get; set; }
    }

    public class Address {
        public Address() {
            Users = new List<User>();
        }
        public string Street { get; set; }
        public int ZipCode { get; set; }

        public IList<User> Users { get; set; }
    }
}
