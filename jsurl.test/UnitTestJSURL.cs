using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [DataRow(1.5, "~1.5")]
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
        public void Test04ToString()
        {
            var expected = @"~(documentId~'5e52732b~val~'Page*201*20stamp~page~1~pageRect~(cx~298.375~system~(height~844.142~origin~'TopLeft)~width~596.75)~parts~(~(config~(id~'b4ff8f92~templateType~(color~'rgb*28255*2c4*2c19*29)~templateTypeId~'a3b011d4)~configId~'b4ff8f92~parts~(~(key~'~rect~(system~(height~844~width~596.75)~x1~501.75)~val~'QUALITY)~(key~'SERIAL~val~'N*2fA))~val~'000SD11N*2fA))~stampRect~(cx~429.625~system~(height~844~origin~'BottomRight~width~596.75)~whr~1.71343873517787)~type~'st~vals~(ser~'N*2fA~ta~'000SD11))";

            string json = "{\"documentId\": \"5e52732b\",\"val\":\"Page 1 stamp\",\"page\": 1,\"pageRect\": {\"cx\": 298.375,\"system\": {\"height\": 844.142,\"origin\": \"TopLeft\"},\"width\": 596.75},\"parts\": [{\"config\": {\"id\": \"b4ff8f92\",\"templateType\": {\"color\": \"rgb(255,4,19)\"},\"templateTypeId\": \"a3b011d4\"},\"configId\": \"b4ff8f92\",\"parts\": [{\"key\": \"\",\"rect\": {\"system\": {\"height\": 844,\"width\": 596.75},\"x1\": 501.75},\"val\": \"QUALITY\"},{\"key\": \"SERIAL\",\"val\": \"N/A\"}],\"val\": \"000SD11N/A\"}],\"stampRect\": {\"cx\": 429.625,\"system\": {\"height\": 844,\"origin\": \"BottomRight\",\"width\": 596.75},\"whr\": 1.71343873517787,},\"type\": \"st\",\"vals\": {\"ser\": \"N/A\",\"ta\": \"000SD11\"}}";

            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);

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
        }

        [TestMethod]
        public void Test07Parse()
        {
            var actual = jsurl.Parse(@"~(documentId~'5e52732b~parts~(~(key~1~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~321.25~x2~538~y1~584.25~y2~710.75)~type~'stampPart~val~'000SD11*20N*2fA))~stampRect~(cx~429.625~cy~647.5~height~126.5~hwr~0.583621683967705~s~27418.875~scale~1~sd~216.75~system~(height~844~origin~'BottomRight~width~596.75)~whr~1.71343873517787~width~216.75~x1~321.25~x2~538~y1~584.25~y2~710.75)~type~'st)");

            string obj1Str = JsonConvert.SerializeObject(actual).Replace("\r\n", "").Replace(" ", "");
            string expected = @"{""documentId"":""5e52732b"",""parts"":[{""key"":1,""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":321.25,""x2"":538,""y1"":584.25,""y2"":710.75},""type"":""stampPart"",""val"":""000SD11 N/A""}],""stampRect"":{""cx"":429.625,""cy"":647.5,""height"":126.5,""hwr"":0.583621683967705,""s"":27418.875,""scale"":1,""sd"":216.75,""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""whr"":1.71343873517787,""width"":216.75,""x1"":321.25,""x2"":538,""y1"":584.25,""y2"":710.75},""type"":""st""}".Replace("\r\n", "").Replace(" ", "");

            Assert.AreEqual(expected.GetHashCode(), obj1Str.GetHashCode());
        }

        [TestMethod]
        public void Test08ParseArrays()
        {
            var actual = jsurl.Parse(@"~(documentId~'5e52732b~page~1~pageRect~(cx~298.375~system~(height~844.142~origin~'TopLeft)~width~596.75)~parts~(~(config~(id~'b4ff8f92~templateType~(color~'rgb*28255*2c4*2c19*29)~templateTypeId~'a3b011d4)~configId~'b4ff8f92~parts~(~(key~'~rect~(system~(height~844~width~596.75)~x1~501.75)~val~'QUALITY)~(key~'SERIAL~val~'N*2fA))~val~'000SD11N*2fA))~stampRect~(cx~429.625~system~(height~844~origin~'BottomRight~width~596.75)~whr~1.71343873517787)~type~'st~vals~(ser~'N*2fA~ta~'000SD11))");

            string obj1Str = JsonConvert.SerializeObject(actual).Replace("\r\n", "").Replace(" ", "");
            string expected = @"{""documentId"":""5e52732b"",""page"":1,""pageRect"":{""cx"":298.375,""system"":{""height"":844.142,""origin"":""TopLeft""},""width"":596.75},""parts"":[{""config"":{""id"":""b4ff8f92"",""templateType"":{""color"":""rgb(255,4,19)""},""templateTypeId"":""a3b011d4""},""configId"":""b4ff8f92"",""parts"":[{""key"":"""",""rect"":{""system"":{""height"":844,""width"":596.75},""x1"":501.75},""val"":""QUALITY""},{""key"":""SERIAL"",""val"":""N/A""}],""val"":""000SD11N/A""}],""stampRect"":{""cx"":429.625,""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""whr"":1.71343873517787},""type"":""st"",""vals"":{""ser"":""N/A"",""ta"":""000SD11""}}".Replace("\r\n", "").Replace(" ", "");

            Assert.AreEqual(expected.GetHashCode(), obj1Str.GetHashCode());
        }


        [TestMethod]
        public void Test09Parse()
        {
            var actual = jsurl.Parse(@"~(documentId~'5e52732b~page~1~pageRect~(cx~298.375~cy~422.071~height~844.142~s~503741.7385~scale~1~system~(height~844.142~origin~'TopLeft~width~596.75)~width~596.75~x1~0~x2~596.75~y1~0~y2~844.142)~parts~(~(config~(id~'b4ff8f92~name~'form~templateType~(color~'rgb*28255*2c*204*2c*2019*29~id~'a3b011d4~name~'ta~sortOrder~0)~templateTypeId~'a3b011d4)~configId~'b4ff8f92~configName~'FORM1~key~1~parts~(~(key~'~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~501.75~x2~538~y1~703.75~y2~710.75)~val~'QUALITY)~(key~'~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~321.25~x2~341.25~y1~694~y2~700.5)~val~'009A)~(key~'~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~431.75~x2~520~y1~584.25~y2~591.75))~(key~'TAG~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~379~x2~423.25~y1~585.25~y2~591.75))~(key~'SERIAL~val~'N*2fA))~rect~(system~(height~844~origin~'BottomRight~width~596.75)~x1~321.25~x2~538~y1~584.25~y2~710.75)~type~'stampPart~val~'000SD11*20N*2fA))~stampRect~(cx~429.625~cy~647.5~height~126.5~hwr~0.583621683967705~s~27418.875~scale~1~sd~216.75~system~(height~844~origin~'BottomRight~width~596.75)~whr~1.71343873517787~width~216.75~x1~321.25~x2~538~y1~584.25~y2~710.75)~type~'st~val~'Page*201~vals~(ser~'N*2fA~ta~'000SD11))");
            
            string obj1Str = JsonConvert.SerializeObject(actual).Replace("\r\n", "").Replace(" ", "");
            string expected = @"{""documentId"":""5e52732b"",""page"":1,""pageRect"":{""cx"":298.375,""cy"":422.071,""height"":844.142,""s"":503741.7385,""scale"":1,""system"":{""height"":844.142,""origin"":""TopLeft"",""width"":596.75},""width"":596.75,""x1"":0,""x2"":596.75,""y1"":0,""y2"":844.142},""parts"":[{""config"":{""id"":""b4ff8f92"",""name"":""form"",""templateType"":{""color"":""rgb(255, 4, 19)"",""id"":""a3b011d4"",""name"":""ta"",""sortOrder"":0},""templateTypeId"":""a3b011d4""},""configId"":""b4ff8f92"",""configName"":""FORM1"",""key"":1,""parts"":[{""key"":"""",""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":501.75,""x2"":538,""y1"":703.75,""y2"":710.75},""val"":""QUALITY""},{""key"":"""",""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":321.25,""x2"":341.25,""y1"":694,""y2"":700.5},""val"":""009A""},{""key"":"""",""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":431.75,""x2"":520,""y1"":584.25,""y2"":591.75}},{""key"":""TAG"",""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":379,""x2"":423.25,""y1"":585.25,""y2"":591.75}},{""key"":""SERIAL"",""val"":""N/A""}],""rect"":{""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""x1"":321.25,""x2"":538,""y1"":584.25,""y2"":710.75},""type"":""stampPart"",""val"":""000SD11 N/A""}],""stampRect"":{""cx"":429.625,""cy"":647.5,""height"":126.5,""hwr"":0.583621683967705,""s"":27418.875,""scale"":1,""sd"":216.75,""system"":{""height"":844,""origin"":""BottomRight"",""width"":596.75},""whr"":1.71343873517787,""width"":216.75,""x1"":321.25,""x2"":538,""y1"":584.25,""y2"":710.75},""type"":""st"",""val"":""Page 1"",""vals"":{""ser"":""N/A"",""ta"":""000SD11""}}".Replace("\r\n", "").Replace(" ", "");

            Assert.AreEqual(expected.GetHashCode(), obj1Str.GetHashCode());
        }

        [TestMethod]
        public void Test11TryParse()
        {
            //NOT IMPLEMENTED
            var actual = jsurl.TryParse("~null",out dynamic result);
            Assert.IsTrue(actual);    
            Assert.IsNull(result);    
            
            actual = jsurl.TryParse("~1", out result);
            Assert.IsTrue(actual);
            Assert.AreEqual(1, result);

            actual = jsurl.TryParse("1", out result);
            Assert.IsFalse(actual);
            Assert.IsNull(result);
        }
    }
}