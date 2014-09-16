namespace PatchingEventSourcing {
    public class PatchPropertyVisitor {

    }

    public class Patch {
        public string Operation { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
    }
}
