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

            //========= Get sub directories Attempt 1 ================================

            //Directory.GetDirectories(@"C:\School\Grad School (Comp Sci)\Thesis\Apps");
            //String[] allfiles = System.IO.Directory.GetFiles(@"C:\School\Grad School (Comp Sci)\Thesis\Apps", "*.*", System.IO.SearchOption.AllDirectories);
            //Debug.WriteLine(allfiles);


            //========= Get sub directories Attempt 2 ================================

            //Debug.WriteLine(Directory.GetDirectories(@"C:\School\Grad School (Comp Sci)\Thesis\Apps"));



            //========= Get sub directories Attempt 3 ================================-

            //var di = new DirectoryInfo("..//..//..//SrcML");
            //var subDirectories = di.GetDirectories(@"C:\School\Grad School (Comp Sci)\Thesis\Apps\accelerometer-app-master");
            //Debug.WriteLine(subDirectories);




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
                //DisplayWhetherAppIsUnitTested();           
                DisplayCallsToOnSensorChanged(globalNamespace);
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


        private void DisplayWhetherAppIsUnitTested()
        {
            throw new NotImplementedException();
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



    }
}
