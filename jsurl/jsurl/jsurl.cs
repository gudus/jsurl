using System.Text.RegularExpressions;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections;
using System.Text;
using System.Runtime.Remoting;
using System.Dynamic;
using System;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Reflection;

namespace jsurl
{
    public static class jsurl
    {
        private static string Encode(string data)
        {
            Regex regex = new Regex(@"[^\w-.]");
            return regex.Replace(data, new MatchEvaluator(RegexReadTerm));
        }

        private static string RegexReadTerm(Match m)
        {
            if (m.Value == "$") return "!";
            int ch= (int)m.Value[0];
            // thanks to Douglas Crockford for the negative slice trick
            if (ch < 0x100)
            {
                string val = ("00" + ((int)ch).ToString("x4"));
                return "*" + val.Substring(val.Length - 2);
            }
            else
            {
                string val = ("0000" + ((int)ch).ToString("x4"));
                return "**" + val.Substring(val.Length - 4);
            }
        }

        public static string ToString(dynamic data)
        {
            if (data == null)
                return "~null";
            Type t = data.GetType();
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Int32:
                    case TypeCode.Int64:
                    int valueInt = 0;
                    if (int.TryParse(data?.ToString(), out valueInt))
                        return "~" + valueInt;
                    else
                        return "~null";
                case TypeCode.Double:
                    case TypeCode.Decimal:
                    double valueDouble = 0;
                    if (double.TryParse(data?.ToString(), out valueDouble))
                        return "~" + valueDouble;
                    else
                        return "~null";
                case TypeCode.Boolean:
                    return "~" +  data?.ToString()?.ToLower();
                case TypeCode.String:
                    return "~'" + Encode(data);
                case TypeCode.Object:
                    if(data==null) return "~null";

                    List<string> tmpAry = new List<string>();
                    if(data is Guid)
                    {
                        return "~'" + Encode(data.ToString());
                    }
                    else if (data is Array)
                    {
                        for (var i = 0; i < data.Length; i++)
                        {
                            tmpAry.Add(ToString(data[i]));
                        }
                        string text = "";
                        if (tmpAry.Count > 0)
                            text = string.Join("", tmpAry);
                        else
                            text = "~";
                        return "~(" + text + ")";
                    }
                    else if(data is ArrayList || data is IList)
                    {
                        var list = data as IList;
                        if (list!=null)
                        {
                            for (var i = 0; i < list.Count; i++)
                            {
                                tmpAry.Add(ToString(list[i]));
                            }
                            string text = "";
                            if (tmpAry.Count > 0)
                                text = string.Join("", tmpAry);
                            else
                                text = "~";
                            return "~(" + text + ")";
                        }
                    }
                    else
                    {
                        IDictionary<string, object> propertyValues = null;

                        if (!(data is ExpandoObject))
                        {
                            string json= JsonConvert.SerializeObject(data);

                            propertyValues = (IDictionary<string, object>)JsonConvert.DeserializeObject<ExpandoObject>(json);
                        }
                        else
                        {
                            propertyValues = data;
                        }

                        foreach (var prop in propertyValues)
                        {
                            //
                            if (prop.Key != null)
                            {
                                var val = ToString(prop.Value);
                                // skip undefined and functions
                                if (val != null)
                                {
                                    tmpAry.Add(Encode(prop.Key) + val);
                                }
                            }
                        }
                    }
                    return "~(" + string.Join("~", tmpAry)+ ")";
                    
                default:
                    // function, undefined
                    return "";
            }
        }

        private static int Expected(string data, int index, char expect)
        {
            if (data[index] != expect)
                throw new Exception("bad JSURL syntax: expected " + expect + ", got " + data[index]);
            else
                return (index+1);
        }

        public static bool TryParse(string data,out dynamic def)
        {
            try
            {
                def= Parse(data);
            }
            catch (Exception e)
            {
                def = null;
                return false;
            }
            return true;
        }

