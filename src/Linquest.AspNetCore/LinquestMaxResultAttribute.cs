using System;

namespace Linquest.AspNetCore {

    public class LinquestMaxResultAttribute : Attribute {

        public LinquestMaxResultAttribute(int count) {
            Count = count;
        }

        public int Count { get; }
    }
}
