using System;

namespace Linquest.AspNetCore {

    [AttributeUsage(AttributeTargets.Method)]
    public class LinquestMaxResultAttribute : Attribute {

        public LinquestMaxResultAttribute(int count) {
        }
    }
}
