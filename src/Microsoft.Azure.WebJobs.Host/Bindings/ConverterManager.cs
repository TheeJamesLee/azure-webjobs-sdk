﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs
{
    // Concrete implementation of IConverterManager
    internal class ConverterManager : IConverterManager
    {
        // Map from <TSrc,TDest> to a converter function. 
        private Dictionary<string, object> _funcsWithAttr = new Dictionary<string, object>();
        private Dictionary<string, object> _funcs = new Dictionary<string, object>();

        public ConverterManager()
        {
            this.AddConverter<byte[], string>(DefaultByteArray2String);
        }

        private static string DefaultByteArray2String(byte[] bytes)
        {
            string str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        private static string GetKey<TSrc, TDest>()
        {
            return typeof(TSrc).FullName + "|" + typeof(TDest).FullName;
        }
        private static string GetKey<TSrc, TDest, TAttribute>()
        {
            return typeof(TSrc).FullName + "|" + typeof(TDest).FullName + "|" + typeof(TAttribute).FullName;
        }

        public void AddConverter<TSrc, TDest>(Func<TSrc, TDest> converter)
        {
            string key = GetKey<TSrc, TDest>();
            _funcs[key] = converter;            
        }

        public void AddConverter<TSrc, TDest, TAttribute>(Func<TSrc, TAttribute, TDest> converter)
            where TAttribute : Attribute
        {
            Func<TSrc, TAttribute, ValueBindingContext, TDest> func = (src, attr, context) => converter(src, attr);
            AddConverterWithContext(func);
        }

        public void AddConverterWithContext<TSrc, TDest, TAttribute>(Func<TSrc, TAttribute, ValueBindingContext, TDest> converter)
            where TAttribute : Attribute
        {
            // $$$
            string key = GetKey<TSrc, TDest, TAttribute>();
            _funcsWithAttr[key] = converter;
        }

        private FuncConverter<TSrc, TAttribute, TDest> TryGetConverter<TSrc, TAttribute, TDest>()
            where TAttribute : Attribute
        {
            string key1 = GetKey<TSrc, TDest, TAttribute>();

            // First try specific that uses the TAttribute 
            object obj;
            if (_funcsWithAttr.TryGetValue(key1, out obj))
            {
                var func = (FuncConverter<TSrc, TAttribute, TDest>)obj;
                return func;
            }

            // Fallback
            string key2 = GetKey<TSrc, TDest>();
            if (_funcs.TryGetValue(key2, out obj))
            {
                var func = (Func<TSrc, TDest>)obj;
                return (TSrc src, TAttribute attr, ValueBindingContext context) => func(src);
            }

            return null;
        }

        public FuncConverter<TSrc, TAttribute, TDest> GetConverter<TSrc, TDest, TAttribute>()
            where TAttribute : Attribute
        {
            // Give precedence to exact matches.
            // this lets callers override any other rules (like JSON binding) 

            // TSrc --> TDest
            var exactMatch = TryGetConverter<TSrc, TAttribute, TDest>();
            if (exactMatch != null)
            {
                return exactMatch;
            }

            // Object --> TDest
            // Catch all for any conversion to TDest
            var objConversion = TryGetConverter<object, TAttribute, TDest>();
            if (objConversion != null)
            {
                return (src, attr, context) =>
                {
                    var result = objConversion(src, attr, context);
                    return result;
                };
            }

            // Inheritence (also covers idempotency)
            if (typeof(TDest).IsAssignableFrom(typeof(TSrc)))
            {
                return (src, attr, context) =>
                {
                    object obj = (object)src;
                    return (TDest)obj;
                };
            }

            // string --> TDest
            var fromString = TryGetConverter<string, TAttribute, TDest>();
            if (fromString == null)
            {
                return null;
            }

            // String --> TDest
            if (typeof(TSrc) == typeof(string))
            {
                return (src, attr, context) =>
                {
                    var result = fromString((string)(object)src, attr, context);
                    return result;
                };
            }

            // Allow some well-defined intermediate conversions. 
            // If this is "wrong" for your type, then it should provide an exact match to override.

            // Byte[] --[builtin]--> String --> TDest
            if (typeof(TSrc) == typeof(byte[]))
            {
                var bytes2string = TryGetConverter<byte[], TAttribute, string>();

                return (src, attr, context) =>
                {
                    byte[] bytes = (byte[])(object)src;
                    string str = bytes2string(bytes, attr, context);
                    var result = fromString(str, attr, context);
                    return result;
                };
            }

            // General JSON serialization rule. 

            if (typeof(TSrc).IsPrimitive ||
               (typeof(TSrc) == typeof(object)) ||
                typeof(IEnumerable).IsAssignableFrom(typeof(TSrc)))
            {
                return null;
            }

            var funcJobj = TryGetConverter<object, TAttribute, JObject>();
            if (funcJobj == null)
            {
                funcJobj = (object obj, TAttribute attr, ValueBindingContext context) => JObject.FromObject(obj);
            }

            // TSrc --[Json]--> string --> TDest
            return (src, attr, context) =>
            {
                JObject jobj = funcJobj((object)src, attr, context);
                string json = jobj.ToString();
                TDest obj = fromString(json, attr, context);
                return obj;
            };
        }
    } // end class ConverterManager
}