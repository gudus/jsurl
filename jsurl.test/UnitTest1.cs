using Newtonsoft.Json;
using System.Collections;
using System.Dynamic;
using static System.Net.Mime.MediaTypeNames;

namespace jsurl.test
{
    [TestClass]
    public class UnitTestJSURL
    {
        [DataTestMethod]
        [DataRow(null, "~null")]
        [DataRow(false, "~false")]
        [DataRow(true, "~true")]
        [DataRow(0, "~0")]
        [DataRow(1, "~1")]
        
        [DataRow("hello world\u203c", "~'hello*20world**203c")]
        [DataRow(" !\"#$%&'()*+,-./09:;<=>?@AZ[\\]^_`az{|}~", "~'*20*21*22*23!*25*26*27*28*29*2a*2b*2c-.*2f09*3a*3b*3c*3d*3e*3f*40AZ*5b*5c*5d*5e_*60az*7b*7c*7d*7e")]
        public void Test01BasicValues(dynamic text, string expected)
        {
            string actual = jsurl.ToString(text);

            Assert.AreEqual(expected, actual);
        }

        

        [TestMethod]
        public void Test02Arrays()
        {
            ArrayList array = new ArrayList();
            string actual = jsurl.ToString(array);

            string expected = "~(~)";
            Assert.AreEqual(expected, actual);

            expected = "~(~null~false~0~'hello*20world**203c)";
            array.Add(null);
            array.Add(false);
            array.Add(0);
            array.Add("hello world\u203c");

            actual = jsurl.ToString(array);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Test03Objects()
        {
            dynamic obj = new ExpandoObject();

            string expected = "~()";
            string actual = jsurl.ToString(obj);

            Assert.AreEqual(expected, actual);

            obj.c = null;
            obj.d = false;
            obj.e = 0;
            obj.f = "hello world\u203c";


            expected = "~(c~null~d~false~e~0~f~'hello*20world**203c)";
            actual = jsurl.ToString(obj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Test04Mix()
        {
            dynamic obj = new ExpandoObject();

            dynamic objTest1 = new ExpandoObject();
            dynamic objTest2 = new ExpandoObject();
            var testArray1 = new ArrayList() { 1, 2 };
            var testArray2 = new ArrayList();

            objTest2.d = "hello";
            objTest2.e = objTest1;
            objTest2.f = testArray2;
           
            obj.a = new ArrayList()
            {
                testArray1,testArray2,objTest1
            };
            obj.b = testArray2;
            obj.c = objTest2;

            string expected = "~(a~(~(~1~2)~(~)~())~b~(~)~c~(d~'hello~e~()~f~(~)))";
            string actual = jsurl.ToString(obj);

            Assert.AreEqual(expected, actual);


        }

        [TestMethod]
        public void Test05Parse()
        {
            var actual = jsurl.Parse("~(a~%27hello~b~%27world)");
            dynamic obj = new ExpandoObject();
            obj.a = "hello";
            obj.b = "world";

            var obj1Str = JsonConvert.SerializeObject(actual);
            var obj2Str = JsonConvert.SerializeObject(obj);
            Assert.AreEqual(obj1Str, obj2Str);
            //Assert.AreEqual(obj, actual);
        }

        [TestMethod]
        public void Test06Parse()
        {
            var actual = jsurl.Parse("~(a~%2527hello~b~%2525252527world)");
            dynamic obj = new ExpandoObject();
            obj.a = "hello";
            obj.b = "world";

            var obj1Str = JsonConvert.SerializeObject(actual);
            var obj2Str = JsonConvert.SerializeObject(obj);
            Assert.AreEqual(obj1Str, obj2Str);
            //Assert.AreEqual(obj, actual);
        }

        [TestMethod]
        public void Test07TryParse()
        {
            //NOT IMPLEMENTED
            var actual = jsurl.Parse("~null");
            Assert.IsNull(actual);    
            
            actual = jsurl.Parse("~1");
            Assert.AreEqual(1,actual);

            actual = jsurl.Parse("1");
            Assert.AreEqual(null, actual);
        }
    }
}