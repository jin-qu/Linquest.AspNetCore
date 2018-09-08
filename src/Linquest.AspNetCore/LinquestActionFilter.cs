using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Linquest.AspNetCore {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class LinquestActionFilterAttribute : ActionFilterAttribute {

        public LinquestActionFilterAttribute(Type configType = null): base() {
            Order = 0;
            
            if (configType == null) return;

            Config = Activator.CreateInstance(configType) as ILinquestConfig;
            if (Config == null) throw new ArgumentException("Cannot create config instance");
        }

        public LinquestActionFilterAttribute(ILinquestConfig config) => Config = config;

        public ILinquestConfig Config { get; }

        public override void OnResultExecuting(ResultExecutingContext context) {
            base.OnResultExecuting(context);
        }
    }
}
