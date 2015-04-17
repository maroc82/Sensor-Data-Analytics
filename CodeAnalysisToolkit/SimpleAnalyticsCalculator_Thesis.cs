using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML;
using ABB.SrcML.Data;
using NUnit.Framework;
using System.Collections;

namespace CodeAnalysisToolkit
{
    [TestFixture]
    public class SimpleAnalyticsCalculator_Thesis
    {
        //------Test Case Class------------------------------------------------------------------------

        [TestCase]
        public void CalculateSimpleProjectStats()
        {
            int NumOfApps = 30;


            //-----------Current Working Method to Get sub directories -------------------------

            // Get list of files in the specific directory.
            string[] TopDirectories = Directory.GetDirectories(@"C:\School\Grad School (Comp Sci)\Thesis\Apps\",
                "*.*",
                SearchOption.TopDirectoryOnly);

            // Display all the files.
            //for (int i = 0; i <= NumOfApps; i++)
            //{
            //    Console.WriteLine(TopDirectories[i]);
            //}

            //Print out all Top Sub Directoies for Specified Path 
            //foreach (string file in TopDirectories)
            //{
            //    Console.WriteLine(file);
            //}

            //----------End of Print Sub directory Method----------------------------------------


            for (int i = 0; i < NumOfApps; i++)
            {
                var dataProject = new DataProject<CompleteWorkingSet>(TopDirectories[i],
                    Path.GetFullPath(TopDirectories[i]),
                    "..//..//..//SrcML");

                Console.WriteLine();
                Debug.WriteLine("#############################################");
                Debug.WriteLine("Parsing " + TopDirectories[i]);
                            
                dataProject.UpdateAsync().Wait();

                NamespaceDefinition globalNamespace;
                Assert.That(dataProject.WorkingSet.TryObtainReadLock(5000, out globalNamespace));

                DisplaySensorTypes(globalNamespace);
                //DisplayWhetherAppIsUnitTested(globalNamespace);           
                DisplayCallsToOnSensorChanged(globalNamespace);
                //GetTypeForKeyword(globalNamespace);
                DisplayTestCaseClasses(globalNamespace);

            }
        }


        //-------Display Sensor Type Class--------------------------------------------------------------

        private void DisplaySensorTypes(NamespaceDefinition globalNamespace)
        {
            var getDefaultSensorCalls = from statement in globalNamespace.GetDescendantsAndSelf()
                                        from expression in statement.GetExpressions()
                                        from call in expression.GetDescendantsAndSelf<MethodCall>()                                     
                                        where call.Name == "getDefaultSensor"
                                        select call;

            foreach (var call in getDefaultSensorCalls)
            {
                if (call.Arguments.Any())
                {
                    var firstArg = call.Arguments.First();
                    var components = firstArg.Components;
                    if (components.Count() == 3 &&
                        components.ElementAt(0).ToString() == "Sensor" &&
                        components.ElementAt(1).ToString() == ".")
                    {
                        Debug.WriteLine("sensor " + components.ElementAt(2).ToString() + " found");
                    }
                }
            }
        }

        //-------Display If this class has a Unit test--------------------------------------------------------------

        private void DisplayWhetherAppIsUnitTested(NamespaceDefinition globalNamespace)
        {
            var testClasses = from klas in globalNamespace.GetDescendants<TypeDefinition>()
                where klas.GetParentTypes(false).Any(t => t.Name == "ServiceTestCase")
                select klas;        
        
            if (testClasses.Count() == 0)
            {
                Debug.WriteLine("This File Does not contain any tests");
            }
            else
            {

                Debug.WriteLine("----- ");
                Debug.WriteLine("\r\n");
                Debug.WriteLine(testClasses.Count() + " TestClasses ");
                Debug.WriteLine("----- ");

                foreach(var testClass in testClasses)
                {
                    Debug.WriteLine(testClass.GetFullName() + " is a test class");
                }
            }
        }




        //-------Display If ActivityUnitTestCase test--------------------------------------------------------------

