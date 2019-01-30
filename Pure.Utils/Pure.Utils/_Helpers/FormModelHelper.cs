using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pure.Utils
{
    public class FormModelHelper
    {

        private static T Bind<T>(Func<string, object> getValue) where T : new()
        {

            T t = new T();

            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                if (!pi.CanWrite)
                    continue;
                object value = getValue(pi.Name);// context.Request[pi.Name];
                if (value != null && value != DBNull.Value)
                {
                    try
                    {
                        if (value.ToString() != "")
                            pi.SetValue(t, Convert.ChangeType(value, pi.PropertyType), null);
                        else
                            pi.SetValue(t, value, null);
                    }
                    catch
                    {
                        try
                        {
                            if (pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(Decimal))
                            {
                                if (value.ToString() != "")
                                    pi.SetValue(t, Convert.ToDecimal(value), null);
                                else
                                    pi.SetValue(t, value, null);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return t;
        }
   

        public static T ConvertToModel<T>(Microsoft.AspNetCore.Http.HttpContext  context) where T : new()
        {
            return Bind<T>(str =>
            {
                return WebRequestHelper.RequestValue(str, context);// context.Request[str];
            });
        }

        static Dictionary<string, object> NvcToDictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }

        //public static Dictionary<string, object> GetRequestParams( )
        //{
        //    return GetRequestParams(HttpContext.Current);
        //}
        public static Dictionary<string, object> GetRequestParams(Microsoft.AspNetCore.Http.HttpContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (context == null)
            {
                return result;
            }
            var request = context.Request;
            Func<Func<HttpRequest, IQueryCollection>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    var resultNV = new NameValueCollection();
                    foreach (var item in getter(request))
                    {
                        resultNV.Add(item.Key, item.Value);
                    }

                    return resultNV;

                }
                catch (Exception e)
                {
                    throw;
                }
            };
            Func<Func<HttpRequest, IFormCollection>, NameValueCollection> tryGetCollectionForm = getter =>
            {
                try
                {
                    var resultNV = new NameValueCollection();
                    foreach (var item in getter(request))
                    {
                        resultNV.Add(item.Key, item.Value);
                    }

                    return resultNV;

                }
                catch (Exception e)
                {
                    throw;

                }
            };
         
            //_serverVariables = tryGetCollectionServerVars(r => r.GetServerVariables());
            var _queryString = tryGetCollection(r => r.Query);
            var _formString = new NameValueCollection();
            if (request.HasFormParams())
            {
                _formString = tryGetCollectionForm(r => r.Form);

            }
            //var _queryString = tryGetCollection(r => r.QueryString);
            //var _formString = tryGetCollection(r => r.Form);

            var _query = NvcToDictionary(_queryString, true);
            var _form = NvcToDictionary(_formString, true);
            foreach (var item in _query)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value );
                }
            }
            foreach (var item in _form)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value );
                }
            }

            return result;
        }

    }
}