        public static dynamic Parse(string data)
        {
            int index = 0;
            return Parse(data, ref index);
        }

        private static dynamic Parse(string data,ref int index)
        {
            dynamic result = null;
            data = Regex.Replace(data, @"%(25)*27", "'");
            index = Expected(data, index, '~');
            while (index< data.Length)
            {
                char ch = data[index];
                if (data[index]== '(')
                {
                    index++;
                    if (data[index] == '~')
                    {
                        result = new List<dynamic>();
                        if (data[index + 1] == ')') { index++; }
                        else
                        {
                            do
                            {
                                dynamic r = Parse(data, ref index);
                                if (r != null) result.Add(r);
                            } while (data[index] == '~');
                        }
                    }
                    else
                    {
                        result = new ExpandoObject() as IDictionary<string, object>;
                        if (data[index] != ')')
                        {
                            do
                            {
                                var key = GetProperty(data, ref index);
                                var value = Parse(data, ref index); //Get value
                                if (!(result as IDictionary<string, object>).ContainsKey(key))
                                    (result as IDictionary<string, object>).Add(key, value);
                               
                            } while (data[index] == '~'&& ++index< data.Length);
                        }
                    }
                    index = Expected(data, index, ')');
                    break;
                }
                //this is property value starts
                else if (data[index] == '\'')
                {
                    index++;
                    result = GetProperty(data, ref index);
                    break;
                }
                //object or array ends
                else if (data[index] == ')')
                {
                    index++;
                    break;
                }
                //simple property value
                else
                {
                    int beg = index;
                    while (index < data.Length && Regex.IsMatch(data[index].ToString(), @"[^)~]"))
                    {
                        index++;
                    }
                    var sub = data.Substring(beg, index - beg);
                    if (Regex.IsMatch(ch.ToString(), @"[\d\-]"))
                    {
                        result = decimal.Parse(sub);
                        if (result == Math.Floor(result))
                        {
                            result = (int)result;
                        }

                    }
                    else if (sub == "null")
                    {
                        result = null;
                    }
                    else
                    {
                        bool value = false;
                        if (bool.TryParse(sub, out value))
                            result = value;
                        else
                        {
                            result = sub;
                        }
                    }
                    break;
                }
            }

            return result;
        }

        private static string GetProperty(string data, ref int i)
        {
            int beg = i;
            string r = "";
            char ch = data[i];
            while (i < data.Length&& ch!= '~' && ch != ')')
            {
                switch (ch)
                {
                    case '*':
                        if (beg < i) r += data.Substring(beg, i- beg);
                        if(data[i+1] == '*')
                        {
                            string text = data.Substring(i + 1, 6);
                            int actual = ParseInteger(text,out text);
                            char charCode = (char)actual;
                            r += charCode;
                            i += 6;
                            beg = i;
                        }
                        else
                        {
                            string text = data.Substring(i, 3);
                            int actual = ParseInteger(text, out text);
                            char charCode = (char)actual;
                            r += charCode;
                            i += 3;
                            beg = i;
                        }
                        break;
                    case '!':
                        if (beg < i) r += data.Substring(beg, i - beg);
                        r += '$';
                        beg = ++i;
                        break;
                    //case '(':
                    //    beg++;
                    //    i++;
                    //    break;
                    default:
                        i++;
                        break;
                }
                ch = data[i];
            }
            return r + data.Substring(beg, i - beg);
        }

        private static readonly Regex LeadingInteger = new Regex(@"^[a-zA-Z0-9]+");
        private static int ParseInteger(string item,out string text)
        {
            if (item.StartsWith('*'))
            {
                item = item.Substring(1, item.Length - 1);
            }
            Match match = LeadingInteger.Match(item);
            if (!match.Success)
            {
                throw new ArgumentException("Not an integer");
            }
            text = item;
            return Convert.ToInt32(match.Value, 16); 
        }
    }
}
