namespace Linquest.AspNetCore {

    public class LinquestParameter {

        public LinquestParameter(string name, string value) {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }
    }
}