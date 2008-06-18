#region License

/*
 * Copyright 2004 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Util;

#endregion

namespace Spring.Reflection.Dynamic
{
	/// <summary>
    /// Unit tests for the DynamicMethod class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
	[TestFixture]
    public sealed class DynamicMethodTests
    {
        private Inventor tesla;
        private Inventor pupin;
        private Society ieee;

	    #region SetUp and TearDown

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ContextRegistry.Clear();
            tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            tesla.Inventions = new string[]
                {
                    "Telephone repeater", "Rotating magnetic field principle",
                    "Polyphase alternating-current system", "Induction motor",
                    "Alternating-current power transmission", "Tesla coil transformer",
                    "Wireless communication", "Radio", "Fluorescent lights"
                };
            tesla.PlaceOfBirth.City = "Smiljan";

            pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian");
            pupin.Inventions = new string[] { "Long distance telephony & telegraphy", "Secondary X-Ray radiation", "Sonar" };
            pupin.PlaceOfBirth.City = "Idvor";
            pupin.PlaceOfBirth.Country = "Serbia";

            ieee = new Society();
            ieee.Members.Add(tesla);
            ieee.Members.Add(pupin);
            ieee.Officers["president"] = pupin;
            ieee.Officers["advisors"] = new Inventor[] { tesla, pupin }; // not historically accurate, but I need an array in the map ;-)
        }
	    
        [TestFixtureTearDown]
        public void TearDown()
        {
            //DynamicReflectionManager.SaveAssembly();
        }
	    
        #endregion
        
	    [Test]
        public void TestInstanceMethods() 
        {
            IDynamicMethod getAge = DynamicMethod.Create(typeof(Inventor).GetMethod("GetAge"));
            Assert.AreEqual(tesla.GetAge(DateTime.Today), getAge.Invoke(tesla, new object[] {DateTime.Today}));

	        MethodTarget target = new MethodTarget();
            IDynamicMethod test = DynamicMethod.Create(typeof(MethodTarget).GetMethod("MethodReturningString"));
            Assert.AreEqual(tesla.Name, test.Invoke(target, new object[] { 5, DateTime.Today, new String[] {"xyz", "abc"}, tesla}));

	        ArrayList list = new ArrayList(new string[] {"one", "two", "three"});
            IDynamicMethod removeAt = DynamicMethod.Create(typeof(ArrayList).GetMethod("RemoveAt"));
            removeAt.Invoke(list, new object[] {1});
	        Assert.AreEqual(2, list.Count);
	        Assert.AreEqual("three", list[1]);
        }

	    [Test]
        public void TestStaticMethods()
        {
            IDynamicMethod isNullOrEmpty = DynamicMethod.Create(typeof(StringUtils).GetMethod("IsNullOrEmpty"));
            Assert.IsTrue((bool) isNullOrEmpty.Invoke(null, new object[] { null }));
            Assert.IsTrue((bool) isNullOrEmpty.Invoke(null, new object[] { String.Empty }));
            Assert.IsFalse((bool) isNullOrEmpty.Invoke(null, new object[] { "Ana Maria" }));
        }

	    [Test]
        public void TestRefOutMethods()
	    {
            IDynamicMethod refMethod = DynamicMethod.Create(typeof(MethodTarget).GetMethod("MethodWithRefParameter"));

            MethodTarget target = new MethodTarget();
            object[] args = new object[] {"aleks", 5};
	        refMethod.Invoke(target, args);
            Assert.AreEqual("ALEKS", args[0]);
            Assert.AreEqual(25, args[1]);

            IDynamicMethod outMethod = DynamicMethod.Create(typeof(MethodTarget).GetMethod("MethodWithOutParameter"));
            args = new object[] { "aleks", null };
            outMethod.Invoke(target, args);
            Assert.AreEqual("ALEKS", args[1]);

            IDynamicMethod refOutMethod = DynamicMethod.Create(typeof(RefOutTestObject).GetMethod("DoIt"));
            RefOutTestObject refOutTarget = new RefOutTestObject();

            args = new object[] { 0, 1, null };
            refOutMethod.Invoke(refOutTarget, args);
            Assert.AreEqual(2, args[1]);
            Assert.AreEqual("done", args[2]);
            refOutMethod.Invoke(refOutTarget, args);
            Assert.AreEqual(3, args[1]);
            Assert.AreEqual("done", args[2]);

	        int count = 0;
	        string done;
            target.DoItCaller(0, ref count, out done);
            Assert.AreEqual(1, count);
            Assert.AreEqual("done", done);
        }
	    
        #region Performance tests

        private DateTime start, stop;

        //[Test]
        public void PerformanceTests()
        {
            int n = 10000000;
            object x = null;

            // tesla.GetAge
            start = DateTime.Now;
            for (int i = 0; i < n; i++)
            {
                x = tesla.GetAge(DateTime.Today);
            }
            stop = DateTime.Now;
            PrintTest("tesla.GetAge (direct)", n, Elapsed);

            start = DateTime.Now;
            IDynamicMethod getAge = DynamicMethod.Create(typeof(Inventor).GetMethod("GetAge"));
            for (int i = 0; i < n; i++)
            {
                object[] args = new object[] { DateTime.Today };
                x = getAge.Invoke(tesla, args);
            }
            stop = DateTime.Now;
            PrintTest("tesla.GetAge (dynamic reflection)", n, Elapsed);

            start = DateTime.Now;
            MethodInfo getAgeMi = typeof(Inventor).GetMethod("GetAge");
            for (int i = 0; i < n; i++)
            {
                object[] args = new object[] { DateTime.Today };
                x = getAgeMi.Invoke(tesla, args);
            }
            stop = DateTime.Now;
            PrintTest("tesla.GetAge (standard reflection)", n, Elapsed);
        }

        private double Elapsed
        {
            get { return (stop.Ticks - start.Ticks) / 10000000f; }
        }

        private void PrintTest(string name, int iterations, double duration)
        {
            Debug.WriteLine(String.Format("{0,-60} {1,12:#,###} {2,12:##0.000} {3,12:#,###}", name, iterations, duration, iterations / duration));
        }

        #endregion
	    
    }

    #region IL generation helper classes (they help if you look at them in Reflector ;-)

    public class InstanceMethod : IDynamicMethod
    {
        public object Invoke(object target, object[] args)
        {
            return ((MethodTarget) target).MethodReturningString(
                (int) args[0], (DateTime) args[1], (string[]) args[2], (Inventor) args[3]);
        }

        public object InvokeVoid(object target, object[] args)
        {
            ((MethodTarget) target).RemoveAt(5);
            return null;
        }

        public object InvokeWithOut(object target, object[] args)
        {
            string outVar = null;
            ((MethodTarget)target).MethodWithOutParameter((string) args[0], out outVar);
            args[1] = outVar;
            return null;
        }

        public object InvokeWithRef(object target, object[] args)
        {
            string refVar1 = (string) args[0];
            int refVar2 = (int) args[0];
            ((MethodTarget)target).MethodWithRefParameter(ref refVar1, ref refVar2);
            args[0] = refVar1;
            args[1] = refVar2;
            return null;
        }

        public object InvokeDoIt(object target, object[] args)
        {
            int reference = (int) args[1];
            string output;
            ((RefOutTestObject) target).DoIt((int) args[0], ref reference, out output);
            args[1] = reference;
            args[2] = output;
            return null;
        }
    }

    public class MethodTarget
    {
        public string MethodReturningString(int arg1, DateTime arg2, string[] arg3, Inventor arg4)
        {
            return arg4.Name;
        }
        
        public void RemoveAt(int index)
        {}

        public void MethodWithOutParameter(string lower, out string upper)
        {
            upper = lower.ToUpper();
        }

        public void MethodWithRefParameter(ref string lowerUpper, ref int square)
        {
            lowerUpper = lowerUpper.ToUpper();
            square = square * square;
        }

        public void DoItCaller(int count, ref int reference, out string output)
        {
            InstanceMethod caller = new InstanceMethod();

            RefOutTestObject target = new RefOutTestObject();
            object[] args = new object[] {count, reference, null};
            caller.InvokeDoIt(target, args);

            reference = (int) args[1];
            output = (string) args[2];
        }
    }

    public interface IRefOutTestObject
    {
        void DoIt(int count, ref int reference, out string output);
    }

    public class RefOutTestObject : IRefOutTestObject
    {
        public void DoIt(int count, ref int reference, out string output)
        {
            output = "done";
            reference++;
        }
    }

    #endregion
 
   
}