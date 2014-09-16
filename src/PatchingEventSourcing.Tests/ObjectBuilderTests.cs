using System.Collections.Generic;
using NUnit.Framework;
using PatchingEventSourcing.ValueTypes;

namespace PatchingEventSourcing.Tests {
    [TestFixture]
    public class ObjectBuilderTests {
        [Test]
        public void Simple() {
            var builder = new ObjectBuilder<User>(new TypeTreeCache(new TypeTreeBuilder()), AllValueTypesProvider.ValueTypes);

            var patches = new List<Patch>();

            for (var i = 0; i < 100000; i++)
            {
                patches.Add(new Patch() {Operation = "replace", Path = "/Name", Value = "Niclas"});
                patches.Add(new Patch() {Operation = "replace", Path = "/Address/Street", Value = "My street" });
            }

            var user = builder.Build(patches);
        }
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
        public string Street { get; set; }
        public int ZipCode { get; set; }
    }
}