        private void DisplayTestCaseClasses(NamespaceDefinition globalNamespace)
        {
            var testClasses = from klas in globalNamespace.GetDescendants<TypeDefinition>()
                              where klas.ParentTypeNames.Any(t => t.Name.Contains("ActivityUnitTestCase") || 
                                                                  t.Name.Contains("ServiceTestCase") ||
                                                                  t.Name.Contains("ApplicationTestCase") ||
                                                                  t.Name.Contains("ProviderTestCase2") ||
                                                                  t.Name.Contains("LoaderTestCase") || 
                                                                  t.Name.Contains("ActivityInstrumentationTestCase2"))
                              select klas;

            if (testClasses.Count() == 0)
            {
                Debug.WriteLine("This File Does not contain any test case classes");
            }
            else
            {

                Debug.WriteLine("----- ");
                Debug.WriteLine("\r\n");
                Debug.WriteLine(testClasses.Count() + " Test Classes found ");
                Debug.WriteLine("----- ");

                foreach (var testClass in testClasses)
                {
                    Debug.WriteLine(testClass.GetFullName());
                    //foreach(var parent in testClass.ParentTypeNames)
                    //{
                    //    Debug.WriteLine("parent: " + parent);
                    //}
                }
            }
        }




        //-------Display Calls to OnSensorChanged Class------------------------------------------------

        private void DisplayCallsToOnSensorChanged(NamespaceDefinition globalNamespace)
        {
            var senChangedMethods = from method in globalNamespace.GetDescendants<MethodDefinition>()
                where method.Name == "onSensorChanged"
                select method;

            if (senChangedMethods.Count() == 0)
            {
                Debug.WriteLine("This File Does not contain any Sensor Change Mehtods");
            }

            else
            {

                Debug.WriteLine("----- ");
                Debug.WriteLine("\r\n");
                Debug.WriteLine(senChangedMethods.Count() + " Implementations of " + senChangedMethods.First().GetFullName());
                Debug.WriteLine("----- ");

                int n = senChangedMethods.Count();
                for (int i = 0; i < n; i++)
                {
                    var senChangedMethod = senChangedMethods.ElementAt(i);
                    Debug.WriteLine("Implementations of onSensorChaged # " + (i + 1) + ": " + senChangedMethod.GetFullName());

                    //"GetCallsToSelf" returns the number of times the number is called
                    var callsToSenChanged = senChangedMethod.GetCallsToSelf();
                    for (int j = 0; j < callsToSenChanged.Count(); j++)
                    {
                        var callerMethod = callsToSenChanged.ElementAt(j).ParentStatement.GetAncestorsAndSelf<MethodDefinition>();
                        if (callerMethod.Any())
                        {
                            Debug.WriteLine("   Called by --> " + callerMethod.ElementAt(0).GetFullName());
                        }
                    }
                    //Debug.WriteLine("----- ");
                }
            }  //End of Else does not Equal 0 Check
        }



        //-------Display Test Class--------------------------------------------------------------

        private void GetTypeForKeyword(NamespaceDefinition use)
        {
            //if (use == null) { throw new ArgumentNullException("use"); }
            //if (use.ParentStatement == null)
            //{
            //    throw new ArgumentException("ParentStatement is null", "use");
            //}

            //if (use.Name == "ServiceTestCase")
            //{
            //    //return the surrounding type definition
            //    Debug.WriteLine(use.ParentStatement.GetAncestorsAndSelf<TypeDefinition>().Take(1));
            //}
            
            //    //return all the parent classes of the surrounding type definition
            //    var enclosingType = use.ParentStatement.GetAncestorsAndSelf<TypeDefinition>().FirstOrDefault();
            //    if (enclosingType == null)
            //    {
            //        Debug.WriteLine(Enumerable.Empty<TypeDefinition>());
            //    }
            //    else
            //    {
            //        Debug.WriteLine(enclosingType.GetParentTypes(true));
            //    }
            //}

            //Debug.WriteLine(Enumerable.Empty<TypeDefinition>());
        }


    }
}
