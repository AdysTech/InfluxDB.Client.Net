using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net.Core.Extensions
{
   public static class DictionaryHelper
   {
		/// <summary>
		/// Extension method that turns a dictionary of string and object to an ExpandoObject
		/// </summary>
		public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
		{
			var expando = new ExpandoObject();
			var expandoDic = (IDictionary<string, object>)expando;

			// go through the items in the dictionary and copy over the key value pairs)
			foreach (var kvp in dictionary)
			{
				// if the value can also be turned into an ExpandoObject, then do it!
				if (kvp.Value is IDictionary<string, object>)
				{
					var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
					expandoDic.Add(kvp.Key, expandoValue);
				}
				else if (kvp.Value is ICollection)
				{
					// iterate through the collection and convert any string-object dictionaries
					// along the way into expando objects
					var itemList = new List<object>();
					foreach (var item in (ICollection)kvp.Value)
					{
						if (item is IDictionary<string, object>)
						{
							var expandoItem = ((IDictionary<string, object>)item).ToExpando();
							itemList.Add(expandoItem);
						}
						else
						{
							itemList.Add(item);
						}
					}

					expandoDic.Add(kvp.Key, itemList);
				}
				else
				{
					expandoDic.Add(kvp);
				}
			}

			return expando;
		}

		public static IReadOnlyList<dynamic> ToExpando<T>(this IReadOnlyList<T> readonlyDictionaryList)
      {
			IReadOnlyList<IDictionary<string, object>> readonlyDictionaryListTypes = readonlyDictionaryList as IReadOnlyList<IDictionary<string, object>>;
			if (readonlyDictionaryListTypes != null)
         {
				return readonlyDictionaryListTypes.Select(item => item.ToExpando()).ToList().AsReadOnly();
			}
			return null;
		}
	}
}