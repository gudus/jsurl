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
        public void Test07Parse()
        {
            var actual = jsurl.Parse("~(type~'stamp~documentId~'5e52732b-50a3-4925-9abb-6e7810e4defc~val~'Page*201*20stamp~vals~(TAG~'234-**0442**0442-00223~SERIAL~'N*2fA)~page~1~pageRect~(x1~0~x2~596.75~y1~0~y2~844.142~width~596.75~height~844.142~scale~1~cx~298.375~cy~422.071~s~503741.73850000004~system~(origin~'TopLeft~width~596.75~height~844.142))~stampRect~(x1~321.25~x2~538~y1~584.25~y2~710.75~width~216.75~height~126.5~scale~1~cx~429.625~cy~647.5~s~27418.875~whr~1.7134387351778657~hwr~0.5836216839677048~sd~216.75~system~(origin~'BottomRight~width~596.75~height~844))~parts~(~(key~1~type~'stampPart~val~'234-**0442**0442-00223*20N*2fA~rect~(x1~321.25~y1~584.25~x2~538~y2~710.75~system~(origin~'BottomRight~width~596.75~height~844))~config~(id~'b4ff8f92-d1be-466b-8b34-40e74aa3530b~name~'ICF*20009A*20W~templateType~(id~'a3b011d4-25c9-44a7-8ab6-002ae4d79658~name~'Tag~color~'rgb*28255*2c*204*2c*2019*29~sortOrder~0)~templateTypeId~'a3b011d4-25c9-44a7-8ab6-002ae4d79658)~configId~'b4ff8f92-d1be-466b-8b34-40e74aa3530b~configName~'ICF*20009A*20W~parts~(~(key~'~val~'QUALITY~rect~(x1~501.75~y1~703.75~x2~538~y2~710.75~system~(origin~'BottomRight~width~596.75~height~844)))~(key~'~val~'009A~rect~(x1~321.25~y1~694~x2~341.25~y2~700.5~system~(origin~'BottomRight~width~596.75~height~844)))~(key~'~rect~(x1~431.75~y1~584.25~x2~520~y2~591.75~system~(origin~'BottomRight~width~596.75~height~844)))~(key~'TAG~rect~(x1~379~y1~585.25~x2~423.25~y2~591.75~system~(origin~'BottomRight~width~596.75~height~844)))~(key~'SERIAL~val~'N*2fA)))))");
            
            var obj1Str = JsonConvert.SerializeObject(actual);
            var obj2Str = JsonConvert.SerializeObject(@"");
            string expected = @"{
    ""documentId"": ""5e52732b-50a3-4925-9abb-6e7810e4defc"",
    ""page"": 1,
    ""pageRect"": {
        ""cx"": 298.375,
        ""cy"": 422.071,
        ""height"": 844.142,
        ""s"": 503741.7385,
        ""scale"": 1,
        ""system"": {
            ""height"": 844.142,
            ""origin"": ""TopLeft"",
            ""width"": 596.75
        },
        ""width"": 596.75,
        ""x1"": 0,
        ""x2"": 596.75,
        ""y1"": 0,
        ""y2"": 844.142
    },
    ""parts"": [
        {
            ""config"": {
                ""id"": ""b4ff8f92-d1be-466b-8b34-40e74aa3530b"",
                ""name"": ""ICF 009A W"",
                ""templateType"": {
                    ""color"": ""rgb(255, 4, 19)"",
                    ""id"": ""a3b011d4-25c9-44a7-8ab6-002ae4d79658"",
                    ""name"": ""Tag"",
                    ""sortOrder"": 0
                },
                ""templateTypeId"": ""a3b011d4-25c9-44a7-8ab6-002ae4d79658""
            },
            ""configId"": ""b4ff8f92-d1be-466b-8b34-40e74aa3530b"",
            ""configName"": ""ICF 009A W"",
            ""key"": 1,
            ""parts"": [
                {
                    ""key"": """",
                    ""rect"": {
                        ""system"": {
                            ""height"": 844,
                            ""origin"": ""BottomRight"",
                            ""width"": 596.75
                        },
                        ""x1"": 501.75,
                        ""x2"": 538,
                        ""y1"": 703.75,
                        ""y2"": 710.75
                    },
                    ""val"": ""QUALITY""
                },
                {
                    ""key"": """",
                    ""rect"": {
                        ""system"": {
                            ""height"": 844,
                            ""origin"": ""BottomRight"",
                            ""width"": 596.75
                        },
                        ""x1"": 321.25,
                        ""x2"": 341.25,
                        ""y1"": 694,
                        ""y2"": 700.5
                    },
                    ""val"": ""009A""
                },
                {
                    ""key"": """",
                    ""rect"": {
                        ""system"": {
                            ""height"": 844,
                            ""origin"": ""BottomRight"",
                            ""width"": 596.75
                        },
                        ""x1"": 431.75,
                        ""x2"": 520,
                        ""y1"": 584.25,
                        ""y2"": 591.75
                    }
                },
                {
                    ""key"": ""TAG"",
                    ""rect"": {
                        ""system"": {
                            ""height"": 844,
                            ""origin"": ""BottomRight"",
                            ""width"": 596.75
                        },
                        ""x1"": 379,
                        ""x2"": 423.25,
                        ""y1"": 585.25,
                        ""y2"": 591.75
                    }
                },
                {
                    ""key"": ""SERIAL"",
                    ""val"": ""N/A""
                }
            ],
            ""rect"": {
                ""system"": {
                    ""height"": 844,
                    ""origin"": ""BottomRight"",
                    ""width"": 596.75
                },
                ""x1"": 321.25,
                ""x2"": 538,
                ""y1"": 584.25,
                ""y2"": 710.75
            },
            ""type"": ""stampPart"",
            ""val"": ""234-тт-00223 N/A""
        }
    ],
    ""stampRect"": {
        ""cx"": 429.625,
        ""cy"": 647.5,
        ""height"": 126.5,
        ""hwr"": 0.583621683967705,
        ""s"": 27418.875,
        ""scale"": 1,
        ""sd"": 216.75,
        ""system"": {
            ""height"": 844,
            ""origin"": ""BottomRight"",
            ""width"": 596.75
        },
        ""whr"": 1.71343873517787,
        ""width"": 216.75,
        ""x1"": 321.25,
        ""x2"": 538,
        ""y1"": 584.25,
        ""y2"": 710.75
    },
    ""type"": ""stamp"",
    ""val"": ""Page 1 stamp"",
    ""vals"": {
        ""SERIAL"": ""N/A"",
        ""TAG"": ""234-тт-00223""
    }
}";
            Assert.AreEqual(expected, obj1Str);
            //Assert.AreEqual(obj, actual);
        }

        [TestMethod]
        public void Test08TryParse()
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