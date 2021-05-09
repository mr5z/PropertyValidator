using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PropertyValidator.Extensions
{
    static class ObjectExtension
	{
		public static T ToObject<T>(this IDictionary<string, object> source)
			where T : class, new() => ToObject<T, object>(source);

		public static T ToObject<T>(this IDictionary<string, string> source)
			where T : class, new() => ToObject<T, string>(source);

		public static object ToObject(this IDictionary<string, object> source, Type type)
			=> ToObject<object>(source, type);

		private static TReturnType ToObject<TReturnType, TValue>(IDictionary<string, TValue> source)
			where TReturnType : class, new() => (TReturnType)ToObject(source, typeof(TReturnType));

		private static object ToObject<TValue>(IDictionary<string, TValue> source, Type type)
		{
			var obj = Activator.CreateInstance(type);
			foreach (var item in source)
			{
				type.GetProperty(item.Key)
					.SetValue(obj, item.Value, null);
			}
			return obj;
		}

		public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
		{
			return source.GetType().GetProperties(bindingAttr).ToDictionary
			(
				propInfo => propInfo.Name,
				propInfo => propInfo.GetValue(source, null)
			);
		}
	}
}
