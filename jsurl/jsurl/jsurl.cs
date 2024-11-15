using System.Text.RegularExpressions;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections;
using System.Text;
using System.Runtime.Remoting;
using System.Dynamic;

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
            //m.Value = m.Value.charCodeAt(0);
            int ch= (int)m.Value[0];
            // thanks to Douglas Crockford for the negative slice trick
            if (ch < 0x100)
            {
                string val = ("00" + ((int)ch).ToString("x4"));
                return "*" + val.Substring(val.Length - 2);
                //return "*" + ("00" + ch.ToString(16)).slice(-2);
            }
            else
            {
                string val = ("0000" + ((int)ch).ToString("x4"));
                return "**" + val.Substring(val.Length - 4);
                //return "**" + ("0000" + ch.ToString(16)).slice(-4);
            }
        }

        public static string ToString(dynamic data)
        {
            if (data == null)
                return "~null";
            Type t = data.GetType();
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Int32:
                    int result = 0;
                    if (int.TryParse(data?.ToString(), out result))
                        return "~" + result;
                    else
                        return "~null";
                case TypeCode.Boolean:
                    return "~" +  data?.ToString()?.ToLower();
                case TypeCode.String:
                    return "~'" + Encode(data);
                case TypeCode.Object:
                    if(data==null) return "~null";

                    List<string> tmpAry = new List<string>();

                    if (data is Array)
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
                    else if(data is ArrayList)
                    {
                        for (var i = 0; i < data.Count; i++)
                        {
                            tmpAry.Add(ToString(data[i]) );
                        }
                        string text = "";
                        if (tmpAry.Count > 0)
                            text = string.Join("", tmpAry);
                        else
                            text = "~";
                        return "~(" + text + ")";
                    }
                    else
                    {
                        IDictionary<string, object> propertyValues = data;

                        foreach (var prop in propertyValues)
                        {
                            //
                            if(prop.Key != null)
                            {
                                var val = ToString(prop.Value);
                                // skip undefined and functions
                                if (val!=null)
                                {
                                    tmpAry.Add(Encode(prop.Key) + val);
                                }
                            }
                        }

                        return "~(" + string.Join("~", tmpAry)+ ")";
                    }
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

        //public static dynamic Parse(string data)
        //{
        //    if (data == null) return null;
        //    data = Regex.Replace(data, @"/%(25)*27/g", "'");
        //    int i = 0;
        //    int dataLength = data.Length;

        //    i=Expected(data, i, '~');

            
        //}

        public static dynamic Parse(string data)
        {
            int i = 0;
            return Parse(data, ref i);
        }

        private static dynamic Parse(string data,ref int i)
        {
            dynamic result= null;
            if (data == null) return null;
            data = Regex.Replace(data, @"%(25)*27", "'");
            i = Expected(data, i, '~');
            do
            {
                char ch = data[i];
                switch (ch)
                {
                    case '(':
                        i++;
                        if(data[i]== '~')
                        {
                            //this is array
                            result = new List<dynamic>();
                            if (data[i + 1] == ')') { i++; }
                            else
                            {
                                do
                                {
                                    result.Add(Parse(data, ref i));
                                } while (data[i] == '~');
                            }

                        }
                        else
                        {
                            result = new  ExpandoObject() as IDictionary<string, object>;
                            if (data[i] != ')')
                            {
                                do
                                {
                                    var key = GetProperty(data,ref i);
                                    (result as IDictionary<string, object>).Add(key, Parse(data, ref i));
                                    ++i;
                                } while (data[i-1] == '~'&&i<data.Length);
                            }

                        }
                        i = Expected(data, i-1, ')');
                        return result;
                        break;
                    case '\'':
                        i++;
                        result = GetProperty(data, ref i);
                        return result;
                        break;
                    default:
                       int beg= i;
                        //Regex regex = new Regex(@"/[^)~]/");
                        while (i < data.Length && Regex.IsMatch(data[i].ToString(), @"[^)~]"))
                        { 
                            i++;
                        }
                        var sub = data.Substring(beg, i- beg);
                        if (Regex.IsMatch(ch.ToString(), @"[\d\-]"))
                        {
                            result = float.Parse(sub);
                        }else if (sub == "null")
                        {
                            result = null;
                        }
                        else
                        {
                            result = bool.Parse(sub);
                            //if (typeof result === "undefined") throw new Error("bad value keyword: " + sub);
                        }
                        return result;
                        break;
                }

            } while (i < data.Length);

            return result;
        }

        private static string GetPropertyOld(string data, ref int i)
        {
            var beg = i;
            string r = "";
            char ch = data[i];
            while (i< data.Length&& ch!= '~'&& ch != ')')
            {
                switch (ch)
                {
                    case '*':
                        if (beg < i) r += data.Substring(beg, i- beg);
                        if (data[i + 1] == '*')
                        {
                            r += Encoding.ASCII.GetString(FromHex(data.Substring(i + 2, data.Length - (i + 6))));
                            beg = (i += 6);
                        }
                        else
                        {
                            r += Encoding.ASCII.GetString(FromHex(data.Substring(i + 1, data.Length-( i + 3))));
                            beg = (i += 3);
                        }
                        break;
                    case '!':
                        if (beg < i)
                        {
                            r += data.Substring(beg, i- beg);
                            r += '$';
                            beg = ++i;
                        }
                        break;
                    default:
                        i++;
                        break;
                }
                ch = data[i];
            }
            return r + data.Substring(beg, i- beg);
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
                            //r+= String.fromCharCode(parseInt(data.Substring(i + 2, 6), 16)), beg = (i += 6);
                            string text = data.Substring(i + 1, 6);
                            int actual = ParseInteger(text);
                            string textHex = actual.ToString("x4");
                            char charCode = (char)int.Parse(textHex, System.Globalization.NumberStyles.HexNumber);
                            r += charCode;
                            i += 6;
                            beg = i;
                        }
                        else
                        {
                            //r += String.fromCharCode(parseInt(s.substring(i + 1, i + 3), 16)), beg = (i += 3);
                            string text = data.Substring(i, 3);
                            int actual = ParseInteger(text);
                            string textHex = actual.ToString("x4");
                            char charCode = (char)int.Parse(textHex, System.Globalization.NumberStyles.HexNumber);
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
                    default:
                        i++;
                        break;
                }
                ch = data[i];
            }
            return r + data.Substring(beg, i - beg);
        }

        private static readonly Regex LeadingInteger = new Regex(@"^(-?\d+)");
        private static int ParseInteger(string item)
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
            return int.Parse(match.Value);
        }


        private static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
